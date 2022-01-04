using UnityEngine;

// Maybe replace with ParentConstraints https://docs.unity3d.com/Manual/class-ParentConstraint.html
namespace VRception
{
    /// <summary>
    /// The module parent allows to specify a parent from another space that is than used as the parent of this interactable 
    /// For example, to attach a interactable from reality to another interactable from virtuality
    /// </summary>
    public class ModuleParent : MonoBehaviour, IEventSelect
    {
        //// SECTION "Parent Settings"
        [Header("Parent Settings", order = 0)]
        [Helpbox("The module parent allows to specify a parent from another space that is than used as the parent of this interactable. For example, to attach a interactable from reality to another interactable from virtuality. Below, one can specify the transform of the parent. If no parent is assigned, the module is not active.", order = 1)]
        [Tooltip("Specify the parent from another space that this interactable should use.", order = 2)]
        public Transform parent = null;

        private bool isInitialized;
        private Vector3 localPosition;
        private Quaternion localRotation;

        // Start is called before the first frame update
        void Start()
        {
            // To follow the game object specified as parent, this object should be a root game object
            this.transform.parent = null;

            this.isInitialized = false;
            this.localPosition = Vector3.zero;
            this.localRotation = Quaternion.identity;
        }

        // Update is called once per frame
        public void Update()
        {
            // Initialize module
            if(!this.isInitialized && this.parent != null)
            {
                this.localPosition = this.parent.InverseTransformPoint(this.transform.position);
                this.localRotation = Quaternion.Inverse(this.parent.rotation) * this.transform.rotation;
                this.isInitialized = true;
            }

            // If initialized but no parent object, then destroy this module
            if(this.isInitialized && this.parent == null)
            {
                DestroyImmediate(this);
                return;
            }

            // If someone changed the parent, then destroy this module
            if(this.transform.parent != null)
            {
                DestroyImmediate(this);
                return;
            }

            // Update the position of this object to follow the parent
            this.transform.position = this.parent.TransformPoint(this.localPosition);

            // Update the rotation of this object to rotate with the parent
            this.transform.rotation = parent.rotation * this.localRotation;
        }

        // Event method that is invoked when this interactable is selected
        public void OnSelectEnter(bool isChild)
        {
            if(isChild)
                return;

            // If selected, the destroy this component
            DestroyImmediate(this);
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