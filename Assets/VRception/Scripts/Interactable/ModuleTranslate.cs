using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The module translate enables players to translate an interactable if selected (in the global coordinate system).
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleTranslate : MonoBehaviour, IEventSelect
    {
        //// SECTION "Translate Settings"
        [Header("Translate Settings", order = 0)]
        [Helpbox("The module translate enables players to translate an interactable if selected (in the global coordinate system). Different to the 'ModuleTranslateLocal,' this module enables to reposition interactables in the global coordinate system. The translation axes that can be selected below represent the ones of the global coordinate system.", order = 1)]
        [Tooltip("If yes, the interactable can be translated on the x-axis.", order = 2)]
        public bool translateX = true;

        [Tooltip("If yes, the interactable can be translated on the y-axis.")]
        public bool translateY = true;

        [Tooltip("If yes, the interactable can be translated on the z-axis.")]
        public bool translateZ = true;

        private Interactable interactable = null;

        private Vector3 initialControllerPosition;
        private Vector3 initialObjectPosition;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();

            this.initialControllerPosition = Vector3.zero;
            this.initialObjectPosition = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            if (this.interactable.IsSelected() && !this.interactable.IsPrefabInInterface())
            {
                // Get controller position
                Vector3 controllerPosition = this.ApplyConstraints(Mapping.Axis3DInteractableControllerPosition());

                // Offset between object and controller
                Vector3 offsetObjectController = this.initialObjectPosition - this.initialControllerPosition;

                // Current Tracking Space rotation
                // TODO: must be used if tracking space is recalibrated by the user. Otherwise we get weird rotations!
                Quaternion trackingSpaceRotation = Settings.instance.GetReferences().trackingSpace.transform.rotation;

                // Is a rotation component attached?
                ModuleRotate rotate = this.GetComponent<ModuleRotate>();
                if (rotate != null)
                {
                    // Get relative controller rotation
                    Quaternion controllerRotation = rotate.ApplyConstraints(Mapping.Axis3DInteractableControllerOrientation());
                    controllerRotation = rotate.GetInitialControllerOrientation() * Quaternion.Inverse(controllerRotation);

                    // Rotate support vector with rotation from controller
                    offsetObjectController = Quaternion.Inverse(controllerRotation) * offsetObjectController;
                }

                // Update the position of the object according to the controller
                this.transform.position = offsetObjectController + controllerPosition;
            }
        }

        // Event method that is invoked when this interactable is selected
        public void OnSelectEnter(bool isChild)
        {
            if(isChild)
                return;

            // Store controller position on select
            this.initialControllerPosition = this.ApplyConstraints(Mapping.Axis3DInteractableControllerPosition());

            // Store object position on select
            this.initialObjectPosition = this.transform.position;
        }

        // Event method that is invoked when this interactable is not selected anymore
        public void OnSelectExit(bool isChild)
        {

        }

        // Method applies the configured constraints to a vector3
        private Vector3 ApplyConstraints(Vector3 translation)
        {
            if (!this.translateX)
                translation.x = 0;
            if (!this.translateY)
                translation.y = 0;
            if (!this.translateZ)
                translation.z = 0;

            return translation;
        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}