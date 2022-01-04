using UnityEngine;
using UnityEditor;
using Photon.Pun;
using MongoDB.Bson;

namespace VRception
{
    /// <summary>
    /// Interactables can be attached to gameobjects/prefabs to make them interactable.
    /// To customize the functionality of an interactable, several different modules are available.
    /// </summary>
    public class Interactable : MonoBehaviour, IEventCopy
    {
        //// SECTION "Interactable Settings"
        [Header("Interactable Settings", order = 0)]
        [Helpbox("Interactables are another important feature of the VRception toolkit. They can be attached to gameobjects/prefabs to make them interactable. To customize the functionality of an interactable, several different modules are available. Each module introduces its own functionality such as highlighting, deleting, duplicating, translating, rotating, and scaling an interactable; among other functionalities. Each interactable is active in only one specified mode (see below). Moreover, interactables can be attached to each other. For example, in-game to construct more complex objects or in the Unity editor as subinteractables that function as additional input for an interactable (e.g., the 'simpleDisplay' and 'SimpleProjector' use subinteractables active in the configuration mode to allow players to configure their camera component).", order = 1)]
        [Tooltip("Define the mode in which the interactable is active.", order = 2)]
        public Mode activeMode = Mode.SIMULATION;

        [Tooltip("If enabled, the interactable becomes a child of a colliding object on release.")]
        public bool changeParent = false;

        [Tooltip("If true, this interactable is not considered as parent game object.")]
        public bool ignoreAsParent = false;

        [Tooltip("If true, this interactable is and should always be attached to another interactable.")]
        public bool isSubInteractable = false;

        [Tooltip("If true, this object is synchronized over network.")]
        public bool isSynchronized = true;

        [Tooltip("If true, this object will be logged for replay.")]
        public bool isLogging = true;

        [Tooltip("If yes, then all necessary Photon network components are added automatically.")]
        public bool autoGenerate = true;

        //// SECTION "References"
        [Header("References", order = 0)]
        [Helpbox("For interactables to work properly, it is important that the actually 3D model representing the interactable is provided in any child gameobject of the interactable gameobject (specify it below). This was a code design decision that made developing the features such as scaling or attaching objects to each other easier for us. Below, one can specify the gameobject that contains the model (this gameobject can, of course, have children and deep children), any renderer of the model availble that will be used to highlight the interactable, and a collision detector that is used to detect collision with the player controller or other interactables.", order = 1)]
        [Tooltip("Reference to a child that contains the 3D model.", order = 2)]
        public GameObject modelGameObject = null; // IMPORTANT! must be a child of the interactable game object

        [Tooltip("Renderer component used to show if the object is hovered or selected.")]
        public Renderer modelRenderer = null;

        [Tooltip("Wrapper of collider that is used to detect collision with other game objects.")]
        public CollisionDetector modelCollisionDetector = null;

        // Photon view attached to interactable
        private PhotonView photonView;

        // Space in which the interactable exists
        private Space space;

        // Reference to collider component
        private new Collider collider;

        // Current states of the interactable
        private bool isHovered;
        private bool isSelected;

        // States of the interactable in last frame
        private bool lastHovered;
        private bool lastSelected;
        private bool lastButton;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Store photon view
            this.photonView = PhotonView.Get(this);

            this.space = Utilities.GetSpaceByLayer(this.gameObject.layer);

            this.isHovered = false;
            this.isSelected = false;

            this.lastHovered = false;
            this.lastSelected = false;
            this.lastButton = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Get reference for collider
            this.collider = this.modelCollisionDetector.GetComponent<Collider>();

            // Register deactivate function to ensure interactable is only active in specified mode
            switch (this.activeMode)
            {
                case Mode.SIMULATION:
                    Settings.instance.onModeTransitionFromSimulation += Reset;
                    break;
                case Mode.CALIBRATION:
                    Settings.instance.onModeTransitionFromCalibration += Reset;
                    break;
                case Mode.CONFIGURATION:
                    Settings.instance.onModeTransitionFromConfiguration += Reset;
                    break;
                case Mode.EXPERIENCE:
                    Settings.instance.onModeTransitionFromConfiguration += Reset;
                    break;
                default:
                    break;
            }

            // Register object for logging
            if (this.isLogging && ControllerLogger.instance != null)
                ControllerLogger.instance.RegisterObjectForLogging(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }

