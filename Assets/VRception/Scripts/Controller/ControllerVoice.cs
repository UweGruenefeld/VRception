using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

namespace VRception
{
    /// <summary>
    /// This scripts helps with setting up the Photon voice chat and configuring it correctly
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class ControllerVoice : MonoBehaviourPunCallbacks
    {
        //// SECTION "Voice Settings"
        [Header("Voice Settings", order = 0)]
        [Helpbox("This scripts helps with setting up the Photon voice chat and configuring it correctly.", order = 1)]
        [Tooltip("Specify the recorder used for the voice chat.", order = 2)]
        public Recorder Recorder;

        [Tooltip("Specify the speaker used for the voice chat.")]
        public Speaker Speaker;

        // Stores an interal referenceto the Photon Voice View
        private PhotonVoiceView photonVoiceView;

        // Start is called before the first frame update
        void Awake()
        {
            // Get reference
            photonVoiceView = GetComponent<PhotonVoiceView>();

            if (this.photonView.IsMine)
            {
                photonVoiceView.RecorderInUse = Recorder;
                photonVoiceView.SpeakerInUse = null;

                this.Recorder.gameObject.SetActive(true);
                this.Speaker.gameObject.SetActive(false);

                photonVoiceView.UsePrimaryRecorder = true;
                photonVoiceView.AutoCreateRecorderIfNotFound = false;
            }
            else
            {
                this.Recorder.gameObject.SetActive(false);
                this.Speaker.gameObject.SetActive(true);

                photonVoiceView.RecorderInUse = null;
                photonVoiceView.SpeakerInUse = Speaker;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(Mapping.ButtonVoice() && this.photonView.IsMine)
            {
                Recorder.IsRecording = !Recorder.IsRecording;

                if(Recorder.IsRecording)
                    Settings.instance.SetToast("Microphone is activated");
                else
                    Settings.instance.SetToast("Microphone is deactivated");
            }

            /*
            DEBUGGING INFORMATION
            print("IsPhotonViewReady: " + photonVoiceView.IsPhotonViewReady);
            print("IsSetup: " + photonVoiceView.IsSetup);
            print("IsRecorder: " + photonVoiceView.IsRecorder); // local only
            print("IsRecording: " + photonVoiceView.IsRecording); // local only
            print("IsSpeaker: " + photonVoiceView.IsSpeaker);
            print("IsSpeakerLinked: " + photonVoiceView.IsSpeakerLinked);
            print("IsSpeaking: " + photonVoiceView.IsSpeaking);
            */
        }
    }
}