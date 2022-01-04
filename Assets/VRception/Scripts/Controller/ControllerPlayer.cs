﻿using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MongoDB.Bson;

namespace VRception
{
    /// <summary>
    /// This script provides the logic for player gameobjects that are used to represent the players
    /// </summary>
    [RequireComponent(typeof(References))]
    [RequireComponent(typeof(PhotonView))]
    public class ControllerPlayer : MonoBehaviourPunCallbacks
    {
        //// SECTION "Player Settings"
        [Header("Player Settings", order = 0)]
        [Helpbox("This script provides the logic for player gameobjects that are used to represent the players. In 'Network Settings' one can specify which player prefab is used. All available settings for the player character can be altered below. As the hands of the local player are generated by the Oculus plugin, it makes sense to select a character without hands for the local player.", order = 1)]
        [Tooltip("Specify the renderer that renders the character of the local player.", order = 2)]
        public Renderer localPlayerRenderer;

        [Tooltip("Specify the renderer that renders the character of the remote player.")]
        public Renderer remotePlayerRenderer;

        [Tooltip("To make the different player character distinguishable, one can specify a renderer here that is updated with the players representive color.")]
        public Renderer playerColorRenderer;

        [Tooltip("Specify the material which is updated with the players color and assigned to the renderer specifed above.")]
        public Material colorMaterial;
    
        [Tooltip("Specify the gameobject representing the head of the player. Is used to display the different headsets a player can wear.")]
        public GameObject playerHead;

        [Tooltip("Specify the camera rig gameobject here.")]
        public GameObject cameraRig;

        //// SECTION "Target References"
        [Header("Target References", order = 0)]
        [Helpbox("These references are gameobjects attached to the correct gameobjects in the OVRCameraRig. They are used for logging and replay of the player character.", order = 1)]
        [Tooltip("Specify the target gameobject that represents the head. Probably a child of CenterEyeAnchor.", order = 2)]
        public GameObject targetHead;

        [Tooltip("Specify the target gameobject that represents the left hand. Probably a child of LeftControllerAnchor.")]
        public GameObject targetLeftHand;

        [Tooltip("Specify the target gameobject that represents the right hand. Probably a child of RightControllerAnchor.")]
        public GameObject targetRightHand;

        // Stores a reference to the references script
        private References references;

        // Stores the owner of this PhotonView
        private Player owner;

        // Stores references to the gameobjects representing the different headsets
        private GameObject headsetLeft;
        private GameObject headsetLeftCenter;
        private GameObject headsetCenter;
        private GameObject headsetRightCenter;
        private GameObject headsetRight;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Get a reference on the references script
            this.references = this.GetComponent<References>();

            // Activate the camera rig
            this.cameraRig.SetActive(true); // COMMENT when using FinalIK
        }

        // Method updates the player with their color
        private void UpdateColorOfPlayer()
        {
            // Check if custom player colors are requested
            if(this.colorMaterial == null || this.playerColorRenderer == null)
                return;

            // Generate random player color
            // TODO set the same color for the same player across clients
            this.colorMaterial.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

            // Set material to player character
            this.playerColorRenderer.material = this.colorMaterial;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Attach to correct space
            ControllerSpaces.instance.MoveGameObjectToSpace(this.gameObject, Space.SHARED);

            // Save owner of this object
            this.owner = this.photonView.Owner;

            // Register player for logging
            if (ControllerLogger.instance != null)
                ControllerLogger.instance.RegisterObjectForLogging(gameObject);

            this.UpdateColorOfPlayer();

            // Check if this player is a remote player
            if (!this.photonView.IsMine)
            {
                // Set correct renderer active
                if(this.remotePlayerRenderer != null)
                    this.remotePlayerRenderer.enabled = true;
                if(this.localPlayerRenderer != null)
                    this.localPlayerRenderer.enabled = false;

                // Set correct player color
                this.UpdateColorOfPlayer();

                // Everything for the remote player has been adjusted
                return;
            }

            // Starting here, we can be sure it is a local player

            // Set correct renderer active
            if(this.remotePlayerRenderer != null)
                this.remotePlayerRenderer.enabled = false;
            if(this.localPlayerRenderer != null)
                this.localPlayerRenderer.enabled = true;

            // Set correct player color
            this.UpdateColorOfPlayer();

            // Initalize settings with references
            Settings.instance.Initalize(this.references);

            // Load all headset prefabs
            this.headsetLeft = this.InstantiatePrefab(Settings.instance.prefabHeadsetLeft);
            this.headsetLeftCenter = this.InstantiatePrefab(Settings.instance.prefabHeadsetLeftCenter);
            this.headsetCenter = this.InstantiatePrefab(Settings.instance.prefabHeadsetCenter);
            this.headsetRightCenter = this.InstantiatePrefab(Settings.instance.prefabHeadsetRightCenter);
            this.headsetRight = this.InstantiatePrefab(Settings.instance.prefabHeadsetRight);

            // Register for crossfader change
            Settings.instance.onCrossfaderChange += this.OnCrossfaderChange;
            this.OnCrossfaderChange();

        }

