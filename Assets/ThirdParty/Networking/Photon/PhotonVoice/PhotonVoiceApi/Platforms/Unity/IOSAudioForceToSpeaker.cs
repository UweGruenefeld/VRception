using UnityEngine;

namespace Photon.Voice.Unity
{
    public class IOSAudioForceToSpeaker : MonoBehaviour
    {
#if UNITY_IOS
	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern void Photon_IOSAudio_ForceToSpeaker();
#endif

        void PhotonVoiceCreated()
        {
#if UNITY_IOS
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			Photon_IOSAudio_ForceToSpeaker();
			Debug.Log("iOS audio output forced to speaker");
		}
#endif
        }
    }
}