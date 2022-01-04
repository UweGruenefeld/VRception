using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This script allows to advance the default behavior of the camera component with functions useful for the VRception toolkit
    /// It is not used with the two main cameras that continously render the left and right space (reality, virtuality)
    /// Rather for cameras involved in the design of cross-reality prototypes (e.g., it is used for the display and projector predefined objects)
    /// Mainly, it controls which space the camera will render
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraAdvanced : MonoBehaviour
    {
        //// SECTION "Advanced Settings"
        [Header("Advanced Settings", order = 0)]
        [Helpbox("This script allows to advance the default behavior of the camera component with functions useful for the VRception toolkit. Mainly, its purpose is to control which space the camera will render and it configures the camera to render to a 'RenderTexture' that is set as the material of a below specified 'MeshRenderer.'", order = 1)]
        [Tooltip("If true, then camera renders either the opposite space of the referenced interactable (for left and right space) or renders the same space of the interactable (for shared and default space).", order = 2)]
        public bool adaptToInteractableSpace = true;

        [Tooltip("Specify the interactable to which the camera will adapt, if the property 'Adapt To Interactable Space' above is true.")]
        public Interactable interactable = null;

        [Tooltip("If 'Adapt To Interactable Space' is false, the camera is configured to look at the specified space.")]
        public Space defaultLookAtSpace = Space.SHARED;

        [Tooltip("Specify the 'MeshRenderer' for which the material will be updated with the rendering of this camera.")]
        public MeshRenderer meshRenderer = null;
        
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

        // Start is called before the first frame update
        void Start()
        {
            // Get camera component
            Camera camera = this.GetComponent<Camera>();

            // Create new render texture
            RenderTexture renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            // Create material from render texture
            Material material = new Material(Shader.Find("Custom/TransparentShader"));
            material.SetTexture("_MainTex", renderTexture);

            // Render camera to render texture
            camera.targetTexture = renderTexture;

            // Set mesh renderer material
            if(meshRenderer != null)
                meshRenderer.material = material;

            // Update culling mask
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
        }

        // Method that is called before the attached camera component renders a frame
        void OnPreRender()
        {
            switch(this.interactable.GetSpace())
            {
                case Space.LEFT:
                    // Set skybox of right space
                    RenderSettings.skybox = ControllerSpaces.instance.skyboxRight;
                    break;
                case Space.RIGHT:
                    // Set skybox of left space
                    RenderSettings.skybox = ControllerSpaces.instance.skyboxLeft;
                    break;
                default:
                    // Set skybox to null
                    RenderSettings.skybox = null;
                    break;
            }
        }

        // Method that is called after the attached camera component rendered a frame
        void OnPostRender()
        {
            if(Settings.instance.crossfader > 0)
                // Set skybox of right space
                RenderSettings.skybox = ControllerSpaces.instance.skyboxRight;
            else
                // Set skybox of left space
                RenderSettings.skybox = ControllerSpaces.instance.skyboxLeft;
        }

        // Method that updates the culling mask of the attached camera component to render the correct space
        private void UpdateCullingMask()
        {
            // Store which space to observe
            Space space;

            // Adapt to interactable space is activated and an interactable is referenced
            if(this.adaptToInteractableSpace && this.interactable != null)
            {
                // Get the space of the referenced interactable
                switch(this.interactable.GetSpace())
                {
                    // The interactable is part of the right space, thus, we render the left space
                    case Space.RIGHT:
                        space = Space.LEFT;
                        break;
                    // The interactable is part of the left space, thus, we render the right space
                    case Space.LEFT:
                        space = Space.RIGHT;
                        break;
                    // The interactable is either in the shared or default space, thus, we render that space as well
                    default:
                        space = this.interactable.GetSpace();
                        break;
                }
            }
            // Adapt to interactable space is disabled, thus, we render the space that has been specified
            else
                space = this.defaultLookAtSpace;

            // Set the correct culling mask for camera
            this.GetComponent<Camera>().cullingMask = Utilities.GetCullingMaskOfSpace(space);
            Debug.Log("[VRception] The advanced camera component with parent interactable " + this.interactable.name + " has set the render space to " + space, this);
        }
    }
}