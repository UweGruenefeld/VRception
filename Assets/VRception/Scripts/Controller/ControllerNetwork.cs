using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace VRception
{
    /// <summary>
    /// This class bundles all functionality related to the networking functionality of the VRception toolkit.
    /// </summary>
    public class ControllerNetwork : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        [Header("Network Settings", order = 0)]
        [Helpbox("The network feature of the VRception toolkit is implemented with Photon Unity Networking (www.photonengine.com). In the current version of VRception, it is not possible to turn of this feature, meaning a network connection to the photon servers is required. The specified room must be a unique string because everyone using the same room name joins into the same room (until a new room is opened because the maximum number of players for that room is reached).", order = 1)]
        [Tooltip("Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.", order = 2)]
        public bool autoConnect = true;
        [Tooltip("Name of the Photon Room.")]
        public string roomName = "VRCeption";
        [Tooltip("Used as PhotonNetwork.GameVersion.")]
        public byte version = 1;
        [Tooltip("The max number of players allowed in room. Once full, a new room will be created by the next connection attemping to join.")]
		public byte maxPlayers = 4;
        [Tooltip("Time To Live (TTL) for an 'actor' in a room. If a client disconnects, this actor is inactive first and removed after this timeout. In milliseconds.")]
        public int playerTTL = -1;

        void Start()
        {
            // If automatic connect is enabled, than connect
            if (this.autoConnect)
                this.ConnectNow();
        }

        void ConnectNow()
        {
            Debug.Log("[VRception] The toolkit will now try to connect to the Photon servers.", this);

            // Start connection
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.version + "." + SceneManagerHelper.ActiveSceneBuildIndex;

            // Setting up default prefab pool for networking 
            DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;

            // Add prefabs from settings
            if (pool != null)
            {
                // Load marker prefab (specified directly in ControllerExperiences) into prefab pool
                if(ControllerExperiences.instance.markerDefaultPrefab != null)
                    if(!pool.ResourceCache.ContainsKey(ControllerExperiences.instance.markerDefaultPrefab.name))
                        pool.ResourceCache.Add(ControllerExperiences.instance.markerDefaultPrefab.name, ControllerExperiences.instance.markerDefaultPrefab);
                
                // Load marker prefabs (specified in the individual Marker gameobjects) into prefab pool
                foreach(Transform child in ControllerExperiences.instance.GetGameObject().transform)
                {
                    Marker marker = child.GetComponent<Marker>();
                    if(marker != null && marker.customPrefab != null && !pool.ResourceCache.ContainsKey(marker.customPrefab.name))
                        pool.ResourceCache.Add(marker.customPrefab.name, marker.customPrefab);
                }

                // Load interface prefabs into prefab pool
                if(Settings.instance.predefinedObjects != null)
                    foreach (GameObject prefab in Settings.instance.predefinedObjects)
                        if(prefab != null && !pool.ResourceCache.ContainsKey(prefab.name))
                            pool.ResourceCache.Add(prefab.name, prefab);

                // Load headset prefabs into prefab pool
                GameObject[] headsets = new GameObject[] {
                    Settings.instance.prefabHeadsetLeft,
                    Settings.instance.prefabHeadsetLeftCenter,
                    Settings.instance.prefabHeadsetCenter,
                    Settings.instance.prefabHeadsetRightCenter,
                    Settings.instance.prefabHeadsetRight
                };
                foreach (GameObject prefab in headsets)
                    if(prefab != null && !pool.ResourceCache.ContainsKey(prefab.name))
                        pool.ResourceCache.Add(prefab.name, prefab);
            }
        }

        // Method is called to join a room
        void JoinRoom()
        {
            // Set room options
            RoomOptions roomOptions = new RoomOptions
            {
                IsVisible = false,
                MaxPlayers = this.maxPlayers,
                CleanupCacheOnLeave = false
            };

            // Set time to life
            if (this.playerTTL >= 0)
                roomOptions.PlayerTtl = this.playerTTL;

            // Try joining or creating the room
            PhotonNetwork.JoinOrCreateRoom(this.roomName, roomOptions, TypedLobby.Default);
        }

        // Method is called when the client is connected to the master server
        public override void OnConnectedToMaster()
        {
            Debug.Log("[VRception] The player is now connected to the Photon server in region [" + PhotonNetwork.CloudRegion + "].", this);

            this.JoinRoom();
        }

        // Method is called when the client is connected to the lobby
        public override void OnJoinedLobby()
        {
            Debug.Log("[VRception] The player is now connected to the Photon relay in region [" + PhotonNetwork.CloudRegion + "].", this);
            
            this.JoinRoom();
        }

        // Method is called upon disconnect of the client
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("[VRception] The player is now disconnected from the Photon server: " + cause + ".", this);
        }

        // Method is called when the player has sucessfully joined a room
        public override void OnJoinedRoom()
        {
            Debug.Log("[VRception] The player is now in a room in region [" + PhotonNetwork.CloudRegion + "]. Networking is all set up.");

            // Inform that loading is complete
            Settings.instance.LoadingFinished();
        }
        
        // Method is called when a change in ownership is requested
        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            Debug.Log("[VRception] Incoming ownership request from player " + requestingPlayer + " asking for ownership of " + targetView + ".", this);

            // Check if it is my photon view
            if (targetView.IsMine)
            {
                // Try to get interactable from photon view
                Interactable interactable = targetView.GetComponent<Interactable>();
                if(interactable != null && interactable.IsSelected())
                    // Deny request as user currently has the interactable selected
                    return;

                // Grant ownership of photon view
                targetView.TransferOwnership(requestingPlayer);
            }
        }

        // Method is called when ownership is transfered
        public void OnOwnershipTransfered(PhotonView targetView, Player requestingPlayer)
        {

        }
    }
}