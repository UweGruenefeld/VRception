#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
//#if UNITY_IOS
#define PHOTON_MICROPHONE_ENUMERATOR
#endif

namespace Photon.Voice.Unity.Demos.DemoVoiceUI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;


    public struct MicRef
    {
        public Recorder.MicType MicType;
        public string Name;
        public int PhotonId;

        public MicRef(string name, int id)
        {
            this.MicType = Recorder.MicType.Photon;
            this.Name = name;
            this.PhotonId = id;
        }

        public MicRef(string name)
        {
            this.MicType = Recorder.MicType.Unity;
            this.Name = name;
            this.PhotonId = -1;
        }

        public override string ToString()
        {
            return string.Format("Mic reference: {0}", this.Name);
        }
    }

    public class MicrophoneDropdownFiller : MonoBehaviour
    {
        private List<MicRef> micOptions;

        #pragma warning disable 649
        [SerializeField]
        private Dropdown micDropdown;

        [SerializeField]
        private Recorder recorder;

        [SerializeField]
        [FormerlySerializedAs("RefreshButton")]
        private GameObject refreshButton;

        [SerializeField]
        [FormerlySerializedAs("ToggleButton")]
        private GameObject toggleButton;
        #pragma warning restore 649

        private Toggle photonToggle;

        private void Awake()
        {
            this.photonToggle = this.toggleButton.GetComponentInChildren<Toggle>();
            this.RefreshMicrophones();
        }

        private void SetupMicDropdown()
        {
            this.micDropdown.ClearOptions();

            this.micOptions = new List<MicRef>();
            List<string> micOptionsStrings = new List<string>();

            for(int i=0; i < Microphone.devices.Length; i++)
            {
                string x = Microphone.devices[i];
                this.micOptions.Add(new MicRef(x));
                micOptionsStrings.Add(string.Format("[Unity] {0}", x));
            }

            #if PHOTON_MICROPHONE_ENUMERATOR
            if (Recorder.PhotonMicrophoneEnumerator.IsSupported)
            {
                for (int i = 0; i < Recorder.PhotonMicrophoneEnumerator.Count; i++)
                {
                    string n = Recorder.PhotonMicrophoneEnumerator.NameAtIndex(i);
                    this.micOptions.Add(new MicRef(n, Recorder.PhotonMicrophoneEnumerator.IDAtIndex(i)));
                    micOptionsStrings.Add(string.Format("[Photon] {0}", n));
                }
            }
            #endif

            this.micDropdown.AddOptions(micOptionsStrings);
            this.micDropdown.onValueChanged.RemoveAllListeners();
            this.micDropdown.onValueChanged.AddListener(delegate { this.MicDropdownValueChanged(this.micOptions[this.micDropdown.value]); });
        }

        private void MicDropdownValueChanged(MicRef mic)
        {
            this.recorder.MicrophoneType = mic.MicType;

            switch (mic.MicType)
            {
                case Recorder.MicType.Unity:
                    this.recorder.UnityMicrophoneDevice = mic.Name;
                    break;
                case Recorder.MicType.Photon:
                    this.recorder.PhotonMicrophoneDeviceId = mic.PhotonId;
                    break;
            }

            if (this.recorder.RequiresRestart)
            {
                this.recorder.RestartRecording();
            }
        }

        private void SetCurrentValue()
        {
            if (this.micOptions == null)
            {
                Debug.LogWarning("micOptions list is null");
                return;
            }
            #if PHOTON_MICROPHONE_ENUMERATOR
            bool photonMicEnumAvailable = Recorder.PhotonMicrophoneEnumerator.IsSupported;
            #else
            bool photonMicEnumAvailable = false;
            #endif
            this.photonToggle.onValueChanged.RemoveAllListeners();
            this.photonToggle.isOn = this.recorder.MicrophoneType == Recorder.MicType.Photon;
            if (!photonMicEnumAvailable)
            {
                this.photonToggle.onValueChanged.AddListener(this.PhotonMicToggled);
            }
            this.micDropdown.gameObject.SetActive(photonMicEnumAvailable || this.recorder.MicrophoneType == Recorder.MicType.Unity);
            this.toggleButton.SetActive(!photonMicEnumAvailable);
            this.refreshButton.SetActive(photonMicEnumAvailable || this.recorder.MicrophoneType == Recorder.MicType.Unity);
            for (int valueIndex = 0; valueIndex < this.micOptions.Count; valueIndex++)
            {
                MicRef val = this.micOptions[valueIndex];
                if (this.recorder.MicrophoneType == val.MicType)
                {
                    if (this.recorder.MicrophoneType == Recorder.MicType.Unity &&
                        Recorder.CompareUnityMicNames(val.Name, this.recorder.UnityMicrophoneDevice))
                    {
                        this.micDropdown.value = valueIndex;
                        return;
                    }
                    if (this.recorder.MicrophoneType == Recorder.MicType.Photon &&
                        val.PhotonId == this.recorder.PhotonMicrophoneDeviceId)
                    {
                        this.micDropdown.value = valueIndex;
                        return;
                    }
                }
            }
            for (int valueIndex = 0; valueIndex < this.micOptions.Count; valueIndex++)
            {
                MicRef val = this.micOptions[valueIndex];
                if (this.recorder.MicrophoneType == val.MicType)
                {
                    if (this.recorder.MicrophoneType == Recorder.MicType.Unity)
                    {
                        this.micDropdown.value = valueIndex;
                        this.recorder.UnityMicrophoneDevice = val.Name;
                        break;
                    }
                    if (this.recorder.MicrophoneType == Recorder.MicType.Photon)
                    {
                        this.micDropdown.value = valueIndex;
                        this.recorder.PhotonMicrophoneDeviceId = val.PhotonId;
                        break;
                    }
                }
            }
            if (this.recorder.RequiresRestart)
            {
                this.recorder.RestartRecording();
            }
        }

        public void PhotonMicToggled(bool on)
        {
            this.micDropdown.gameObject.SetActive(!on);
            this.refreshButton.SetActive(!on);
            if (on)
            {
                this.recorder.MicrophoneType = Recorder.MicType.Photon;
            }
            else
            {
                this.recorder.MicrophoneType = Recorder.MicType.Unity;
            }
            
            if (this.recorder.RequiresRestart)
            {
                this.recorder.RestartRecording();
            }
        }

        public void RefreshMicrophones()
        {
            #if PHOTON_MICROPHONE_ENUMERATOR
            //Debug.Log("Refresh Mics");
            Recorder.PhotonMicrophoneEnumerator.Refresh();
            #endif
            this.SetupMicDropdown();
            this.SetCurrentValue();
        }

        // sync. UI in case a change happens from the Unity Editor Inspector
        private void PhotonVoiceCreated()
        {
            this.RefreshMicrophones();
        }
    }
}