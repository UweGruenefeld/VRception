using Photon.Pun;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The module delete enable players to delete this interactable in-game
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleDelete : MonoBehaviour
    {
        //// SECTION "Delete Settings"
        [Header("Delete Settings", order = 0)]
        [Helpbox("The module delete enable players to delete this interactable in-game. Only the selected interactable is deleted, none of the subinteractables or attached child interactables are deleted. Below, one can specify if the module is active.", order = 1)]
        [Tooltip("If yes, then the delete feature is active.", order = 2)]
        public bool isActive = true;

        private Interactable interactable = null;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // Only if object selected and object is up for removal
            if (this.interactable.IsSelected() && Mapping.ButtonInteractableDelete() && this.isActive)
            {
                // Unparent interactables in children
                UnparentInteractablesInChildren(this.gameObject);

                // Destroy(this.gameObject);

                // Photon Networking Destroy
                PhotonNetwork.Destroy(this.gameObject);
            }
        }

        // Method to unparent all interactables in children or deep children
        private void UnparentInteractablesInChildren(GameObject obj)
        {
            Interactable interactableInChild = obj.GetComponent<Interactable>();
            if (interactableInChild != null && !interactableInChild.isSubInteractable && this.gameObject != obj)
            {
                // Unparent this game object as it contains an interactable that cannot be ignored
                interactableInChild.GetComponent<PhotonView>().RPC("SetSpaceAndParentRPC", RpcTarget.All, interactableInChild.GetSpace(), 0);

                // Send unselect event to notify the interactable and its modules
                interactableInChild.SetSelect(false);
            }
            else
            {
                foreach (Transform transform in Utilities.FindChildrens(obj.transform))
                    this.UnparentInteractablesInChildren(transform.gameObject);
            }
        }
    }
}