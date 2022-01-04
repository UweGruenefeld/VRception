using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The module translate enables players to translate an interactable if selected (in the local coordinate system).
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleTranslateLocal : MonoBehaviour, IEventSelect
    {
        //// SECTION "Translate Settings"
        [Header("Translate Settings", order = 0)]
        [Helpbox("The module translate enables players to translate an interactable if selected (in the local coordinate system). Different to the 'ModuleTranslate,' this module enables to reposition interactables in the local coordinate system (of this interactable). The translation axes that can be selected below represent the ones of the local coordinate system.", order = 1)]
        [Tooltip("If yes, the interactable can be translated on the x-axis.", order = 2)]
        public bool translateX = true;

        [Tooltip("If yes, the interactable can be translated on the y-axis.")]
        public bool translateY = true;

        [Tooltip("If yes, the interactable can be translated on the y-axis.")]
        public bool translateZ = true;

        //// SECTION "Clipping Settings"
        [Header("Clipping Settings", order = 0)]
        [Helpbox("This module allows one to restrict the range of values in each axis below. If enabled, the minimum and maximum value that applies to all axes can be specified below.", order = 1)]
        [Tooltip("If yes, clipping of the values on each axis is enabled.", order = 2)]
        public bool clipValues = false;

        [Tooltip("Specify the maximum value that each axis is allowed to have.")]
        public float maxValue = 1;

        [Tooltip("Specify the minimum value that each axis is allowed to have.")]
        public float minValue = -1;

        //// SECTION "Snapping Settings"
        [Header("Snapping Settings", order = 0)]
        [Helpbox("This module allows one to snap the values of each axis towards zero. If enabled, the values on each axis that fall below the positive/negative tolerance specified below will automatically be adjusted to be zero.", order = 1)]
        [Tooltip("If yes, values of all axis will snap towards zero if the fall below the tolerance,", order = 2)]
        public bool snapValues = false;

        [Tooltip("Specify the tolerance (value is used as positive and negative tolerance) to snap the values of each axis to zero.")]
        public float tolerance = 0.1f;

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
            if(this.interactable.IsSelected())
            {
                // Get controller position
                Vector3 controllerPosition = Mapping.Axis3DInteractableControllerPosition();

                // Calculate new position
                Vector3 position = Quaternion.Inverse(this.transform.rotation) * (controllerPosition - this.initialControllerPosition);
                position += this.initialObjectPosition;

                if(!this.translateX)
                    position.x = 0;
                if(!this.translateY)
                    position.y = 0;
                if(!this.translateZ)
                    position.z = 0;

                if(this.clipValues)
                {
                    position.x = this.ClipValue(position.x);
                    position.y = this.ClipValue(position.y);
                    position.z = this.ClipValue(position.z);
                }

                if(this.snapValues)
                {
                    position.x = this.SnapValue(position.x);
                    position.y = this.SnapValue(position.y);
                    position.z = this.SnapValue(position.z);
                }

                // Update the position of the object according to the controller
                this.transform.localPosition = position;
            }
        }

        // Event method that is invoked when this interactable is selected
        public void OnSelectEnter(bool isChild)
        {
            if(isChild)
                return;

            // Store controller position on select
            this.initialControllerPosition = Mapping.Axis3DInteractableControllerPosition();

            // Store initial position of object
            this.initialObjectPosition = this.transform.localPosition;
        }
        
        // Event method that is invoked when this interactable is not selected anymore
        public void OnSelectExit(bool isChild)
        {

        }

        // Method that clips off values higher than the meaximum or smaller than the minimum
        private float ClipValue(float value)
        {
            if(value > this.maxValue)
                return this.maxValue;
            if(value < this.minValue)
                return this.minValue;

            return value;
        }

        // Method lets values snap to zero if they are within the range of the tolerance
        private float SnapValue(float value)
        {
            if(value < this.tolerance && value > -this.tolerance)
                return 0;
            return value;
        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}