        // LateUpdate is called after all Update functions have been called
        void LateUpdate()
        {
            // Is settings not initialized yet?
            if (!Settings.instance.IsInitialized())
                return;

            // Interactable only works in the specified mode
            if (Settings.instance.mode != this.activeMode)
            {
                this.isHovered = false;
                this.isSelected = false;

                if (this.isHovered != this.lastHovered)
                {
                    this.SendEventOnHover(this.gameObject, false);
                    this.lastHovered = this.isHovered;
                }

                if (this.isSelected != this.lastSelected)
                {
                    this.SendEventOnSelect(this.gameObject, false);
                    this.lastSelected = this.isSelected;
                }

                return;
            }

            // If user is in complete in left space and object is from right space
            if (Settings.instance.crossfader <= -1 && this.space == Space.RIGHT)
            {
                this.isHovered = false;
                this.isSelected = false;
                
                return;
            }

            // If user is in complete in right space and object is from left space
            if (Settings.instance.crossfader >= 1 && this.space == Space.LEFT)
            {
                this.isHovered = false;
                this.isSelected = false;
                return;
            }

            // Get controller position
            Vector3 controllerPosition = Mapping.Axis3DInteractableControllerPosition();

            // Move the position slightly towards the collider to improve selection
            Vector3 difference = Vector3.zero;
            if (this.collider.bounds.extents.magnitude * this.collider.transform.localScale.magnitude < 0.25f)
                difference = Vector3.Normalize(this.collider.transform.position - controllerPosition) * Settings.instance.selectionExpansion;

            // Is controller within the bounds of the collider?
            if (this.collider.bounds.Contains(controllerPosition + difference))
            {
                // Is object visible?
                if (!((Settings.instance.crossfader >= 1 && this.GetSpace() == Space.LEFT) ||
                    (Settings.instance.crossfader <= -1 && this.GetSpace() == Space.RIGHT)))
                {
                    // Is interactable not marked as hovered already? Then mark it as hovered, if the select button is not pressed.
                    if (!this.isHovered && !Mapping.ButtonInteractableSelect())
                    {
                        Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is hovered", this);

                        this.isHovered = true;
                    }

                    // Is interactable not selected but user is trying to select the object?
                    if (!this.isSelected && Mapping.ButtonInteractableSelect() && !this.lastButton)
                    {
                        Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is selected", this);

                        this.isSelected = true;

                        // Request ownership for the photon view if it is not mine
                        if (!this.photonView.IsMine && this.photonView.ViewID > 0)
                            this.photonView.RequestOwnership();
                    }
                }
            }
            // Interactable is not within the of the collider
            else
            {
                // Is the object marked as hovered but not within range and not selected?
                if (this.isHovered && !this.isSelected)
                {
                    Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is not hovered anymore");
                    this.isHovered = false;
                }
            }

            // Is user not holding the interactable anymore?
            if (this.isSelected && !Mapping.ButtonInteractableSelect())
            {
                Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is not selected anymore", this);

                // Check if it is not an UI element
                if (this.gameObject.layer != Settings.instance.layerInterface.layerIndex)
                {
                    // Set correct layer of gameobject on release
                    Utilities.SetLayerRecursively(this.gameObject, this.space);

                    // Is change of parent enabled?
                    if (this.changeParent)
                    {
                        Debug.Log("[VRception] Change parent is enabled for " + this.gameObject.name, this);

                        // Get current colliding objects
                        GameObject[] collidingObjects = this.modelCollisionDetector.GetCurrentCollisions();

                        // Store the selected colliding object
                        GameObject validCollidingObject = null;

                        // Iterate through colliding objects
                        foreach (GameObject collidingObject in collidingObjects)
                        {
                            // Check if colliding object still exists
                            if (collidingObject == null)
                                continue;

                            Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is colliding with " + collidingObject.name, this);

                            // Can we see the colliding object
                            Space colliderSpace = ControllerSpaces.instance.GetSpaceOfGameObject(collidingObject);
                            if (colliderSpace != this.space && colliderSpace != Space.SHARED &&
                                (Settings.instance.crossfader >= 1 || Settings.instance.crossfader <= -1))
                            {
                                Debug.Log("[VRception] The colliding gameobject " + collidingObject.name + " is not visible at the moment; Ignore it", this);
                                continue;
                            }

                            // Find first interactable in colliding game object
                            Interactable interactableInCollidingObject = Utilities.FindFirstInteractableInGameObjectAndParents(collidingObject);

                            // Is there a interactable attached to the object?
                            if (interactableInCollidingObject != null)
                            {
                                Debug.Log("[VRception] The colliding gameobject " + collidingObject.name + " is part of the interactable " + interactableInCollidingObject.gameObject.name, this);

                                // Is the interactable a child of this interactable?
                                if(collidingObject.transform.IsChildOf(this.transform))
                                {
                                    Debug.Log("[VRception] The interactable " + interactableInCollidingObject.gameObject.name + " is a child interactable; Ignore it", this);
                                    continue;
                                }

                                // Is this interactable only a sub interactable?
                                if (interactableInCollidingObject.isSubInteractable)
                                {
                                    Debug.Log("[VRception] The interactable " + interactableInCollidingObject.gameObject.name + " is configured a sub interactable; Ignore it", this);
                                    continue;
                                }

                                // Should the interactable be ignored?
                                if (interactableInCollidingObject.ignoreAsParent)
                                {
                                    Debug.Log("[VRception] The interactable " + interactableInCollidingObject.gameObject.name + " is configured to never function as parent; Ignore it", this);
                                    validCollidingObject = collidingObject;
                                    continue;
                                }

                                // Interactable is suitable to become the new parent
                                Debug.Log("[VRception] The colliding gameobject " + collidingObject.name + " is part of the interactable " + interactableInCollidingObject.gameObject.name + " that can function as parent", this);
                                validCollidingObject = interactableInCollidingObject.gameObject;
                                break;
                            }
                            // No interactable attached and we can use the collision object as new parent?
                            else
                            {
                                Debug.Log("[VRception] The colliding gameobject " + collidingObject.name + " has no interactable attached and can be used as new parent", this);
                                validCollidingObject = collidingObject;
                                break;
                            }
                        }

                        // Logging
                        if(validCollidingObject != null)
                            Debug.Log("[VRception] We try to attach the gameobject " + this.gameObject.name + " to the parent " + validCollidingObject.name, this);
                        else
                            Debug.Log("[VRception] For the gameobject " + this.gameObject.name + " we could not identify a suitable parent", this);

                        // Check if this view has no view ID yet
                        if (photonView.ViewID <= 0)
                        {
                            Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is not registered in Photon yet", this);
                            
                            ControllerSpaces.instance.MoveGameObjectToSpace(this.gameObject, space);
                            this.SetParent(validCollidingObject);
                        }
                        else
                        {
                            // Attach to correct space and parent
                            PhotonView photonViewCollidingObject = null;

                            if (validCollidingObject != null)
                                photonViewCollidingObject = PhotonView.Get(validCollidingObject);

                            if (photonViewCollidingObject != null)
                            {
                                Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is registered in Photon and we found the ViewID " + photonViewCollidingObject.ViewID + " of the colliding object " + validCollidingObject.name, this);
                                this.photonView.RPC("SetSpaceAndParentRPC", RpcTarget.All, this.space, photonViewCollidingObject.ViewID);
                            }
                            else
                            {
                                // parent viewID not found
                                if(validCollidingObject != null)
                                    Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is registered in Photon but the ViewID of the colliding object " + validCollidingObject.name + " could not be found", this);
                                // no parent specified
                                else
                                    Debug.Log("[VRception] The gameobject " + this.gameObject.name + " is registered in Photon and no parent is specified", this);
                                
                                this.photonView.RPC("SetSpaceAndParentRPC", RpcTarget.All, this.space, 0);
                            }
                        }
                    }
                }

                // User is not holding the object anymore
                this.isSelected = false;
            }

            // Store if button was pressed in last frame
            this.lastButton = Mapping.ButtonInteractableSelect();

            // Inform about hover events
            if (this.isHovered != this.lastHovered)
            {
                this.SendEventOnHover(this.gameObject, this.isHovered);
                this.lastHovered = this.isHovered;
            }

            // Inform about select events
            if (this.isSelected != this.lastSelected)
            {
                this.SendEventOnSelect(this.gameObject, this.isSelected);
                this.lastSelected = this.isSelected;
            }
        }

        // Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
        void OnDestroy()
        {
            Debug.Log("[VRception] The gameobject " + this.gameObject.name + " will be destroyed");

            Settings.instance.onModeTransitionFromSimulation -= Reset;
            Settings.instance.onModeTransitionFromCalibration -= Reset;
            Settings.instance.onModeTransitionFromConfiguration -= Reset;
            Settings.instance.onModeTransitionFromExperience -= Reset;

            PhotonView photonView = PhotonView.Get(gameObject);
            int photonViewID = -1;
            if (photonView != null)
            {
                photonViewID = photonView.ViewID;
            }

            if (ControllerLogger.instance == null || !this.isLogging)
                return;

            string nameClonable = "not_given";

            Cloneable cloneable = GetComponent<Cloneable>();
            if (cloneable != null)
            {
                nameClonable = cloneable.prefabName;
            }

            // Log delete event
            BsonDocument eventDocument = new BsonDocument()
                    {
                        { "event_name", "deleted" },
                        { "photonViewID", photonViewID},
                        { "clonable_prefabName", nameClonable },
                        { "gameObjectName", gameObject.name }
                    };
            ControllerLogger.instance.LogEvent(eventDocument);

            // Unegister object for logging
            ControllerLogger.instance.UnregisterObjectForLogging(gameObject);

            Debug.Log("[VRception] The gameobject " + this.gameObject.name + " has been destroyed");
        }

