using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The module duplicate enable players to duplicate this interactable in-game
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleDuplicate : MonoBehaviour
    {
        //// SECTION "Duplicate Settings"
        [Header("Duplicate Settings", order = 0)]
        [Helpbox("The module duplicate enable players to duplicate this interactable in-game. Only the selected interactable is duplicated, none of the subinteractables or attached child interactables are duplicated. Below, one can specify if the module is active.", order = 1)]
        [Tooltip("If yes, then the duplicate feature is active.", order = 2)]
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
            // Only if object selected 
            if(this.interactable.IsSelected() && Mapping.ButtonInteractableDuplicate() && this.isActive)
            {
                // Get controller position
                Vector3 controllerPosition = Mapping.Axis3DInteractableControllerPosition();

                // Get controller rotation
                Vector3 controllerRotation = Mapping.Axis3DInteractableControllerRotation();

                // Unselect this interactable
                this.interactable.SetSelect(false);

                // Create a clone of the object
                GameObject clone = Utilities.CreateClone(this.gameObject, controllerPosition, this.gameObject.transform.rotation.eulerAngles);

                // Get interactable of cloned game object
                Interactable cloneInteractable = clone.GetComponent<Interactable>();

                // Select the cloned object instead
                cloneInteractable.SetSelect(true);
            }
        }
    }
}