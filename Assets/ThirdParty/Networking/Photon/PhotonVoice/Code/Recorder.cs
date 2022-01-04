// ----------------------------------------------------------------------------
// <copyright file="Recorder.cs" company="Exit Games GmbH">
//   Photon Voice for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// Component representing outgoing audio stream in scene.
// </summary>
// <author>developer@photonengine.com</author>
// ----------------------------------------------------------------------------

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
#define PHOTON_MICROPHONE_ENUMERATOR
#endif

using System;
using System.Linq;
using POpusCodec.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Photon.Voice.Unity
{
    /// <summary>
    /// Component representing outgoing audio stream in scene.
    /// </summary>
    [AddComponentMenu("Photon Voice/Recorder")]
    [HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/recorder")]
    [DisallowMultipleComponent]
    public class Recorder : VoiceComponent
    {
        public const int MIN_OPUS_BITRATE = 6000;
        public const int MAX_OPUS_BITRATE = 510000;

        #region Private Fields

        private static readonly Array samplingRateValues = Enum.GetValues(typeof(SamplingRate));

        [SerializeField]
        private bool voiceDetection;

        [SerializeField]
        private float voiceDetectionThreshold = 0.01f;

        [SerializeField]
        private int voiceDetectionDelayMs = 500;

        private object userData;

        private LocalVoice voice = LocalVoiceAudioDummy.Dummy;

        #if UNITY_EDITOR
        [SerializeField]
        #endif
        private string unityMicrophoneDevice;

        #if UNITY_EDITOR
        [SerializeField]
        #endif
        private int photonMicrophoneDeviceId = -1;

        private IAudioDesc inputSource;

        private VoiceClient client;

        [SerializeField]
        [FormerlySerializedAs("audioGroup")]
        private byte interestGroup;

        [SerializeField]
        private bool debugEchoMode;

        [SerializeField]
        private bool reliableMode;

        [SerializeField]
        private bool encrypt;

        [SerializeField]
        private bool transmitEnabled;

        [SerializeField]
        private SamplingRate samplingRate = SamplingRate.Sampling24000;

        [SerializeField]
        private OpusCodec.FrameDuration frameDuration = OpusCodec.FrameDuration.Frame20ms;

        [SerializeField]
        private int bitrate = 30000;

        [SerializeField]
        private InputSourceType sourceType;

        [SerializeField]
        private MicType microphoneType;

        [SerializeField]
        private AudioClip audioClip;

        [SerializeField]
        private bool loopAudioClip = true;

        private bool isRecording;

        private Func<IAudioDesc> inputFactory;

        private static AudioInEnumerator photonMicrophoneEnumerator;

        private AudioInChangeNotifier photonMicChangeNotifier;

        [SerializeField]
        private bool reactOnSystemChanges;

        private bool subscribedToSystemChangesPhoton;
        private bool subscribedToSystemChangesUnity;

        [SerializeField]
        private bool autoStart = true;

        #if UNITY_IOS || UNITY_EDITOR
        [SerializeField] 
        private IOS.AudioSessionParameters audioSessionParameters = IOS.AudioSessionParametersPresets.Game;
        #pragma warning disable 649
        [SerializeField]
        private bool useCustomAudioSessionParameters;
        [SerializeField] 
        private int audioSessionPresetIndex;
        #pragma warning restore 649
        #endif

        #if UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        private NativeAndroidMicrophoneSettings nativeAndroidMicrophoneSettings = new NativeAndroidMicrophoneSettings();
        #endif

        [SerializeField]
        private bool recordOnlyWhenEnabled;

        [SerializeField]
        private bool skipDeviceChangeChecks;

        private bool wasRecordingBeforePause;

        [SerializeField]
        private bool stopRecordingWhenPaused;
        
        [SerializeField]
        private bool useOnAudioFilterRead;

        [SerializeField]
        private bool trySamplingRateMatch;

        [SerializeField]
        private bool useMicrophoneTypeFallback = true;

        #endregion

        #region Properties

        private bool subscribedToSystemChanges
        {
            get
            {
                return this.subscribedToSystemChangesUnity || this.subscribedToSystemChangesPhoton;
            }
        }

        /// <summary>Enumerator for the available microphone devices gathered by the Photon plugin.</summary>
        public static AudioInEnumerator PhotonMicrophoneEnumerator
        {
            get
            {
                if (photonMicrophoneEnumerator == null)
                {
                    VoiceLogger logger = new VoiceLogger("PhotonMicrophoneEnumerator");
                    photonMicrophoneEnumerator = new AudioInEnumerator(logger);
                    if (!photonMicrophoneEnumerator.IsSupported)
                    {
                        logger.LogWarning("PhotonMicrophoneEnumerator is not supported on this platform.");
                    }
                    else if (photonMicrophoneEnumerator.Error != null && logger.IsErrorEnabled)
                    {
                        logger.LogError(photonMicrophoneEnumerator.Error);
                    }
                }
                return photonMicrophoneEnumerator;
            }
        }

        /// <summary>If true, this Recorder has been initialized and is ready to transmit to remote clients. Otherwise call <see cref="Init(VoiceConnection)"/>.</summary>
        public bool IsInitialized
        {
            get { return this.client != null; }
        }

        [Obsolete("Renamed to RequiresRestart")]
        public bool RequiresInit
        {
            get { return this.RequiresRestart; }
        }

        /// <summary>Returns true if something has changed in the Recorder while recording that won't take effect unless recording is restarted using <see cref="RestartRecording"/>.</summary>
        /// <remarks>Think of this as a "isDirty" flag.</remarks>
        public bool RequiresRestart { get; protected set; }

        /// <summary>If true, audio transmission is enabled.</summary>
        public bool TransmitEnabled
        {
            get { return this.transmitEnabled; }
            set
            {
                if (value != this.transmitEnabled)
                {
                    this.transmitEnabled = value;
                    if (this.voice != LocalVoiceAudioDummy.Dummy)
                    {
                        this.voice.TransmitEnabled = value;
                    }
                }
            }
        }

        /// <summary>If true, voice stream is sent encrypted.</summary>
        public bool Encrypt
        {
            get { return this.encrypt; }
            set
            {
                if (this.encrypt == value)
                {
                    return;
                }
                this.encrypt = value;
                this.voice.Encrypt = value;
            }
        }

        /// <summary>If true, outgoing stream routed back to client via server same way as for remote client's streams.</summary>
        public bool DebugEchoMode
        {
            get
            {
                if (this.debugEchoMode && this.InterestGroup != 0)
                {
                    this.voice.DebugEchoMode = false;
                    this.debugEchoMode = false;
                }
                return this.debugEchoMode;
            }
            set
            {
                if (this.debugEchoMode == value)
                {
                    return;
                }
                if (this.InterestGroup != 0)
                {
                    if (this.Logger.IsWarningEnabled)
                    {
                        this.Logger.LogWarning("Cannot enable DebugEchoMode when InterestGroup value ({0}) is different than 0.", this.interestGroup);
                    }
                    return;
                }
                this.debugEchoMode = value;
                this.voice.DebugEchoMode = value;
            }
        }

        /// <summary>If true, stream data sent in reliable mode.</summary>
        public bool ReliableMode
        {
            get 
            { 
                return this.reliableMode;
            }
            set
            {
                if (this.voice != LocalVoiceAudioDummy.Dummy)
                {
                    this.voice.Reliable = value;
                }
                this.reliableMode = value;
            }
        }

        /// <summary>If true, voice detection enabled.</summary>
        public bool VoiceDetection
        {
            get
            {
                this.GetStatusFromDetector();
                return this.voiceDetection;
            }
            set
            {
                this.voiceDetection = value;
                if (this.VoiceDetector != null)
                {
                    this.VoiceDetector.On = value;
                }
            }
        }

        /// <summary>Voice detection threshold (0..1, where 1 is full amplitude).</summary>
        public float VoiceDetectionThreshold
        {
            get
            {
                this.GetThresholdFromDetector();
                return this.voiceDetectionThreshold;
            }
            set
            {
                if (this.voiceDetectionThreshold.Equals(value))
                {
                    return;
                }
                if (value < 0f || value > 1f)
                {
                    if (this.Logger.IsErrorEnabled)
                    {
                        this.Logger.LogError("Value out of range: VAD Threshold needs to be between [0..1], requested value: {0}", value);
                    }
                    return;
                }
                this.voiceDetectionThreshold = value;
                if (this.VoiceDetector != null)
                {
                    this.VoiceDetector.Threshold = this.voiceDetectionThreshold;
                }
            }
        }

        /// <summary>Keep detected state during this time after signal level dropped below threshold. Default is 500ms</summary>
        public int VoiceDetectionDelayMs
        {
            get
            {
                this.GetActivityDelayFromDetector();
                return this.voiceDetectionDelayMs;
            }
            set
            {
                if (this.voiceDetectionDelayMs == value)
                {
                    return;
                }
                this.voiceDetectionDelayMs = value;
                if (this.VoiceDetector != null)
                {
                    this.VoiceDetector.ActivityDelayMs = value;
                }
            }
        }

        /// <summary>Custom user object to be sent in the voice stream info event.</summary>
        public object UserData
        {
            get { return this.userData; }
            set
            {
                if (this.userData != value)
                {
                    this.userData = value;
                    if (this.IsRecording)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "UserData");
                        }
                    }
                }
            }
        }

        /// <summary>Set the method returning new Voice.IAudioDesc instance to be assigned to a new voice created with Source set to Factory</summary>
        public Func<IAudioDesc> InputFactory
        {
            get { return this.inputFactory; }
            set
            {
                if (this.inputFactory != value)
                {
                    this.inputFactory = value;
                    if (this.IsRecording && this.SourceType == InputSourceType.Factory)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "InputFactory");
                        }
                    }
                }
            }
        }

        /// <summary>Returns voice activity detector for recorder's audio stream.</summary>
        public AudioUtil.IVoiceDetector VoiceDetector
        {
            get { return this.voiceAudio != null ? this.voiceAudio.VoiceDetector : null; }
        }

        /// <summary>Set or get Unity microphone device used for streaming.</summary>
        public string UnityMicrophoneDevice
        {
            get
            {
                if (!IsValidUnityMic(this.unityMicrophoneDevice))
                {
                    if (this.Logger.IsInfoEnabled)
                    {
                        this.Logger.LogInfo("\"{0}\" is not a valid Unity microphone device, switching to default", this.unityMicrophoneDevice);
                    }
                    this.unityMicrophoneDevice = null;
                    #if !UNITY_WEBGL
                    if (UnityMicrophone.devices.Length > 0)
                    {
                        this.unityMicrophoneDevice = UnityMicrophone.devices[0];
                    }
                    #endif
                }
                return this.unityMicrophoneDevice;
            }
            set
            {
                if (!IsValidUnityMic(value))
                {
                    if (this.Logger.IsErrorEnabled)
                    {
                        this.Logger.LogError("\"{0}\" is not a valid Unity microphone device", value);
                    }
                    return;
                }
                if (!CompareUnityMicNames(this.unityMicrophoneDevice, value))
                {
                    this.unityMicrophoneDevice = value;
                    #if !UNITY_WEBGL
                    if (string.IsNullOrEmpty(this.unityMicrophoneDevice) && UnityMicrophone.devices.Length > 0)
                    {
                        this.unityMicrophoneDevice = UnityMicrophone.devices[0];
                    }
                    #endif
                    if (this.IsRecording && this.SourceType == InputSourceType.Microphone && this.MicrophoneType == MicType.Unity)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "UnityMicrophoneDevice");
                        }
                    }
                    this.CheckAndSetSamplingRate();
                }

            }
        }

        /// <summary>Set or get photon microphone device used for streaming.</summary>
        public int PhotonMicrophoneDeviceId
        {
            get
            {
                #if !PHOTON_MICROPHONE_ENUMERATOR
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Photon microphone device IDs are not supported on this platform");
                }
                this.photonMicrophoneDeviceId = -1;
                #else
                if (!this.CheckIfPhotonMicrophoneIdIsValid(PhotonMicrophoneEnumerator, this.photonMicrophoneDeviceId))
                {
                    if (this.Logger.IsInfoEnabled)
                    {
                        this.Logger.LogInfo("\"{0}\" is not a valid Photon microphone device ID, switching to default (-1)", this.photonMicrophoneDeviceId);
                    }
                    this.photonMicrophoneDeviceId = -1;
                }
                #endif
                return this.photonMicrophoneDeviceId;
            }
            set
            {
                #if !PHOTON_MICROPHONE_ENUMERATOR
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("Setting a Photon microphone device ID is not supported on this platform");
                }
                #else
                if (!this.CheckIfPhotonMicrophoneIdIsValid(PhotonMicrophoneEnumerator, value))
                {
                    if (this.Logger.IsErrorEnabled)
                    {
                        this.Logger.LogError("\"{0}\" is not a valid Photon microphone device ID", value);
                    }
                    return;
                }

                if (this.photonMicrophoneDeviceId != value)
                {
                    this.photonMicrophoneDeviceId = value;
                    if (this.IsRecording && this.SourceType == InputSourceType.Microphone && this.MicrophoneType == MicType.Photon)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "PhotonMicrophoneDeviceId");
                        }
                    }
                }
                #endif
            }
        }

        /// <summary>Target interest group that will receive transmitted audio.</summary>
        /// <remarks>If AudioGroup != 0, recorder's audio data is sent only to clients listening to this group.</remarks>
        [Obsolete("Use InterestGroup instead")]
        public byte AudioGroup
        {
            get { return this.InterestGroup; }
            set { this.InterestGroup = value; }
        }

        /// <summary>Target interest group that will receive transmitted audio.</summary>
        /// <remarks>If InterestGroup != 0, recorder's audio data is sent only to clients listening to this group.</remarks>
        public byte InterestGroup
        {
            get
            {
                if (this.isRecording && this.voice.InterestGroup != this.interestGroup)
                {
                    // interest group probably set via GlobalInterestGroup!
                    this.interestGroup = this.voice.InterestGroup;
                    if (this.debugEchoMode && this.interestGroup != 0)
                    {
                        this.debugEchoMode = false;
                    }
                }
                return this.interestGroup;
            }
            set
            {
                if (this.interestGroup == value)
                {
                    return;
                }
                if (this.debugEchoMode && value != 0)
                {
                    this.debugEchoMode = false;
                    if (this.Logger.IsWarningEnabled)
                    {
                        this.Logger.LogWarning("DebugEchoMode disabled because InterestGroup changed to {0}. DebugEchoMode works only with Interest Group 0.", value);
                    }
                }
                this.interestGroup = value;
                this.voice.InterestGroup = value;
            }
        }

        /// <summary>Returns true if audio stream broadcasts.</summary>
        public bool IsCurrentlyTransmitting
        {
            get { return this.voice.IsCurrentlyTransmitting; }
        }

        /// <summary>Level meter utility.</summary>
        public AudioUtil.ILevelMeter LevelMeter
        {
            get { return this.voiceAudio != null ? this.voiceAudio.LevelMeter : null; }
        }

        /// <summary>If true, voice detector calibration is in progress.</summary>
        public bool VoiceDetectorCalibrating { get { return this.voiceAudio != null && this.TransmitEnabled && this.voiceAudio.VoiceDetectorCalibrating; } }

        protected ILocalVoiceAudio voiceAudio { get { return this.voice as ILocalVoiceAudio; } }

        /// <summary>Audio data source.</summary>
        public InputSourceType SourceType
        {
            get { return this.sourceType; }
            set
            {
                if (this.sourceType != value)
                {
                    this.sourceType = value;
                    if (this.IsRecording)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "Source");
                        }
                    }
                    this.CheckAndSetSamplingRate();
                }
            }
        }

        /// <summary>Which microphone API to use when the Source is set to Microphone.</summary>
        public MicType MicrophoneType
        {
            get { return this.microphoneType; }
            set
            {
                if (this.microphoneType != value)
                {
                    #if !UNITY_STANDALONE_OSX && !UNITY_STANDALONE_WIN && !UNITY_ANDROID && !UNITY_IOS
                    if (value == MicType.Photon)
                    {
                        if (this.Logger.IsErrorEnabled)
                        {
                            this.Logger.LogError("Photon microphone type is not supported on this platform");
                        }
                        return;
                    }
                    #endif
                    this.microphoneType = value;
                    if (this.IsRecording && this.SourceType == InputSourceType.Microphone)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "MicrophoneType");
                        }
                    }
                    this.CheckAndSetSamplingRate();
                }
            }
        }

        #pragma warning disable 618
        /// <summary>Force creation of 'short' pipeline and convert audio data to short for 'float' audio sources.</summary>
        [Obsolete("No longer used. Implicit conversion is done internally when needed.")]
        public SampleTypeConv TypeConvert { get; set; }
        #pragma warning restore 618

        /// <summary>Source audio clip.</summary>
        public AudioClip AudioClip
        {
            get { return this.audioClip; }
            set
            {
                if (this.audioClip != value)
                {
                    this.audioClip = value;
                    if (this.IsRecording && this.SourceType == InputSourceType.AudioClip)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "AudioClip");
                        }
                    }
                    this.CheckAndSetSamplingRate();
                }
            }
        }

        /// <summary>Loop playback for audio clip sources.</summary>
        public bool LoopAudioClip
        {
            get { return this.loopAudioClip; }
            set
            {
                if (this.loopAudioClip != value)
                {
                    this.loopAudioClip = value;
                    if (this.IsRecording && this.SourceType == InputSourceType.AudioClip)
                    {
                        AudioClipWrapper wrapper = this.inputSource as AudioClipWrapper;
                        if (wrapper != null)
                        {
                            wrapper.Loop = value;
                        }
                        else if (this.Logger.IsErrorEnabled)
                        {
                            this.Logger.LogError("Unexpected: Recorder inputSource is not of AudioClipWrapper type or is null.");
                        }
                    }
                }
            }
        }

        /// <summary>Outgoing audio stream sampling rate.</summary>
        public SamplingRate SamplingRate
        {
            get { return this.samplingRate; }
            set
            {
                this.CheckAndSetSamplingRate(value);
            }
        }

        /// <summary>Outgoing audio stream encoder delay.</summary>
        public OpusCodec.FrameDuration FrameDuration
        {
            get { return this.frameDuration; }
            set
            {
                if (this.frameDuration != value)
                {
                    this.frameDuration = value;
                    if (this.IsRecording)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "FrameDuration");
                        }
                    }
                }
            }
        }

        /// <summary>Outgoing audio stream bitrate.</summary>
        public int Bitrate
        {
            get { return this.bitrate; }
            set
            {
                if (this.bitrate != value)
                {
                    if (value < MIN_OPUS_BITRATE || value > MAX_OPUS_BITRATE)
                    {
                        if (this.Logger.IsErrorEnabled)
                        {
                            this.Logger.LogError("Unsupported bitrate value {0}, valid range: {1}-{2}", value, MIN_OPUS_BITRATE, MAX_OPUS_BITRATE);
                        }
                    }
                    else
                    {
                        this.bitrate = value;
                        if (this.IsRecording)
                        {
                            this.RequiresRestart = true;
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "Bitrate");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Gets or sets whether this Recorder is actively recording audio to be transmitted.</summary>
        public bool IsRecording
        {
            get
            {
                return this.isRecording;
            }
            set
            {
                if (this.isRecording != value)
                {
                    if (this.isRecording)
                    {
                        this.StopRecording();
                    }
                    else
                    {
                        this.StartRecording();
                    }
                }
            }
        }

        /// <summary> If true, the Recorder will automatically restart recording to recover from audio device changes. </summary>
        /// <remarks>
        /// By default, the Recorder will restart recording only when the <see cref="Recorder.SourceType"/> is <see cref="InputSourceType.Microphone"/>
        /// and the device being used is no longer available or valid, in some cases you may need to force restarts even if the device in use did not change.
        /// To enable this set <see cref="Recorder.SkipDeviceChangeChecks"/> to true.
        /// </remarks>
        public bool ReactOnSystemChanges
        {
            get { return this.reactOnSystemChanges; }
            set
            {
                if (this.reactOnSystemChanges != value)
                {
                    this.reactOnSystemChanges = value;
                    if (this.IsRecording)
                    {
                        if (this.reactOnSystemChanges)
                        {
                            if (!this.subscribedToSystemChanges)
                            {
                                this.SubscribeToSystemChanges();
                            }
                        }
                        else if (this.subscribedToSystemChanges)
                        {
                            this.UnsubscribeFromSystemChanges();
                        }
                    }
                }
            }
        }

        /// <summary> If true, automatically start recording when initialized. </summary>
        public bool AutoStart
        {
            get { return this.autoStart; }
            set { this.autoStart = value; }
        }

        /// <summary> If true, component will work only when enabled and active in hierarchy. </summary>
        public bool RecordOnlyWhenEnabled
        {
            get { return this.recordOnlyWhenEnabled; }
            set
            {
                if (this.recordOnlyWhenEnabled != value)
                {
                    this.recordOnlyWhenEnabled = value;
                    if (this.IsInitialized)
                    {
                        if (this.recordOnlyWhenEnabled)
                        {
                            this.IsRecording = this.isActiveAndEnabled;
                        } 
                        else if (this.AutoStart)
                        {
                            this.StartRecording();
                        }
                    }
                }
            }
        }

        /// <summary> If true, restarts recording without checking if audio config/device changes affected recording. </summary>
        /// <remarks> To be used when <see cref="Recorder.ReactOnSystemChanges"/> is true. </remarks>
        public bool SkipDeviceChangeChecks
        {
            get { return this.skipDeviceChangeChecks; }
            set { this.skipDeviceChangeChecks = value; }
        }

        /// <summary> If true, stop recording when paused resume/restart when un-paused. </summary>
        public bool StopRecordingWhenPaused
        {
            get { return this.stopRecordingWhenPaused; }
            set { this.stopRecordingWhenPaused = value; }
        }
        
        /// <summary> If true, recording will make use of Unity's OnAudioFitlerRead callback from a muted local AudioSource. </summary>
        /// <remarks> If enabled, 3D sounds and voice positioning can be lost. </remarks>
        public bool UseOnAudioFilterRead
        {
            get
            {
                return this.useOnAudioFilterRead;
            }
            set
            {
                if (this.useOnAudioFilterRead != value)
                {
                    this.useOnAudioFilterRead = value;
                    if (this.IsRecording && this.SourceType == InputSourceType.Microphone && this.MicrophoneType == MicType.Unity)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "UseOnAudioFilterRead");
                        }
                    }
                }
            }
        }

        /// <summary> If true, Recorder will try to match sampling rates of microphone device and Opus encoder to avoid re sampling of audio input. </summary>
        public bool TrySamplingRateMatch
        {
            get
            {
                return this.trySamplingRateMatch;
            }
            set
            {
                if (this.trySamplingRateMatch != value)
                {
                    this.trySamplingRateMatch = value;
                    if (this.trySamplingRateMatch)
                    {
                        this.CheckAndSetSamplingRate();
                    }
                }
            }
        }

        /// <summary> If true, if recording fails to start with Unity microphone type, Photon microphone type is used -if available- as a fallback and vice versa. </summary>
        public bool UseMicrophoneTypeFallback
        {
            get
            {
                return this.useMicrophoneTypeFallback;
            }
            set
            {
                this.useMicrophoneTypeFallback = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the Recorder component to be able to transmit audio.
        /// </summary>
        /// <param name="voiceClient">The VoiceClient to be used with this Recorder.</param>
        /// <param name="customObj">Optional user data object to be transmitted with the voice stream info</param>
        [Obsolete("use Init(VoiceConnection) instead")]
        public void Init(VoiceClient voiceClient, object customObj = null)
        {
            if (this.IsInitialized)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Recorder already initialized.");
                }
                return;
            }

            if (voiceClient == null)
            {
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("voiceClient is null.");
                }
                return;
            }

            this.client = voiceClient;
            this.userData = customObj;

            if (this.AutoStart)
            {
                this.StartRecording();
            }
        }

        /// <summary>
        /// Initializes the Recorder component to be able to transmit audio.
        /// </summary>
        /// <param name="voiceConnection">The VoiceConnection to be used with this Recorder.</param>
        public void Init(VoiceConnection voiceConnection)
        {
            if (voiceConnection == null)
            {
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("voiceConnection is null.");
                }
                return;
            }
            if (!this.IgnoreGlobalLogLevel)
            {
                this.LogLevel = voiceConnection.GlobalRecordersLogLevel;
            }
            if (this.IsInitialized)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Recorder already initialized.");
                }
                return;
            }
            if (voiceConnection.VoiceClient == null)
            {
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("voiceConnection.VoiceClient is null.");
                }
                return;
            }
            this.client = voiceConnection.VoiceClient;
            if (this.AutoStart)
            {
                this.StartRecording();
            }
        }

        [Obsolete("Renamed to RestartRecording")]
        public void ReInit()
        {
            this.RestartRecording();
        }

        /// <summary>
        /// Restarts recording if something has changed that requires this.
        /// </summary>
        /// <param name="force">Set to true if you want to restart even if this is not required (RequiresRestart = false)</param>
        public void RestartRecording(bool force = false)
        {
            if (!force && !this.RequiresRestart)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Recorder does not require restart.");
                }
                return;
            }
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogDebug("Restarting recording, RequiresRestart?={0} forcedRestart?={1}", this.RequiresRestart, force);
            }
            this.StopRecording();
            this.StartRecording();
        }

        /// <summary>Trigger voice detector calibration process.
        /// While calibrating, keep silence. Voice detector sets threshold basing on measured background noise level.
        /// </summary>
        /// <param name="durationMs">Duration of calibration in milliseconds.</param>
        /// <param name="detectionEndedCallback">Callback when VAD calibration ends.</param>
        public void VoiceDetectorCalibrate(int durationMs, Action<float> detectionEndedCallback = null)
        {
            if (this.voiceAudio != null)
            {
                if (!this.TransmitEnabled)
                {
                    if (this.Logger.IsWarningEnabled)
                    {
                        this.Logger.LogWarning("Cannot start voice detection calibration when transmission is not enabled");
                    }
                    return;
                }
                this.voiceAudio.VoiceDetectorCalibrate(durationMs, newThreshold =>
                {
                    this.GetThresholdFromDetector();
                    if (detectionEndedCallback != null)
                    {
                        detectionEndedCallback(this.voiceDetectionThreshold);
                    }
                });
            }
        }

        /// <summary> Starts recording. </summary>
        public void StartRecording()
        {
            if (this.IsRecording)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Recorder is already started.");
                }
                return;
            }
            if (!this.IsInitialized)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Recording can't be started if Recorder is not initialized. Call Recorder.Init(VoiceConnection) first.");
                }
                return;
            }
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogDebug("Starting recording");
            }
            this.wasRecordingBeforePause = false;
            this.Setup();
        }

        /// <summary> Stops recording. </summary>
        public void StopRecording()
        {
            if (!this.IsRecording)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Recorder is not started.");
                }
                return;
            }
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogDebug("Stopping recording");
            }
            this.wasRecordingBeforePause = false;
            this.RemoveVoice(true);
        }

        #if UNITY_EDITOR || UNITY_IOS
        /// <summary>
        /// Sets the AudioSessionParameters for iOS audio initialization when Photon MicrophoneType is used.
        /// </summary>
        /// <param name="asp">You can use custom value or one from presets, <see cref="IOS.AudioSessionParametersPresets"/></param>
        /// <returns>If a change has been made.</returns>
        public bool SetIosAudioSessionParameters(IOS.AudioSessionParameters asp)
        {
            return this.SetIosAudioSessionParameters(asp.Category, asp.Mode, asp.CategoryOptions);
        }
        /// <summary>
        /// Sets the AudioSessionParameters for iOS audio initialization when Photon MicrophoneType is used.
        /// </summary>
        /// <param name="category">Audio session category to be used.</param>
        /// <param name="mode">Audio session mode to be used.</param>
        /// <param name="options">Audio session category options to be used</param>
        /// <returns>If a change has been made.</returns>
        public bool SetIosAudioSessionParameters(IOS.AudioSessionCategory category, IOS.AudioSessionMode mode, IOS.AudioSessionCategoryOption[] options)
        {
            int opt = 0;
            if (options != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    opt |= (int)options[i];
                }
            }
            if (this.audioSessionParameters.Category != category || 
                this.audioSessionParameters.Mode != mode ||
                this.audioSessionParameters.CategoryOptionsToInt() != opt)
            {
                this.audioSessionParameters.Category = category;
                this.audioSessionParameters.Mode = mode;
                this.audioSessionParameters.CategoryOptions = options;
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Changing iOS audioSessionParameters = {0}", this.audioSessionParameters);
                }
                #if !UNITY_EDITOR
                if (this.IsRecording && this.SourceType == InputSourceType.Microphone && this.MicrophoneType == MicType.Photon)
                {
                    this.RequiresRestart = true;
                    if (this.Logger.IsInfoEnabled)
                    {
                        this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "iOSAudioSessionParameters");
                    }
                }
                #endif
                return true;
            }
            return false;
        }
        #endif

        #if UNITY_EDITOR || UNITY_ANDROID
        /// <summary>
        /// Sets the native Android audio input settings when the Photon microphone type is used.
        /// </summary>
        /// <param name="nams">The settings to be applied</param>
        /// <returns>If a change has been made.</returns>
        public bool SetAndroidNativeMicrophoneSettings(NativeAndroidMicrophoneSettings nams)
        {
            return this.SetAndroidNativeMicrophoneSettings(nams.AcousticEchoCancellation, nams.AutomaticGainControl,
                nams.NoiseSuppression);
        }
        /// <summary>
        /// Sets the native Android audio input settings when the Photon microphone type is used.
        /// </summary>
        /// <param name="aec">Acoustic Echo Cancellation</param>
        /// <param name="agc">Automatic Gain Control</param>
        /// <param name="ns">Noise Suppression</param>
        /// <returns>If a change has been made.</returns>
        public bool SetAndroidNativeMicrophoneSettings(bool aec = false, bool agc = false, bool ns = false)
        {
            if (this.nativeAndroidMicrophoneSettings.AcousticEchoCancellation != aec ||
                this.nativeAndroidMicrophoneSettings.AutomaticGainControl != agc ||
                this.nativeAndroidMicrophoneSettings.NoiseSuppression != ns)
                {
                    if (this.Logger.IsInfoEnabled)
                    {
                        this.Logger.LogInfo("Changing Android native microphone settings to aec = {0}, agc = {1}, ns = {2}", aec, agc, ns);
                    }
                    #if !UNITY_EDITOR
                    if (this.IsRecording && this.SourceType == InputSourceType.Microphone && this.MicrophoneType == MicType.Photon)
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "nativeAndroidMicrophoneSettings");
                        }
                    }
                    #endif
                    return true;
                }
            return false;
        }
        #endif

        /// <summary> Resets audio session and parameters locally to fix broken recording due to system configuration modifications or audio interruptions or audio routing changes. </summary>
        /// <returns> If reset is done. </returns>
        public bool ResetLocalAudio()
        {
            if (this.inputSource != null && this.inputSource is IResettable)
            {
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Resetting local audio.");
                }
                (this.inputSource as IResettable).Reset();
                return true;
            }
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogDebug("InputSource is null or not resettable.");
            }
            return false;
        }

        #endregion

        #region Private Methods

        private void Setup()
        {
            this.voice = this.CreateLocalVoiceAudioAndSource();
            if (this.voice == LocalVoiceAudioDummy.Dummy)
            {
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("Local input source setup and voice stream creation failed. No recording or transmission will be happening. See previous error log messages for more details.");
                }
                return;
            }
            if (this.ReactOnSystemChanges && !this.subscribedToSystemChanges)
            {
                this.SubscribeToSystemChanges();
            }
            if (this.VoiceDetector != null)
            {
                this.VoiceDetector.Threshold = this.voiceDetectionThreshold;
                this.VoiceDetector.ActivityDelayMs = this.voiceDetectionDelayMs;
                this.VoiceDetector.On = this.voiceDetection;
            }
            this.voice.InterestGroup = this.InterestGroup;
            this.voice.DebugEchoMode = this.DebugEchoMode;
            this.voice.Encrypt = this.Encrypt;
            this.voice.Reliable = this.ReliableMode;
            this.RequiresRestart = false;
            this.isRecording = true;
            this.SendPhotonVoiceCreatedMessage();
            this.voice.TransmitEnabled = this.TransmitEnabled;
        }

        private LocalVoice CreateLocalVoiceAudioAndSource()
        {
            switch (this.SourceType)
            {
                case InputSourceType.Microphone:
                {
                    bool fallbackMicrophone = false;
                    switch (this.MicrophoneType)
                    {
                        case MicType.Unity:
                        {
                            #if UNITY_WEBGL
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Unity Microphone not supported in WebGL, Photon Voice 2 does not support WebGL but we made sure code compiles for WebGL at least.");
                            }
                            return LocalVoiceAudioDummy.Dummy;
                            #else
                            if (UnityMicrophone.devices.Length < 1)
                            {
                                if (this.Logger.IsErrorEnabled)
                                {
                                    this.Logger.LogError("No Unity microphone device available (Microphone.devices is empty).");
                                }
                            }
                            else
                            {
                                string micDev = this.UnityMicrophoneDevice;
                                if (this.Logger.IsInfoEnabled)
                                {
                                    this.Logger.LogInfo("Setting recorder's source to Unity microphone device {0}", micDev);
                                }
                                // mic can ignore passed sampling rate and set its own
                                if (this.UseOnAudioFilterRead)
                                {
                                    this.inputSource = new MicWrapperPusher(micDev, this.transform, (int)this.SamplingRate, this.Logger);
                                }
                                else
                                {
                                    this.inputSource = new MicWrapper(micDev, (int)this.SamplingRate, this.Logger);
                                }
                            }
                            if (this.inputSource != null) 
                            {
                                if (this.inputSource.Error != null)
                                {
                                    if (this.Logger.IsErrorEnabled)
                                    {
                                        this.Logger.LogError("Unity microphone input source creation failure: {0}", this.inputSource.Error);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            #if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
                            if (this.UseMicrophoneTypeFallback && !fallbackMicrophone)
                            {
                                fallbackMicrophone = true;
                                if (this.Logger.IsErrorEnabled)
                                {
                                    this.Logger.LogError("Unity microphone failed. Falling back to Photon microphone");
                                }
                                goto case MicType.Photon;
                            }
                            #endif
                            #endif
                        }
                            break;
                        case MicType.Photon:
                        {
                            #if PHOTON_MICROPHONE_ENUMERATOR
                            int hwMicDev = this.PhotonMicrophoneDeviceId;
                            if (this.Logger.IsInfoEnabled)
                            {
                                string photonMicName;
                                if (this.TryGetPhotonMicrophoneNameFromId(PhotonMicrophoneEnumerator, hwMicDev, out photonMicName))
                                {
                                    this.Logger.LogInfo("Setting recorder's source to Photon microphone device ID={0} \"{1}\"", hwMicDev, photonMicName);
                                }
                                else
                                {
                                    this.Logger.LogInfo("Setting recorder's source to Photon microphone device ID={0}", hwMicDev);
                                }
                            }
                            #else
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Setting recorder's source to Photon microphone device");
                            }
                            #endif
                            #if UNITY_STANDALONE_WIN && !UNITY_EDITOR || UNITY_EDITOR_WIN
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Setting recorder's source to WindowsAudioInPusher");
                            }
                            this.inputSource = new Windows.WindowsAudioInPusher(hwMicDev, this.Logger);
                            #elif UNITY_IOS && !UNITY_EDITOR
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Setting recorder's source to IOS.AudioInPusher with session {0}", audioSessionParameters);
                            }
                            this.inputSource = new IOS.AudioInPusher(audioSessionParameters, this.Logger);
                            #elif UNITY_STANDALONE_OSX && !UNITY_EDITOR || UNITY_EDITOR_OSX
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Setting recorder's source to MacOS.AudioInPusher");
                            }
                            this.inputSource = new MacOS.AudioInPusher(hwMicDev, this.Logger);
                            #elif UNITY_ANDROID && !UNITY_EDITOR
                            if (this.Logger.IsInfoEnabled)
                            {
                                this.Logger.LogInfo("Setting recorder's source to UnityAndroidAudioInAEC");
                            }
                            this.inputSource = new UnityAndroidAudioInAEC(this.Logger, this.nativeAndroidMicrophoneSettings.AcousticEchoCancellation, this.nativeAndroidMicrophoneSettings.AutomaticGainControl, this.nativeAndroidMicrophoneSettings.NoiseSuppression);
                            #else
                            if (this.Logger.IsErrorEnabled)
                            {
                                this.Logger.LogError("Photon microphone type is not supported for the current platform.");
                            }
                            #endif
                            if (this.inputSource != null) 
                            {
                                if (this.inputSource.Error != null)
                                {
                                    if (this.Logger.IsErrorEnabled)
                                    {
                                        this.Logger.LogError("Photon microphone input source creation failure: {0}", this.inputSource.Error);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (this.UseMicrophoneTypeFallback && !fallbackMicrophone)
                            {
                                fallbackMicrophone = true;
                                if (this.Logger.IsErrorEnabled)
                                {
                                    this.Logger.LogError("Photon microphone failed. Falling back to Unity microphone");
                                }
                                goto case MicType.Unity;
                            }
                            break;
                        }
                        default:
                            if (this.Logger.IsErrorEnabled)
                            {
                                this.Logger.LogError("unknown MicrophoneType value {0}", this.MicrophoneType);
                            }
                            return LocalVoiceAudioDummy.Dummy;
                    }
                }
                    break;
                case InputSourceType.AudioClip:
                {
                    if (this.AudioClip == null)
                    {
                        if (this.Logger.IsErrorEnabled)
                        {
                            this.Logger.LogError("AudioClip property must be set for AudioClip audio source");
                        }
                        return LocalVoiceAudioDummy.Dummy;
                    }
                    AudioClipWrapper audioClipWrapper = new AudioClipWrapper(this.AudioClip); // never fails, no need to check Error
                    audioClipWrapper.Loop = this.LoopAudioClip;
                    this.inputSource = audioClipWrapper;
                }
                    break;
                case InputSourceType.Factory:
                {
                    if (this.InputFactory == null)
                    {
                        if (this.Logger.IsErrorEnabled)
                        {
                            this.Logger.LogError("Recorder.InputFactory must be specified if Recorder.Source set to Factory");
                        }
                        return LocalVoiceAudioDummy.Dummy;
                    }
                    this.inputSource = this.InputFactory();
                    if (this.inputSource.Error != null && this.Logger.IsErrorEnabled)
                    {
                        this.Logger.LogError("InputFactory creation failure: {0}.", this.inputSource.Error);
                    }
                }
                    break;
                default:
                    if (this.Logger.IsErrorEnabled)
                    {
                        this.Logger.LogError("unknown Source value {0}", this.SourceType);
                    }
                    return LocalVoiceAudioDummy.Dummy;
            }
            if (this.inputSource == null || this.inputSource.Error != null)
            {
                return LocalVoiceAudioDummy.Dummy;
            }
            if (this.inputSource.Channels == 0)
            {
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("inputSource.Channels is zero");
                }
                return LocalVoiceAudioDummy.Dummy;
            }
            SamplingRate effectiveSamplingRate = this.samplingRate;
            int samplingRateInt = (int)effectiveSamplingRate;
            if (this.TrySamplingRateMatch && this.inputSource.SamplingRate != samplingRateInt)
            {
                effectiveSamplingRate = this.GetSupportedSamplingRate(this.inputSource.SamplingRate);
                if (effectiveSamplingRate != this.samplingRate && this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("Sampling rate requested ({0}Hz) is not used, input source is expecting {1}Hz instead so switching to the closest supported value: {1}Hz.", samplingRateInt, this.inputSource.SamplingRate, (int)effectiveSamplingRate);
                }
            }
            AudioSampleType audioSampleType = AudioSampleType.Source;
            WebRtcAudioDsp dsp = this.GetComponent<WebRtcAudioDsp>();
            if (dsp != null && dsp.enabled)
            {
                audioSampleType = AudioSampleType.Short;
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Type Conversion set to Short. Audio samples will be converted if source samples types differ.");
                }
                samplingRateInt = (int) effectiveSamplingRate;
                if (Array.IndexOf(WebRTCAudioProcessor.SupportedSamplingRates, samplingRateInt) < 0)
                {
                    switch (effectiveSamplingRate)
                    {
                        case SamplingRate.Sampling12000:
                            effectiveSamplingRate = SamplingRate.Sampling16000;
                            break;
                        case SamplingRate.Sampling24000:
                            effectiveSamplingRate = SamplingRate.Sampling48000;
                            break;
                    }
                    if (this.Logger.IsWarningEnabled)
                    {
                        this.Logger.LogWarning("Sampling rate requested ({0}Hz) is not supported by WebRTC Audio DSP, switching to the closest supported value: {1}Hz.", samplingRateInt, (int)effectiveSamplingRate);
                    }
                }
            }
            VoiceInfo voiceInfo = VoiceInfo.CreateAudioOpus(effectiveSamplingRate, this.inputSource.Channels, this.FrameDuration, this.Bitrate, this.UserData);
            return this.client.CreateLocalVoiceAudioFromSource(voiceInfo, this.inputSource, audioSampleType);
        }

        protected virtual void SendPhotonVoiceCreatedMessage()
        {
            this.gameObject.SendMessage("PhotonVoiceCreated", new Unity.PhotonVoiceCreatedParams { Voice = this.voice, AudioDesc = this.inputSource }, SendMessageOptions.DontRequireReceiver);
        }

        private void OnDestroy()
        {
            // no need to send PhotonVoiceRemoved since object is destroyed
            this.RemoveVoice(false);
        }

        private void RemoveVoice(bool sendUnityMsg)
        {
            if (this.subscribedToSystemChanges)
            {
                this.UnsubscribeFromSystemChanges();
            }
            this.GetThresholdFromDetector();
            this.GetStatusFromDetector();
            this.GetActivityDelayFromDetector();
            if (this.voice != LocalVoiceAudioDummy.Dummy)
            {
                this.interestGroup = this.voice.InterestGroup;
                if (this.debugEchoMode && this.interestGroup != 0)
                {
                    this.debugEchoMode = false;
                }
                this.voice.RemoveSelf();
                this.voice = LocalVoiceAudioDummy.Dummy;
            }
            if (this.inputSource != null)
            {
                this.inputSource.Dispose();
                this.inputSource = null;
            }
            if (sendUnityMsg)
            {
                this.gameObject.SendMessage("PhotonVoiceRemoved", SendMessageOptions.DontRequireReceiver);
            }
            this.isRecording = false;
            this.RequiresRestart = false;
        }

        private void OnAudioConfigChanged(bool deviceWasChanged)
        {
            if (this.Logger.IsInfoEnabled)
            {
                this.Logger.LogInfo("OnAudioConfigChanged deviceWasChanged={0}", deviceWasChanged);
            }
            if (this.SkipDeviceChangeChecks || deviceWasChanged)
            {
                this.HandleDeviceChange();
            }
        }

        private void PhotonMicrophoneChangeDetected()
        {
            if (this.Logger.IsInfoEnabled)
            {
                this.Logger.LogInfo("Microphones change detected by Photon native plugin");
            }
            this.HandleDeviceChange();
        }

        private void HandleDeviceChange()
        {
            #if PHOTON_MICROPHONE_ENUMERATOR
            PhotonMicrophoneEnumerator.Refresh();
            #endif
            if (this.IsRecording)
            {
                bool restart = false;
                if (this.SkipDeviceChangeChecks)
                {
                    restart = true;
                }
                else if (this.SourceType == InputSourceType.Microphone)
                {
                    if (this.MicrophoneType == MicType.Photon)
                    {
                        #if PHOTON_MICROPHONE_ENUMERATOR
                        restart = this.photonMicrophoneDeviceId == -1 || !this.CheckIfPhotonMicrophoneIdIsValid(PhotonMicrophoneEnumerator, this.photonMicrophoneDeviceId);
                        #else
                        restart = true;
                        #endif
                    }
                    else
                    {
                        restart = string.IsNullOrEmpty(this.unityMicrophoneDevice) || !IsValidUnityMic(this.unityMicrophoneDevice);
                    }
                }
                if (restart)
                {
                    if (this.ResetLocalAudio())
                    {
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Local audio reset as a result of audio config/device change.");
                        }
                    }
                    else
                    {
                        this.RequiresRestart = true;
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Restarting Recording as a result of audio config/device change.");
                        }
                        this.RestartRecording();
                    }
                }
            }
        }

        private void SubscribeToSystemChanges()
        {
            #if UNITY_ANDROID || UNITY_IOS
            if (this.SourceType == InputSourceType.Microphone && this.MicrophoneType == MicType.Photon)
            {
                if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("ReactOnSystemChanges ignored when using Photon microphone type as this is handled internally for iOS and Android via native plugins.");
                }
                return;
            }
            #endif
            this.photonMicChangeNotifier = new AudioInChangeNotifier(this.PhotonMicrophoneChangeDetected, this.Logger);
            if (this.photonMicChangeNotifier.IsSupported)
            {
                if (this.photonMicChangeNotifier.Error == null)
                {
                    this.subscribedToSystemChangesPhoton = true;
                    if (this.Logger.IsInfoEnabled)
                    {
                        this.Logger.LogInfo("Subscribed to audio in change notifications via Photon plugin.");
                    }
                    return;
                }
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("Error creating instance of photonMicChangeNotifier: {0}", this.photonMicChangeNotifier.Error);
                }
            }
            this.photonMicChangeNotifier.Dispose();
            this.photonMicChangeNotifier = null;
            AudioSettings.OnAudioConfigurationChanged += this.OnAudioConfigChanged;
            this.subscribedToSystemChangesUnity = true;
            if (this.Logger.IsInfoEnabled)
            {
                this.Logger.LogInfo("Subscribed to audio configuration changes via Unity callback.");
            }
        }

        private void UnsubscribeFromSystemChanges()
        {
            if (this.subscribedToSystemChangesUnity)
            {
                AudioSettings.OnAudioConfigurationChanged -= this.OnAudioConfigChanged;
                this.subscribedToSystemChangesUnity = false;
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Unsubscribed from audio configuration changes via Unity callback.");
                }
            }
            if (this.subscribedToSystemChangesPhoton)
            {
                if (this.photonMicChangeNotifier == null)
                {
                    if (this.Logger.IsErrorEnabled)
                    {
                        this.Logger.LogError("Unexpected: photonMicChangeNotifier is null while subscribedToSystemChangesPhoton is true.");
                    }
                }
                else
                {
                    this.photonMicChangeNotifier.Dispose();
                    this.photonMicChangeNotifier = null;
                }
                this.subscribedToSystemChangesPhoton = false;
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Unsubscribed from audio in change notifications via Photon plugin.");
                }
            }
        }

        private void GetThresholdFromDetector()
        {
            if (this.IsRecording && this.VoiceDetector != null && !this.voiceDetectionThreshold.Equals(this.VoiceDetector.Threshold))
            {
                if (this.VoiceDetector.Threshold <= 1f && this.VoiceDetector.Threshold >= 0f)
                {
                    if (this.Logger.IsDebugEnabled)
                    {
                        this.Logger.LogDebug("VoiceDetectionThreshold automatically changed from {0} to {1}", this.voiceDetectionThreshold, this.VoiceDetector.Threshold);
                    }
                    this.voiceDetectionThreshold = this.VoiceDetector.Threshold;
                }
                else if (this.Logger.IsWarningEnabled)
                {
                    this.Logger.LogWarning("VoiceDetector.Threshold has unexpected value {0}", this.VoiceDetector.Threshold);
                }
            }
        }

        private void GetActivityDelayFromDetector()
        {
            if (this.IsRecording && this.VoiceDetector != null && this.voiceDetectionDelayMs != this.VoiceDetector.ActivityDelayMs)
            {
                if (this.Logger.IsDebugEnabled)
                {
                    this.Logger.LogDebug("VoiceDetectionDelayMs automatically changed from {0} to {1}", this.voiceDetectionDelayMs, this.VoiceDetector.ActivityDelayMs);
                }
                this.voiceDetectionDelayMs = this.VoiceDetector.ActivityDelayMs;
            }
        }

        private void GetStatusFromDetector()
        {
            if (this.IsRecording && this.VoiceDetector != null && this.voiceDetection != this.VoiceDetector.On)
            {
                if (this.Logger.IsDebugEnabled)
                {
                    this.Logger.LogDebug("VoiceDetection automatically changed from {0} to {1}", this.voiceDetection, this.VoiceDetector.On);
                }
                this.voiceDetection = this.VoiceDetector.On;
            }
        }

        public static bool CompareUnityMicNames(string mic1, string mic2)
        {
            if (IsDefaultUnityMic(mic1) && IsDefaultUnityMic(mic2))
            {
                return true;
            }
            if (mic1 != null && mic1.Equals(mic2))
            {
                return true;
            }
            return false;
        }

        public static bool IsDefaultUnityMic(string mic)
        {
            #if UNITY_WEBGL
            return false;
            #else
            return string.IsNullOrEmpty(mic) || Array.IndexOf(UnityMicrophone.devices, mic) == 0;
            #endif
        }

        private static bool IsValidUnityMic(string mic)
        {
            #if UNITY_WEBGL
            return false;
            #else
            return string.IsNullOrEmpty(mic) || UnityMicrophone.devices.Contains(mic);
            #endif
        }

        private void OnEnable()
        {
            if (this.IsInitialized && !this.IsRecording && this.AutoStart)
            {
                this.StartRecording();
            }
        }

        private void OnDisable()
        {
            if (this.RecordOnlyWhenEnabled && this.IsRecording)
            {
                this.StopRecording();
            }
        }

        private bool CheckIfPhotonMicrophoneIdIsValid(AudioInEnumerator audioInEnumerator, int id)
        {
            if (id == -1) // default
            {
                return true;
            }
            #if PHOTON_MICROPHONE_ENUMERATOR
            if (audioInEnumerator.IsSupported && audioInEnumerator.Error == null && audioInEnumerator.IDIsValid(id))
            {
                for (int i = 0; i < audioInEnumerator.Count; i++)
                {
                    if (audioInEnumerator.IDAtIndex(i) == id)
                    {
                        return true;
                    }
                }
            }
            #endif
            return false;
        }

        private bool TryGetPhotonMicrophoneNameFromId(AudioInEnumerator audioInEnumerator, int id, out string photonMicName)
        { 
            photonMicName = null;
            if (id == -1) // default
            {
                return false;
            }
            #if PHOTON_MICROPHONE_ENUMERATOR
            if (audioInEnumerator.IsSupported && audioInEnumerator.Error == null && audioInEnumerator.IDIsValid(id))
            {
                for (int i = 0; i < audioInEnumerator.Count; i++)
                {
                    if (audioInEnumerator.IDAtIndex(i) == id)
                    {
                        photonMicName = audioInEnumerator.NameAtIndex(i);
                        return true;
                    }
                }
            }
            #endif
            return false;
        }

        private void OnApplicationPause(bool paused)
        {
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogDebug("OnApplicationPause({0})", paused);
            }
            this.HandleApplicationPause(paused);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (this.Logger.IsDebugEnabled)
            {
                this.Logger.LogDebug("OnApplicationFocus({0})", focused);
            }
            this.OnApplicationPause(!focused);
        }

        private void HandleApplicationPause(bool paused)
        {
            if (this.StopRecordingWhenPaused)
            {
                if (paused)
                {
                    this.wasRecordingBeforePause = this.IsRecording;
                    if (this.wasRecordingBeforePause)
                    {
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Stopping recording as application went to background");
                        }
                        this.RemoveVoice(true);
                    }
                }
                else if (this.wasRecordingBeforePause)
                {
                    if (!this.IsRecording)
                    {
                        if (this.Logger.IsInfoEnabled)
                        {
                            this.Logger.LogInfo("Starting recording as application is back from background");
                        }
                        this.Setup();
                    }
                    this.wasRecordingBeforePause = false;
                }
            } 
            else if (this.ResetLocalAudio() && this.Logger.IsInfoEnabled)
            {
                this.Logger.LogInfo("Local audio reset as application is back from background");
            }
        }

        private SamplingRate GetSupportedSamplingRate(int requested)
        {
            if (Enum.IsDefined(typeof(SamplingRate), requested))
            {
                return (SamplingRate)requested;
            }
            int diff = int.MaxValue;
            SamplingRate res = SamplingRate.Sampling48000;
            foreach (SamplingRate sr in samplingRateValues)
            {
                int sri = (int) sr;
                int d = Math.Abs(sri - requested);
                if (d < diff)
                {
                    diff = d;
                    res = sr;
                }
            }
            return res;
        }

        private SamplingRate GetSupportedSamplingRateForUnityMicrophone(SamplingRate requested)
        {
            int minFreq, maxFreq;
            UnityMicrophone.GetDeviceCaps(this.UnityMicrophoneDevice, out minFreq, out maxFreq);
            return this.GetSupportedSamplingRate(requested, minFreq, maxFreq);
        }

        private SamplingRate GetSupportedSamplingRate(SamplingRate requested, int minFreq, int maxFreq)
        {
            SamplingRate res = requested;
            int requestedSamplingRateInt = (int) this.samplingRate;
            if (requestedSamplingRateInt < minFreq || maxFreq != 0 && requestedSamplingRateInt > maxFreq)
            {
                if (Enum.IsDefined(typeof(SamplingRate), maxFreq))
                {
                    res = (SamplingRate)maxFreq;
                }
                else
                {
                    requestedSamplingRateInt = maxFreq;
                    int diff = int.MaxValue;
                    foreach (SamplingRate sr in samplingRateValues)
                    {
                        int sri = (int) sr;
                        if (sri < minFreq || maxFreq != 0 && sri > maxFreq)
                        {
                            continue;
                        }
                        int d = Math.Abs(sri - requestedSamplingRateInt);
                        if (d < diff)
                        {
                            diff = d;
                            res = sr;
                        }
                    }
                }
            }
            return res;
        }

        private SamplingRate GetSupportedSamplingRate(SamplingRate sR)
        {
            switch (this.SourceType)
            {
                case InputSourceType.Microphone:
                    switch (this.MicrophoneType)
                    {
                        case MicType.Unity:
                            return this.GetSupportedSamplingRateForUnityMicrophone(sR);
                        case MicType.Photon:
                            #if UNITY_STANDALONE_WIN && !UNITY_EDITOR || UNITY_EDITOR_WIN
                            return SamplingRate.Sampling16000;
                            #elif UNITY_IOS && !UNITY_EDITOR
                            return SamplingRate.Sampling48000;
                            #elif UNITY_STANDALONE_OSX && !UNITY_EDITOR || UNITY_EDITOR_OSX
                            return SamplingRate.Sampling48000;
                            #elif UNITY_ANDROID && !UNITY_EDITOR
                            return SamplingRate.Sampling48000;
                            #else
                            return sR;
                            #endif
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case InputSourceType.AudioClip:
                    if (this.AudioClip != null)
                    {
                        return this.GetSupportedSamplingRate(this.AudioClip.frequency);
                    }
                    break;
                case InputSourceType.Factory:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return sR;
        }

        private void CheckAndSetSamplingRate(SamplingRate sR)
        {
            if (this.TrySamplingRateMatch)
            {
                SamplingRate closest = this.GetSupportedSamplingRate(sR);
                if (closest != this.samplingRate)
                {
                    if (closest != sR && this.Logger.IsWarningEnabled)
                    {
                        this.Logger.LogWarning("Sampling rate requested ({0}Hz) not supported using closest value ({1}Hz)", (int)sR, (int)closest);
                    }
                    this.samplingRate = closest;
                }
                else
                {
                    return;
                }
            }
            else if (sR != this.samplingRate)
            {
                this.samplingRate = sR;
            }
            else
            {
                return;
            }
            if (this.IsRecording)
            {
                this.RequiresRestart = true;
                if (this.Logger.IsInfoEnabled)
                {
                    this.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "SamplingRate");
                }
            }
        }

        private void CheckAndSetSamplingRate()
        {
            this.CheckAndSetSamplingRate(this.samplingRate);
        }

        #endregion

        public enum InputSourceType
        {
            Microphone,
            AudioClip,
            Factory
        }

        public enum MicType
        {
            Unity,
            Photon
        }

        [Obsolete("No longer needed. Implicit conversion is done internally when needed.")]
        public enum SampleTypeConv
        {
            None,
            Short
        }
        
        [Obsolete("Use Photon.Voice.Unity.PhotonVoiceCreatedParams")]
        public class PhotonVoiceCreatedParams : Unity.PhotonVoiceCreatedParams
        {
        }
    }
}