        // Method gets invoked before the interactable is copied
        public void OnBeforeCopy()
        {

        }

        // Method gets invoked after the interactable is copied
        public void OnAfterCopy()
        {

        }

        // Resets the internal state of an interactable, while fire events to modules listen to them
        public void Reset()
        {
            this.isHovered = false;
            this.isSelected = false;
        }

        // Sets the interactable to the currently selected space
        public void SetSpace()
        {
            this.SetSpace(Settings.instance.GetCurrentSpace());
        }

        // Sets the interactable to a custom via parameter specified space
        public void SetSpace(Space space)
        {
            if (this.space == space)
                return;

            this.space = space;
        }

        // Returns the current space of this interactable
        public Space GetSpace()
        {
            return this.space;
        }

        // Returns if the interactable is currently hovered
        public bool IsHovered()
        {
            return this.isHovered;
        }

        // Returns if the interactable has a parent interactable that is currently hovered
        public bool IsHoveredAsChild()
        {
            if(this.isHovered)
                return false;

            Interactable[] interactables = this.GetComponentsInParent<Interactable>();

            foreach (Interactable interactable in interactables)
                if(interactable.IsHovered())
                    return true;

            return false;
        }

        // Sets if the interactable is currently selected
        public void SetSelect(bool select)
        {
            this.lastSelected = !select;
            this.isSelected = select;
        }

        // Returns if the interactable is currently selected
        public bool IsSelected()
        {
            if (this.photonView.IsMine)
                return this.isSelected;
            return false;
        }

        // Returns if the interactable has a parent interactable that is currently selected
        public bool IsSelectedAsChild()
        {
            if(this.isSelected)
                return false;

            Interactable[] interactables = this.GetComponentsInParent<Interactable>();

            foreach (Interactable interactable in interactables)
                if(interactable.IsSelected())
                    return true;

            return false;
        }

        // Returns if the interactable is part of the list of predefined objects on the interface menu
        public bool IsPrefabInInterface()
        {
            if (this.GetComponent<ModuleInterface>() != null)
                return true;
            return false;
        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }

        // Remote procedure call for Photon to synchronize the space and parent of an interactable
        [PunRPC]
        public void SetSpaceAndParentRPC(Space space, int parentID)
        {
            Debug.Log("[VRception] RPC: for the gameobject " + this.gameObject.name + " we set it to space " + space + " and parent with ViewID " + parentID, this);
            
            // Attach to correct space
            this.space = space;
            ControllerSpaces.instance.MoveGameObjectToSpace(this.gameObject, space);

            // If the parent ID is not valid return
            if (parentID <= 0)
            {
                Debug.Log("[VRception] RPC: The parent viewID for the gameobject " + this.gameObject.name + " is not valid; no parent specified", this);
                this.GetComponent<PhotonTransformView>().enabled = true;
                return;
            }

            // Try to find the parent view
            PhotonView parentPhotonView = PhotonView.Find(parentID);

            // If we did not find a view return
            if (parentPhotonView == null)
            {
                Debug.Log("[VRception] RPC: The parent viewID for the gameobject " + this.gameObject.name + " could not be found; no parent specified", this);
                this.GetComponent<PhotonTransformView>().enabled = true;
                return;
            }

            // Set parent
            Debug.Log("[VRception] RPC: the gameobject " + this.gameObject.name + " has the parent " + parentPhotonView.gameObject.name + " now", this);
            this.SetParent(parentPhotonView.gameObject);
            // this.GetComponent<PhotonTransformView>().enabled = false;
        }

