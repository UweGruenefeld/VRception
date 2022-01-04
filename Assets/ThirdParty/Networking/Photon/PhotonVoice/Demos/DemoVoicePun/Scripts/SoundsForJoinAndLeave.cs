using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SoundsForJoinAndLeave : MonoBehaviourPunCallbacks
{
    public AudioClip JoinClip;
    public AudioClip LeaveClip;
    private AudioSource source;

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (this.JoinClip != null)
        {
            if (this.source == null) this.source = FindObjectOfType<AudioSource>();
            this.source.PlayOneShot(this.JoinClip);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (this.LeaveClip != null)
        {
            if (this.source == null) this.source = FindObjectOfType<AudioSource>();
            this.source.PlayOneShot(this.LeaveClip);
        }
    }
}