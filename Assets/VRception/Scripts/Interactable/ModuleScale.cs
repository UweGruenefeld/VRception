using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The module scale enables players to scale the interactable.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleScale : MonoBehaviour
    {
        //// SECTION "Scale Settings"
        [Header("Scale Settings", order = 0)]
        [Helpbox("The module scale enables players to scale the interactable. For scaling to work properly, it is important that no meshes are attached to the interactable directly but rather everything is attached to a child gameobject of the interactable that is refered in the settings of the 'interactable' itself. Below, one can select on which axis the interactable can be scaled.", order = 1)]
        [Tooltip("Specify if interactable can be scaled on the x-axis.", order = 2)]
        public bool scaleX = true;

        [Tooltip("Specify if interactable can be scaled on the y-axis.")]
        public bool scaleY = true;

        [Tooltip("Specify if interactable can be scaled on the z-axis.")]
        public bool scaleZ = true;

        private Interactable interactable;
        private GameObject model;

        private Vector3 initialControllerPosition;
        private Vector3 initialObjectScale;

        private bool isScaling;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();
            this.model = this.interactable.modelGameObject;

            this.initialControllerPosition = Vector3.zero;
            this.initialObjectScale = Vector3.one;

            this.isScaling = false;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // Is object selected and scaling button is pressed?
            if(this.interactable.IsSelected() && Mapping.ButtonInteractableScale() && !this.interactable.IsPrefabInInterface())
            {
                // Get relevant controller position
                Vector3 controllerPosition = this.transform.InverseTransformPoint(Mapping.Axis3DInteractableScale());
                controllerPosition = new Vector3(Mathf.Abs(controllerPosition.x), Mathf.Abs(controllerPosition.y), Mathf.Abs(controllerPosition.z));

                // First frame in which scaling is activated
                if(!this.isScaling)
                {
                    this.isScaling = true;

                    this.initialControllerPosition = controllerPosition;
                    this.initialObjectScale = this.model.transform.localScale;
                }

                // Calculate change in controller movement
                Vector3 controllerMovement = controllerPosition - this.initialControllerPosition;

                // Apply constraints
                if(!this.scaleX)
                    controllerMovement.x = 0;
                if(!this.scaleY)
                    controllerMovement.y = 0;
                if(!this.scaleZ)
                    controllerMovement.z = 0;

                // Caluclate new scale of object
                Vector3 scale = this.initialObjectScale;
                scale += controllerMovement * Settings.instance.speedScale;
                scale = new Vector3(this.WithinRange(scale.x), this.WithinRange(scale.y), this.WithinRange(scale.z));

                // Set new scale for object
                this.model.transform.localScale = scale;
            }
            // Not selected for scaling
            else
                this.isScaling = false;
        }

        // Method to clip the scaling factor to the values specified in "General Settings"
        private float WithinRange(float value)
        {
            if(value > Settings.instance.maximumScale)
                return Settings.instance.maximumScale;
            if(value < Settings.instance.minimumScale)
                return Settings.instance.minimumScale;
            return value;
        }
    }
}