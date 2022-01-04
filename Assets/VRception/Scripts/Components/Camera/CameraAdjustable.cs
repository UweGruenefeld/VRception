using UnityEngine;
using UnityEngine.Rendering;
using Photon.Pun;

namespace VRception
{
    /// <summary>
    /// This script automatically generates and manages two interactables that enable players to adjust the camera position and lookat in-game during 'Configuration Mode'
    /// In essence, it is written for predefined objects that make use of a camera component and want players to have more control over them (e.g., 'Simple Display' or 'Simple Projector')
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(PhotonView))]
    public class CameraAdjustable : MonoBehaviourPunCallbacks, IEventCopy
    {
        //// SECTION "Adjustable Settings"
        [Header("Adjustable Settings", order = 0)]
        [Helpbox("This script automatically generates and manages two interactables that enable players to adjust the camera position and lookat in-game during 'Configuration Mode.' Below, one can specify the initial position of the two interactables relative to this gameobject with the camera component attached.", order = 1)]
        [Tooltip("Specify the initial position of the interactable controlling the position of the camera.", order = 2)]
        public Vector3 initialCameraPosition = new Vector3(0, 0, 0.15f);

        [Tooltip("Specify the initial position of the interactable controlling the lookAt of the camera.")]
        public Vector3 initialCameraLookAt = new Vector3(0, 0, 0.25f);

        [Tooltip("Specify the interactable this camera components belongs to.")]
        public Interactable interactable = null;

        // Store the interactables
        private GameObject cameraPosition = null;
        private GameObject cameraLookAt = null;

        // Store the last position of the interactables
        private Vector3 lastCameraPosition = Vector3.zero;
        private Vector3 lastCameraLookAt = Vector3.zero;

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
            // Create interactables to adjust camera
            this.CreateAdjustInteractables();

            Settings.instance.onModeTransitionToConfiguration += ActivateAdjusting;
            Settings.instance.onModeTransitionFromConfiguration += DeactivateAdjusting;
        }

        // Update is called once per frame
        void Update()
        {
            // Check if interactables to control camera still exist
            if(this.cameraPosition == null)
            {
                this.cameraPosition = this.CreateAdjustCameraInteractable();
                this.cameraPosition.transform.position = this.lastCameraPosition;

                if(Settings.instance.mode == Mode.CONFIGURATION)
                {
                    if(this.interactable == null || !this.interactable.IsPrefabInInterface())
                        this.cameraPosition.SetActive(true);
                }
            }
            if(this.cameraLookAt == null)
            {
                this.cameraLookAt = this.CreateAdjustLookAtInteractable();
                this.cameraLookAt.transform.position = this.lastCameraLookAt;

                if(Settings.instance.mode == Mode.CONFIGURATION)
                {
                    if(this.interactable == null || !this.interactable.IsPrefabInInterface())
                        this.cameraLookAt.SetActive(true);
                }
            }

            // Check if currently in configuration mode
            if(Settings.instance.mode == Mode.CONFIGURATION)
            {
                // Update where the interactables look to
                this.cameraPosition.GetComponent<Interactable>().modelGameObject.transform.LookAt(this.cameraLookAt.transform.position, Vector3.up);
                this.cameraLookAt.GetComponent<Interactable>().modelGameObject.transform.LookAt(this.cameraPosition.transform.position, Vector3.up);
            }

            // Update parent modules
            ModuleParent modulePosition = this.cameraPosition.GetComponent<ModuleParent>();
            if(modulePosition != null)
                modulePosition.Update();
            ModuleParent moduleLookAt = this.cameraLookAt.GetComponent<ModuleParent>();
            if(moduleLookAt != null)
                moduleLookAt.Update();
        }

        // LateUpdate is called after all Update functions have been called
        void LateUpdate()
        {
            // If I am the owner, then update camera with position and lookat
            if (this.photonView.IsMine)
            {
                this.transform.position = this.cameraPosition.transform.position;
                this.transform.LookAt(this.cameraLookAt.transform.position, Vector3.up);

                // Store last positions
                this.lastCameraPosition = this.cameraPosition.transform.position;
                this.lastCameraLookAt = this.cameraLookAt.transform.position;
            }
            // If someone else is the owner, then listen to that data
            else
            {
                this.cameraPosition.transform.position = this.transform.position;
                this.cameraLookAt.transform.position = this.transform.position + (this.transform.rotation * (Vector3.forward * 0.25f));

                // Store last positions
                this.lastCameraPosition = this.cameraPosition.transform.position;
                this.lastCameraLookAt = this.cameraLookAt.transform.position;
            }
        }

        // OnDestroy will only be called on game objects that have previously been active; it is called when the gameobject is destroyed
        void OnDestroy()
        {
            Settings.instance.onModeTransitionToConfiguration -= ActivateAdjusting;
            Settings.instance.onModeTransitionFromConfiguration -= DeactivateAdjusting;

            DestroyImmediate(this.cameraPosition);
            DestroyImmediate(this.cameraLookAt);
        }

        // Method is called before interactable is about to be copied
        public void OnBeforeCopy()
        {
            // Destroy label
            this.DestroyAdjustInteractables();
        }

        // Method is called after interactable has been copied
        public void OnAfterCopy()
        {
            // Recreate label
            this.CreateAdjustInteractables();

            this.cameraPosition.transform.position = this.lastCameraPosition;
            this.cameraLookAt.transform.position = this.lastCameraLookAt;
        }

        // Is called when the mode transitions to configuration
        public void ActivateAdjusting()
        {
            if(this.interactable.IsPrefabInInterface())
                return;

            this.cameraPosition.SetActive(true);
            this.cameraLookAt.SetActive(true);
        }

        // Is called when the mode transitions away from configuration
        public void DeactivateAdjusting()
        {
            this.cameraPosition.SetActive(false);
            this.cameraLookAt.SetActive(false);
        }

        // Method creates both interactable gameobjects
        private void CreateAdjustInteractables()
        {
            this.cameraPosition = this.CreateAdjustCameraInteractable();
            this.cameraLookAt = this.CreateAdjustLookAtInteractable();
        }

        // Method destroys both interactable gameobjects
        private void DestroyAdjustInteractables()
        {
            if(this.cameraPosition != null)
                DestroyImmediate(this.cameraPosition);
            if(this.cameraLookAt != null)
                DestroyImmediate(this.cameraLookAt);
        }

        // Method creates interactable for changing the camera position
        private GameObject CreateAdjustCameraInteractable()
        {
            return this.CreateAdjustInteractable("CameraPosition", "Camera", this.initialCameraPosition);
        }

        // Method creates interactable for changing the camera lookat
        private GameObject CreateAdjustLookAtInteractable()
        {
            return this.CreateAdjustInteractable("CameraLookAt", "Look At", this.initialCameraLookAt);
        }

        // Method that creates a sub interactable
        private GameObject CreateAdjustInteractable(string name, string description, Vector3 localPosition)
        {
            //// CREATE GAMEOBJECTS FOR INTERACTABLE AND MODEL

            // Create the gameobject for the interactable
            GameObject interactableObject = new GameObject(name);
            interactableObject.transform.parent = this.interactable.transform;
            interactableObject.transform.localPosition = localPosition;
            interactableObject.transform.localRotation = Quaternion.identity;
            interactableObject.transform.localScale = Vector3.one;

            // Create the gameobject that contains the model
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "Model";
            model.transform.parent = interactableObject.transform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * 0.05f;

            // Add correct meshrenderer to model gameobject
            MeshRenderer meshRenderer = model.GetComponent<MeshRenderer>();
            meshRenderer.material = Resources.Load("Materials/DefaultWithoutProjection") as Material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            // Add rigidbody to model gameobject
            Rigidbody rigidbody = model.AddComponent<Rigidbody>();
            rigidbody.angularDrag = 0;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;

            // Add boxcollider for model gameobject
            BoxCollider boxCollider = model.GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;

            // Add the necessary collision detector
            CollisionDetector collisionDetector = model.AddComponent<CollisionDetector>();


            //// ADD INTERACTABLE TO GAMEOBJECT

            // Add the interactable
            Interactable interactable = interactableObject.AddComponent<Interactable>();

            // Configure the settings of the interactabke
            interactable.activeMode = Mode.CONFIGURATION;
            interactable.changeParent = false;  // results in the interactable (not this sub interactable) to become the child of another object/interactable
            interactable.isSubInteractable = true;
            interactable.autoGenerate = true;
            interactable.isLogging = false;

            // Specify the necessary references
            interactable.modelGameObject = model;
            interactable.modelCollisionDetector = collisionDetector;
            interactable.modelRenderer = meshRenderer;

            // Set interactable to correct space
            interactable.SetSpace(this.interactable.GetSpace());


            //// ADD MODULES TO INTERACTABLE

            // Add module highlight to interactable
            interactableObject.AddComponent<ModuleHighlight>();

            // Add module label to interactable
            ModuleLabel moduleLabel = interactableObject.AddComponent<ModuleLabel>();
            moduleLabel.label = description;
            moduleLabel.visibleAlways = true;
            moduleLabel.visibleOnHover = false;

            // Add module line to interactable
            ModuleLine moduleLine = interactableObject.AddComponent<ModuleLine>();
            moduleLine.target = this.interactable.modelGameObject;

            // Add module translate to interactable
            interactableObject.AddComponent<ModuleTranslate>();

            // Add module toast to interactable
            interactableObject.AddComponent<ModuleToast>();


            //// COMPLETE SETTING UP THE ADJUSTABLE INTERACTABLE

            // Set correct layer
            Utilities.SetLayerRecursively(interactableObject, this.interactable.GetSpace());

            // Activate the interactable
            interactableObject.SetActive(false);

            // Return the interactable
            return interactableObject;
        }
    }
}