using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This script allows to advance the default behavior of the projector component with functions useful for the VRception toolkit
    /// Mainly, it controls which space the projector will project
    /// </summary>
    [RequireComponent(typeof(Projector))]
    public class ProjectorAdvanced : MonoBehaviour
    {
        //// SECTION "Advanced Settings"
        [Header("Advanced Settings", order = 0)]
        [Helpbox("This script allows to advance the default behavior of the projector component with functions useful for the VRception toolkit. Mainly, its purpose is to control which space the projector will project and it configures the camera to record the correct space")]
        [Tooltip("If true, then projector projects either the opposite space of the referenced interactable (for left and right space) or projects the same space of the interactable (for shared and default space).", order = 2)]
        public bool adaptToInteractableSpace = true;

        [Tooltip("Specify the interactable to which the projector will adapt, if the property 'Adapt To Interactable Space' above is true.")]
        public Interactable interactable = null;

        [Tooltip("If 'Adapt To Interactable Space' is false, the camera below is configured to look at the specified space.")]
        public Space defaultLookAtSpace = Space.SHARED;

        [Tooltip("If 'Adapt To Interactable Space' is false, the camera below is configured to ignore the specified space.")]
        public Space defaultIgnoreSpace = Space.DEFAULT;

        [Tooltip("Requires a reference to the camera that renders what the projector will project")]
        public new Camera camera = null;

        // Store reference to projector
        private Projector projector;

        // Save the last space the camera has been rendered for; to reduce number of changes to the camera
        private Space lastSpace = Space.SHARED;
        
        // Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector
        void OnValidate()
        {
            // Automatically guesses the relevant interactable
            if(this.interactable == null)
            {
                Interactable[] interactables = this.GetComponentsInParent<Interactable>();
                if(interactables.Length > 0)
                    this.interactable = interactables[0];
            }
        }

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Get a reference om the projector
            this.projector = this.GetComponent<Projector>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Create new render texture
            RenderTexture renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            // Create material from render texture
            Material material = new Material(Shader.Find("Custom/ProjectorShader"));
            material.SetTexture("_ShadowTex", renderTexture);
            material.renderQueue = 3001; // To render after all other transparent objects

            // Render camera to render texture
            this.camera.targetTexture = renderTexture;

            // Set texture of projector
            this.projector.material = material;

            this.UpdateCullingMask();
        }

        // Update is called once per frame
        void Update()
        {
            // Set culling mask of camera
            if(this.interactable != null)
            {
                // Call only if space of object changed
                if(this.interactable.GetSpace() != this.lastSpace)
                    this.UpdateCullingMask();

                // Store current space as last space
                this.lastSpace = this.interactable.GetSpace();
            }

            if(this.interactable != null && this.interactable.IsPrefabInInterface())
                this.projector.enabled = false;
            else
                this.projector.enabled = true;

            // Pass orientation of projector to shader
            this.projector.material.SetVector("_ProjectorDir", new Vector4(transform.forward.x, transform.forward.y, transform.forward.z, 0));

            // Pass configured field of view of projector
            this.projector.material.SetFloat("_FOV", this.projector.fieldOfView);
        }

        // Method that updates the culling mask of the referenced camera component to render the correct space
        private void UpdateCullingMask()
        {
            // Store which space to observe
            Space space;

            // Adapt to interactable space is activated and an interactable is referenced
            if(this.adaptToInteractableSpace && this.interactable != null)
            {
                switch(this.interactable.GetSpace())
                {
                    case Space.SHARED:
                        space = Space.SHARED;
                        this.projector.ignoreLayers = (1 << Settings.instance.layerDefault.layerIndex);
                        break;
                    // The interactable is part of the right space, thus, we render the left space
                    case Space.RIGHT:
                        space = Space.LEFT;
                        this.projector.ignoreLayers = (1 << Settings.instance.layerLeft.layerIndex);
                        break;
                    // The interactable is part of the left space, thus, we render the right space
                    case Space.LEFT:
                        space = Space.RIGHT;
                        this.projector.ignoreLayers = (1 << Settings.instance.layerRight.layerIndex);
                        break;
                    default:
                        space = Space.DEFAULT;
                        this.projector.ignoreLayers = -1; // Ignore everything
                        break;
                }
            }
            // Adapt to interactable space is disabled, thus, we render the space that has been specified
            else
            {
                space = this.defaultLookAtSpace;
                this.projector.ignoreLayers = Utilities.GetLayerOfSpace(this.defaultIgnoreSpace);
            }

            // Set the correct culling mask for camera
            this.camera.cullingMask = Utilities.GetCullingMaskOfSpace(space);
            Debug.Log("[VRception] The advanced projector component with parent interactable " + this.interactable.name + " has set the render space to " + space, this);
        }
    }
}