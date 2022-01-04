using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The interface module is required for interactables that are shown in the list of the menu interface.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleInterface : MonoBehaviour, IEventSelect
    {
        //// SECTION "Interface Settings"
        [Header("Interface Settings", order = 0)]
        [Helpbox("The interface module is required for interactables that are shown in the list of the menu interface. Basically, the module shrinks the predefined object to a smaller size (see 'Preview Scale' below) and as soon as a player selects it, a duplicate of the predefined object with the original scale is created and selected by the player instead. Thus, the previewed predefined object can never really be selected and remove from the list.", order = 1)]
        [Tooltip("Specify ", order = 2)]
        public Vector3 previewScale = Vector3.one;

        private Interactable interactable = null;
        private Vector3 originalScale = Vector3.one;
        private bool isSelected = false;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Get assigned interactable
            this.interactable = this.GetComponent<Interactable>();

            // Configure interactable to not be logged and stored for replay
            this.interactable.isLogging = false;

            // Set prefab for list in menu interface to preview size
            this.originalScale = this.interactable.modelGameObject.transform.localScale;
            this.interactable.modelGameObject.transform.localScale = this.previewScale;
        }

        // Update is called once per frame
        void Update()
        {
            if(this.isSelected)
            {
                // Get controller position
                Vector3 controllerPosition = Mapping.Axis3DInteractableControllerPosition();

                // Get controller rotation
                Vector3 controllerRotation = this.transform.rotation.eulerAngles;

                // Unselect this interactable (it needs to remain in the menu)
                this.interactable.Reset();
                this.isSelected = false;

                // Create a clone of the object
                GameObject clone = Utilities.CreateClone(this.gameObject, controllerPosition, controllerRotation);

                // Get interactable of game object
                Interactable cloneInteractable = clone.GetComponent<Interactable>();

                // Scale interactable to original scale
                cloneInteractable.modelGameObject.transform.localScale = this.originalScale;

                // Select the cloned object instead
                cloneInteractable.SetSelect(true);
            }

            this.interactable.SetSpace();
        }

        // Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
        void OnDestroy()
        {
            // Register object for logging
            this.interactable.isLogging = true;
        }

        // Event method that is invoked when this interactable is selected
        public void OnSelectEnter(bool isChild)
        {
            if(isChild)
                return;
                
            this.isSelected = true;
        } 

        // Event method that is invoked when this interactable is not selected anymore
        public void OnSelectExit(bool isChild)
        {

        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}
