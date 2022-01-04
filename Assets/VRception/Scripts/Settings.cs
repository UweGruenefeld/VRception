using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This class bundles the general settings of the VRception toolkit and provides the fundamental program flow (e.g., switching between modes, crossfader logic).
    /// </summary>
    public class Settings : MonoBehaviour
    {
        [HideInInspector]
        public static Settings instance;

        //// SECTION "Current State"
        [Header("Current State", order = 0)]
        [Helpbox("The variables 'Mode' and 'Playing' reflect the current state of the VRception toolkit. Different modes are available between which one can switch and that each offer their own set of functions in-game (e.g., 'SIMULATION' is the default mode in which player can use the crossfader to control the simulation while 'CALIBRATION' allows to adjust the position and orientation of spaces to adapt them to the players real-world environment).", order = 1)]
        [Tooltip("Specifying the current active mode.", order = 2)]
        public Mode mode = Mode.SIMULATION;

        [Tooltip("If selected, the simulation will play and time will run.")]
        public bool playing = false;
        
        //// SECTION "Crossfader Settings"
        [Header("Crossfader Settings", order = 0)]
        [Helpbox("The crossfader is one of the core functionalities of the VRception toolkit. It enables players to remix the configured spaces via alpha-blending (e.g., players can transition from reality to virtuality). Basically, it represent the Reality-Virtuality Continuum from Kishino and Milgram; here, ranging from -1 (left) to 1 (right).", order = 1)]
        [Tooltip("Representing the position on the continuum between left and right.", order = 2)]
        [Range(-1, 1, order = 3)]
        public float crossfader = 0;

        [Tooltip("Define how sensitive the crossfader is to the thumbstick input.")]
        [Range(0.01f, 1)]
        public float faderSensitivity = 0.1f;

        [Tooltip("Define the range around the center of the crossfader in which it jumps to zero.")]
        [Range(0,0.2f)]
        public float snapToCenter = 0.04f;

        //// SECTION "Headset Settings"
        [Header("Headset Settings", order = 0)]
        [Helpbox("Different headset prefabs can be used to indicate what crossfader setting (continuum manifestation) a player currently experiences. For simplicity, we divided the continuum into five different areas whereas for each area one can specifiy a prefab below.", order = 1)]
        [Tooltip("If enabled, a headset is rendered for the player dependent on the crossfader position.", order = 2)]
        public bool renderHeadset = true;

        [Tooltip("Prefab for headset that is rendered if the crossfader is in the left 20% of the slider.")]
        public GameObject prefabHeadsetLeft = null;

        [Tooltip("Prefab for headset that is rendered if the crossfader is in the 20% of the slider between left and center.")]
        public GameObject prefabHeadsetLeftCenter = null;

        [Tooltip("Prefab for headset that is rendered if the crossfader is in the center 20% of the slider.")]
        public GameObject prefabHeadsetCenter = null;

        [Tooltip("Prefab for headset that is rendered if the crossfader is in the 20% of the slider between center and right.")]
        public GameObject prefabHeadsetRightCenter = null;

        [Tooltip("Prefab for headset that is rendered if the crossfader is in the right 20% of the slider.")]
        public GameObject prefabHeadsetRight = null;

        //// SECTION "Predefined Objects"
        [Header("Predefined Objects", order = 0)]
        [Helpbox("Prototyping cross-reality systems primarly works via predefined objects that are available inside the VRception toolkit (in the VR WYSIWYG). Below one can specify which objects are available in the in-game menu. To demonstrate the functionality, we included a display and projector object. Nevertheless, any Unity prefab can be made available as a predefined object in-game and thus, be used for prototyping. To enable players to interact with the predefined objects, it is recommended to attach the interactable script.", order = 1)]
        [Tooltip("Prefab available in the prefab slots of the interface.", order = 2)]
        public GameObject[] predefinedObjects;

        [Tooltip("Specify how many prefab objects are scrolled through with each input.")]
        [Range(1, 4)]
        public int numberOfScrolledObjects = 2;

        //// SECTION "Interactable Settings"
        [Header("Interactable Settings", order = 0)]
        [Helpbox("Interactables are another essential functionality of the VRception toolkit. They enable players to interact with the objects in their VR environment. Mainly, they are used for the predefined objects and enable players to translate, rotate, scale, duplicate, delete, and highlight them (depending on which interactions are configured for the interactable object). Below one can adjust some of these interactions.", order = 1)]
        [Tooltip("Factor with which the selection area of interactables is expanded beyond the collider.", order = 2)]
        [Range(0, 0.1f, order = 3)]
        public float selectionExpansion = 0.015f;

        [Tooltip("Speed with which prefabs are scaled within the game.")]
        [Range(0.1f, 1)]
        public float speedScale = 0.5f;

        [Tooltip("Specifies the minimum scale to which prefabs can be scaled.")]
        [Range(0.01f, 1f)]
        public float minimumScale = 0.01f;

        [Tooltip("Specifies the maximum scale to which prefabs can be scaled.")]
        [Range(1f, 10)]
        public float maximumScale = 10f;

        //// SECTION "Calibration Settings"
        [Header("Calibration Settings", order = 0)]
        [Helpbox("The CALIBRATION mode is one of the modes the VRception toolkit supports. When a player enters this mode, they can adjust the spaces of VRception (e.g., reality, virtuality) relatively to their real-world environment (position and orientation). Combined with a 3D scan of the players physical location, this allows to bring real-world environments and their haptics into the simulation.", order = 1)]
        [Tooltip("Translation speed for adjusting the position of all spaces during calibration.", order = 2)]
        [Range(0.1f, 10)]
        public float speedTranslate = 0.5f;

        [Tooltip("Rotation speed for adjusting the rotation of all spaces during calibration.")]
        [Range(0.1f, 10)]
        public float speedRotate = 0.5f;

        //// SECTION "Controller Settings"
        [Header("Controller Settings", order = 0)]
        [Helpbox("Adjust the sensitivity of the VR controllers to match personal preferences.", order = 1)]
        [Tooltip("Threshold for trigger input.", order = 2)]
        public float triggerTreshold = 0.95f;

        [Tooltip("Threshold for thumbstick input.")]
        public float thumbstickTreshold = 0.90f;

        //// SECTION "Splash Screen Settings"
        [Header("Splash Screen Settings", order = 0)]
        [Helpbox("A splash screen can be configured below that is displayed during startup of the VRception toolkit.", order = 1)]
        [Tooltip("If enabled, the splash screen is shown during startup.", order = 2)]
        public bool showSplashScreen;

        [Tooltip("Specify the prefab gameobject that is shown as a splash screen during startup.")]
        public GameObject splashScreenPrefab;

        //// SECTION "Other Settings"
        [Header("Other Settings", order = 0)]
        [Helpbox("Configure the duration of toast meassages below.", order = 1)]
        [Tooltip("Duration of toast messages.", order = 2)]
        [Range(1,10)]
        public float toastDuration = 3;

        //// SECTION "Layer Settings"
        [Header("Layer Settings", order = 0)]
        [Helpbox("The VRception toolkit uses not only different scenes to represent different spaces (e.g., reality, virtuality) but also different layers. This allows, for example, cameras to selectively render a certain space. Moreover, it allows to specify an order in which objects are rendered (e.g., user interfaces always on top).", order = 1)]
        [Tooltip("Select layer of interface.", order = 2)]
        public LayerIndex layerInterface;

        [Tooltip("Select layer of shared space.")]
        public LayerIndex layerShared;

        [Tooltip("Select layer of left space.")]
        public LayerIndex layerLeft;

        [Tooltip("Select layer of right space.")]
        public LayerIndex layerRight;

        [Tooltip("Select layer of default space.")]
        public LayerIndex layerDefault;


        // Delegates
        public delegate void OnModeTransition();
        public delegate void OnPlayingChange();
        public delegate void OnCrossfaderChange();

        // Events related to a transition in current activated mode
        public event OnModeTransition onModeTransitionToSimulation;
        public event OnModeTransition onModeTransitionFromSimulation;
        public event OnModeTransition onModeTransitionToCalibration;
        public event OnModeTransition onModeTransitionFromCalibration;
        public event OnModeTransition onModeTransitionToConfiguration;
        public event OnModeTransition onModeTransitionFromConfiguration;
        public event OnModeTransition onModeTransitionToExperience;
        public event OnModeTransition onModeTransitionFromExperience;

        // Events related to simulation playing change
        public event OnPlayingChange onPlayingChange;

        // Events related to crossfader change
        public event OnCrossfaderChange onCrossfaderChange;

        // Internal state of settings
        private Mode lastMode;
        private bool lastPlaying;
        private float lastCrossfader;

        // Store references
        private References references;

        // Store toast state
        private string toastMessage;
        private float toastTime;

        // Store startup camera
        private Camera startupCamera;

        // Store splash screen
        private GameObject splashScreen;
        
        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Welcome message
            Debug.Log("[VRception] The toolkit is starting to initialize itself.");

            // Singleton reference
            Settings.instance = this;

            // The following values are assumed by the code; if not inline with inspector selection, events are fired to update the components
            this.lastMode = Mode.SIMULATION;
            this.lastPlaying = true; 
            this.lastCrossfader = 0;

            // References are not yet ready
            this.references = null;

            // Initalize toast
            this.SetToast("Welcome to VRception");
        }

        // Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        void Start()
        {
            // Activate splash screen
            if(this.showSplashScreen)
            {
                this.splashScreen = Instantiate(this.splashScreenPrefab, new Vector3(0, -1.5f, 10), Quaternion.identity);
                this.splashScreen.transform.parent = this.transform;

                // Add camera to bridge the time before the player joins
                this.startupCamera = this.gameObject.AddComponent<Camera>();
                this.startupCamera.clearFlags = CameraClearFlags.SolidColor;
                this.startupCamera.backgroundColor = Color.black;
                this.startupCamera.cullingMask = (1 << Settings.instance.layerInterface.layerIndex);
                this.gameObject.tag = "MainCamera";
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Abort, if not initialized 
            if(!this.IsInitialized())
                return;

            // Toggle pausing/playing the simulation
            if (Mapping.ButtonSimulationPlayPause())
            {
                // Play/pause simulation
                this.playing = !this.playing;
            }

            // Calculate toast status
            if(this.toastTime > 0)
                this.toastTime -= Time.deltaTime;
            else
                this.toastMessage = "";

            //
            // Input for switching between modes
            //

            // Transition to/from calibration mode
            if(Mapping.ButtonModeCalibration())
            {
                if(this.mode == Mode.SIMULATION)
                    this.mode = Mode.CALIBRATION;
                else
                    this.mode = Mode.SIMULATION;
            }

            // Transition to/from configuration mode
            if (Mapping.ButtonModeConfiguration())
            {
                if(this.mode == Mode.SIMULATION)
                    this.mode = Mode.CONFIGURATION;
                else
                    this.mode = Mode.SIMULATION;
            }

            // Transition to/from experience mode
            if(Mapping.ButtonModeExperience())
            {
                if(this.mode == Mode.SIMULATION)
                    this.mode = Mode.EXPERIENCE;
                else
                    this.mode = Mode.SIMULATION;
            }
        }

        // LateUpdate is called after all Update functions have been called
        void LateUpdate()
        {
            // Abort, if not initialized 
            if(!this.IsInitialized())
                return;

            // // Check if internal state changed

            // Check state for mode
            if(this.mode != this.lastMode)
            {
                Debug.Log("[VRception] Currently active mode is now " + this.mode + ".", this);

                // Call events for mode transition TO state
                switch(this.mode)
                {
                    case Mode.SIMULATION:
                        if(this.onModeTransitionToSimulation != null)
                            this.onModeTransitionToSimulation();
                        break;
                    case Mode.CALIBRATION:
                        if(this.onModeTransitionToCalibration != null)
                            this.onModeTransitionToCalibration();
                        break;
                    case Mode.CONFIGURATION:
                        if(this.onModeTransitionToConfiguration != null)
                            this.onModeTransitionToConfiguration();
                        break;
                    case Mode.EXPERIENCE:
                        if(this.onModeTransitionToExperience != null)
                            this.onModeTransitionToExperience();
                        break;
                    default:
                        break;
                }

                // Call events for mode transition FROM state
                switch(this.lastMode)
                {
                    case Mode.SIMULATION:
                        if(this.onModeTransitionFromSimulation != null)
                            this.onModeTransitionFromSimulation();
                        break;
                    case Mode.CALIBRATION:
                        if(this.onModeTransitionFromCalibration != null)
                            this.onModeTransitionFromCalibration();
                        break;
                    case Mode.CONFIGURATION:
                        if(this.onModeTransitionFromConfiguration != null)
                            this.onModeTransitionFromConfiguration();
                        break;
                    case Mode.EXPERIENCE:
                        if(this.onModeTransitionFromExperience != null)
                            this.onModeTransitionFromExperience();
                        break;
                    default:
                        break;
                }

                // Store the new mode
                this.lastMode = this.mode;
            }

            // Check state for playing
            if(this.playing != this.lastPlaying)
            {
                // Call event
                if(this.onPlayingChange != null)
                    this.onPlayingChange();

                // Store last value
                this.lastPlaying = this.playing;
            }

            // Check state for crossfader state
            if(this.crossfader != this.lastCrossfader)
            {
                // Call event
                if(this.onCrossfaderChange != null)
                    this.onCrossfaderChange();

                // Store last value
                this.lastCrossfader = this.crossfader;
            }
        }

        // Initalize settings with references
        public void Initalize(References references)
        {
            if(references == null || this.references != null)
                return;

            this.references = references;

            // Apply changes in the crossfader value
            this.onCrossfaderChange += ApplyCrossfader;

            // Intially apply the crossfader value
            this.ApplyCrossfader();
        }

        // Returns if simulation is initialized
        public bool IsInitialized()
        {
            return this.references != null;
        }

        // Get intialized references
        public References GetReferences()
        {
            return this.references;
        }

        // Get current camera transform in the scene
        public Transform GetCameraTransform()
        {
            if(this.references == null)
                return this.transform;

            return this.references.cameraLeftLayer.transform;
        }

        // Get current space based on crossfader value
        public Space GetCurrentSpace()
        {
            if(this.crossfader > 0)
            {
                return Space.RIGHT;
            }
            return Space.LEFT;
        }

        // Allows to set the current toast message
        public void SetToast(string toast)
        {
            this.toastTime = this.toastDuration;
            this.toastMessage = toast;
        }

        // Returns the currently specified toast message
        public string GetToast()
        {
            return this.toastMessage;
        }

        // Method should be called when loading of the VRception toolkit is finished
        public void LoadingFinished()
        {
            // Loading already finished?
            if(!this.splashScreen.activeSelf)
                return;

            // Disable splash screen
            this.splashScreen.SetActive(false);

            // Disable startup camera
            this.gameObject.tag = "Untagged";
            this.startupCamera.enabled = false;
            Destroy(this.startupCamera);
        }

        // Apply current change in the crossfader
        private void ApplyCrossfader()
        {
            // Is Settings not initialized yet?
            if(!Settings.instance.IsInitialized())
                return;

            // Calculate left and right percentage values from crossfader
            float left = this.crossfader > 0 ? this.crossfader : 0;
            float right = this.crossfader >= 0 ? 0 : -this.crossfader;

            // Apply left and right percentage value to camera
            Utilities.SetAlpha(this.GetReferences().cameraLeftLayer, 1 - left);
            Utilities.SetAlpha(this.GetReferences().cameraRightLayer, 1 - right);

            // Get references to cameras
            Camera camLeft = this.GetReferences().cameraLeftLayer.GetComponent<Camera>();
            Camera camRight = this.GetReferences().cameraRightLayer.GetComponent<Camera>();

            // Is the slider of the crossfader closer to left
            if(this.crossfader <= 0)
            {
                // Set skybox of left
                RenderSettings.skybox = ControllerSpaces.instance.skyboxLeft;

                // Set up left camera
                camLeft.tag = "MainCamera";
                camLeft.depth = 0;
                camLeft.clearFlags = CameraClearFlags.Skybox;

                if(this.crossfader <= -1)
                    camLeft.cullingMask = 
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerLeft.layerIndex) | 
                        (1 << Settings.instance.layerInterface.layerIndex);
                else
                    camLeft.cullingMask = 
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerLeft.layerIndex);

                // Set up right camera
                camRight.tag = "Untagged";
                camRight.depth = 1;
                camRight.clearFlags = CameraClearFlags.Nothing;

                if(this.crossfader <= -1)
                    camRight.cullingMask = 
                        (1 << Settings.instance.layerRight.layerIndex);
                else
                    camRight.cullingMask = 
                        (1 << Settings.instance.layerRight.layerIndex) | 
                        (1 << Settings.instance.layerInterface.layerIndex);
            }
            // Is the slider of the crossfader closer to right
            else
            {
                // Set skybox of right
                RenderSettings.skybox = ControllerSpaces.instance.skyboxRight;

                // Set up left camera
                camLeft.tag = "Untagged";
                camLeft.depth = 1;
                camLeft.clearFlags = CameraClearFlags.Nothing;

                if(this.crossfader >= 1)
                    camLeft.cullingMask = 
                        (1 << Settings.instance.layerLeft.layerIndex);
                else
                    camLeft.cullingMask = 
                        (1 << Settings.instance.layerLeft.layerIndex) | 
                        (1 << Settings.instance.layerInterface.layerIndex);

                // Set up right camera
                camRight.tag = "MainCamera";
                camRight.depth = 0;
                camRight.clearFlags = CameraClearFlags.Skybox;

                if(this.crossfader >= 1)
                    camRight.cullingMask = 
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerRight.layerIndex) | 
                        (1 << Settings.instance.layerInterface.layerIndex);
                else
                    camRight.cullingMask = 
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerRight.layerIndex);
            }
        }
    }
}