        // Method will send the event IEventHover to all modules (in parent, children, and deep children)
        private void SendEventOnHover(GameObject obj, bool onEnter)
        {
            // Avoid sending messages to disabled gameobjects
            if(!obj.activeSelf)
                return;

            // Derive if current gameobject is a child of the originally triggered gameobject
            bool isChild = obj != this.gameObject;

            // Get interactable of game object
            Interactable interactable = obj.GetComponent<Interactable>();

            // Get event modules of game object
            IEventHover[] modules = obj.GetComponents<IEventHover>();

            // If event modules are available send event
            if (modules != null && modules.Length > 0)
            {
                if (onEnter)
                {
                    // Log event
                    Debug.Log("[VRception] Event OnHoverEnter triggered for " + obj.name + " with child=" + isChild, this);

                    foreach (IEventHover module in modules)
                        module.OnHoverEnter(isChild);
                } 
                else
                {
                    // Log event
                    Debug.Log("[VRception] Event OnHoverExit triggered for " + obj.name + " with child=" + isChild, this);

                    foreach (IEventHover module in modules)
                        module.OnHoverExit(isChild);
                }
            }

            // Continue for children
            foreach (Transform transform in obj.transform)
                this.SendEventOnHover(transform.gameObject, onEnter);
        }

        // Method will send the event IEventSelect to all modules (in parent, children, and deep children)
        private void SendEventOnSelect(GameObject obj, bool onEnter)
        {
            // Avoid sending messages to disabled gameobjects
            if(!obj.activeSelf)
                return;

            // Derive if current gameobject is a child of the originally triggered gameobject
            bool isChild = obj != this.gameObject;

            // Get interactable of game object
            Interactable interactable = obj.GetComponent<Interactable>();

            // Get event modules of game object
            IEventSelect[] modules = obj.GetComponents<IEventSelect>();

            // If event modules are available send event
            if (modules != null && modules.Length > 0)
            {
                if (onEnter)
                {
                    // Log event
                    Debug.Log("[VRception] Event OnSelectEnter triggered for " + obj.name + " with child=" + isChild, this);

                    foreach (IEventSelect module in modules)
                        module.OnSelectEnter(isChild);
                }
                else
                {
                    // Log event
                    Debug.Log("[VRception] Event OnSelectExit triggered for " + obj.name + " with child=" + isChild, this);

                    foreach (IEventSelect module in modules)
                        module.OnSelectExit(isChild);
                }
            }

            // Continue for children
            foreach (Transform transform in obj.transform)
                this.SendEventOnSelect(transform.gameObject, onEnter);
        }

        // Sets the parent of the interactable
        private void SetParent(GameObject parent)
        {
            // Check if parent is specified
            if (parent == null)
            {
                this.transform.parent = null;
                return;
            }

            // Get parent space
            Space parentSpace = ControllerSpaces.instance.GetSpaceOfGameObject(parent);

            // Parent game object is in different space (in our case it means a different scene)
            if (this.space != parentSpace)
            {
                // Get existing parent modules in target parent and its parents
                ModuleParent[] existingModules = parent.GetComponentsInParent<ModuleParent>();

                // Is there a critical cross reference
                bool criticalReference = false;

                // Check if a critical cross reference exists
                foreach (ModuleParent existingModule in existingModules)
                    if (IsTransformInChildren(existingModule.parent, this.transform))
                        criticalReference = true;

                // Make this a root game object if there is a critical reference
                if (criticalReference)
                {
                    this.transform.parent = null;
                    return;
                }

                // Set reference
                ModuleParent moduleParent = this.gameObject.AddComponent<ModuleParent>();
                moduleParent.parent = parent.transform;
            }
            // Game object is in this scene
            else
                this.transform.parent = parent.transform;
        }

        // Returns true if referenced transform is the children transform or one of its children
        private bool IsTransformInChildren(Transform transform, Transform children)
        {
            if (transform == children)
                return true;

            foreach (Transform child in children)
                if (this.IsTransformInChildren(transform, child))
                    return true;

            return false;
        }
    }
}