        // Method is called when a player has left the room
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // Only the master client is responsible for cleanup
            if (!PhotonNetwork.IsMasterClient)
                return;

            // Only if the other player was the owner of this photon view, we can continue
            if (this.owner != otherPlayer)
                return;

            // Photon Networking Destroy

            // Destroy headsets
            PhotonNetwork.Destroy(this.headsetLeft);
            PhotonNetwork.Destroy(this.headsetLeftCenter);
            PhotonNetwork.Destroy(this.headsetCenter);
            PhotonNetwork.Destroy(this.headsetRightCenter);
            PhotonNetwork.Destroy(this.headsetRight);

            // Destroy player
            PhotonNetwork.Destroy(this.gameObject);
        }

        // Method is called when a player entered the room
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            // Check if the player that has entered is me
            if (this.photonView.IsMine)
            {
                // Update parent for remote objects
                GameObject[] headsets = new GameObject[] { this.headsetLeft, this.headsetLeftCenter, this.headsetCenter, this.headsetRightCenter, this.headsetRight };
                foreach (GameObject headset in headsets)
                    if (headset != null)
                        this.photonView.RPC("SetParentRPC", RpcTarget.Others, PhotonView.Get(headset).ViewID,
                            headset.transform.localPosition, headset.transform.localRotation, headset.transform.localScale);

                // Update which of the remote objects should be active
                this.OnCrossfaderChange();
            }
        }

        // Remote procedure call: activates/deactivates gameobject via Photon View
        [PunRPC]
        public void SetViewActiveRPC(int view, bool active)
        {
            PhotonView photonView = PhotonView.Find(view);
            if (photonView != null)
                photonView.gameObject.SetActive(active);
        }

        // Remote procedure call: updates the parent of gameobject registered with a Photon View
        [PunRPC]
        public void SetParentRPC(int view, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            PhotonView photonView = PhotonView.Find(view);
            photonView.GetComponent<PhotonTransformView>().enabled = false;

            photonView.transform.parent = this.playerHead.transform;
            photonView.transform.localPosition = localPosition;
            photonView.transform.localRotation = localRotation;
            photonView.transform.localScale = localScale;
        }

        // Method to instantiate a prefab
        private GameObject InstantiatePrefab(GameObject prefab)
        {
            // Abort, if no prefab is provided
            if (prefab == null)
                return null;

            // Check if prefab has cloneable component
            Cloneable cloneable = prefab.GetComponent<Cloneable>();

            if (cloneable == null || String.IsNullOrEmpty(cloneable.prefabName))
            {
                Debug.LogError("Headset prefabs specified in 'General Settings' require a cloneable component referencing their prefab.");
                return null;
            }

            // Local transform values
            Vector3 localPosition = new Vector3(0, .0018f, -.0006f);
            Quaternion localRotation = Quaternion.Euler(0, 180, 0);
            Vector3 localScale = (1.2f / 100) * Vector3.one;

            // Prefabs should be instatiated in the shared space
            object[] info = { Space.SHARED };

            // Instantiate prefab
            GameObject obj = PhotonNetwork.Instantiate(cloneable.prefabName, Vector3.zero, Quaternion.identity, 0, info);
            obj.transform.parent = this.playerHead.transform;
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = localRotation;
            obj.transform.localScale = localScale;

            // Get photon view from prefab
            PhotonView photonViewObj = PhotonView.Get(obj);

            // Update parent for remote objects
            this.photonView.RPC("SetParentRPC", RpcTarget.Others, photonViewObj.ViewID, localPosition, localRotation, localScale);

            // Return created prefab
            return obj;
        }

        // Method listens to crossfader changes and automatically activates the correct headset
        private void OnCrossfaderChange()
        {
            float crossfader = Settings.instance.crossfader;

            if (this.headsetLeft != null)
                photonView.RPC("SetViewActiveRPC", RpcTarget.All, PhotonView.Get(this.headsetLeft).ViewID, (crossfader <= -.6f));
            if (this.headsetLeftCenter != null)
                photonView.RPC("SetViewActiveRPC", RpcTarget.All, PhotonView.Get(this.headsetLeftCenter).ViewID, (crossfader <= -.2f && crossfader > -.6f));
            if (this.headsetCenter != null)
                photonView.RPC("SetViewActiveRPC", RpcTarget.All, PhotonView.Get(this.headsetCenter).ViewID, (crossfader <= .2f && crossfader > -.2f));
            if (this.headsetRightCenter != null)
                photonView.RPC("SetViewActiveRPC", RpcTarget.All, PhotonView.Get(this.headsetRightCenter).ViewID, (crossfader <= .6f && crossfader > .2f));
            if (this.headsetRight != null)
                photonView.RPC("SetViewActiveRPC", RpcTarget.All, PhotonView.Get(this.headsetRight).ViewID, (crossfader > .6f));
        }
    }
}