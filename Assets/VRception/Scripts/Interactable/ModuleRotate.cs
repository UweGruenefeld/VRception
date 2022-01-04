using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The module rotate enables players to rotate the interactable. 
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleRotate : MonoBehaviour, IEventSelect
    {
        //// SECTION "Rotate Settings"
        [Header("Rotate Settings", order = 0)]
        [Helpbox("The module rotate enables players to rotate the interactable. Below, one can select on which euler angle the interactable can be rotated.", order = 1)]
        [Tooltip("Specify if interactable can be rotated on the x-angle.", order = 2)]
        public bool rotateX = true;

        [Tooltip("Specify if interactable can be rotated on the y-angle.")]
        public bool rotateY = true;

        [Tooltip("Specify if interactable can be rotated on the z-angle.")]
        public bool rotateZ = true;

        private Interactable interactable = null;

        private Quaternion initialControllerOrientation;
        private Quaternion initialObjectOrientation;


        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();

            this.initialControllerOrientation = Quaternion.identity;
            this.initialObjectOrientation = Quaternion.identity;
        }

        // Update is called once per frame
        void Update()
        {
            if (this.interactable.IsSelected() && !this.interactable.IsPrefabInInterface())
            {
                // Get controller rotation
                Quaternion controllerRotation = this.ApplyConstraints(Mapping.Axis3DInteractableControllerOrientation());
                
                // Update the position and rotation of the object according to the controller
                this.transform.rotation = Quaternion.Inverse(this.initialControllerOrientation * Quaternion.Inverse(controllerRotation)) * this.initialObjectOrientation;
            }
        }

        // Event method that is invoked when this interactable is selected
        public void OnSelectEnter(bool isChild)
        {
            if(isChild)
                return;

            // Store controller orientation on select
            this.initialControllerOrientation = this.ApplyConstraints(Mapping.Axis3DInteractableControllerOrientation());

            // Store object orientation on select
            this.initialObjectOrientation = this.transform.rotation;
        }

        // Event method that is invoked when this interactable is not selected anymore
        public void OnSelectExit(bool isChild)
        {

        }

        // Method applies the configured constraints to a quaternion
        public Quaternion ApplyConstraints(Quaternion quaternion)
        {
            Vector3 angles = quaternion.eulerAngles;

            if (!this.rotateX)
                angles.x = 0;
            if (!this.rotateY)
                angles.y = 0;
            if (!this.rotateZ)
                angles.z = 0;

            return Quaternion.Euler(angles.x, angles.y, angles.z);
        }

        // Returns the initial controller rotation
        public Quaternion GetInitialControllerOrientation()
        {
            return this.initialControllerOrientation;
        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}