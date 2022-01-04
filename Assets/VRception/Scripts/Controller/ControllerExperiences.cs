using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace VRception
{
    /// <summary>
    /// This class bundles all functionality related to experiences. 
    /// Experiences offer an alternative to the crossfader, enabling players to experience custom remixes of the different spaces (e.g., specific manifestations of the reality-virtuality continuum).
    /// </summary>
    public class ControllerExperiences : MonoBehaviourPunCallbacks
    {
        [HideInInspector]
        public static ControllerExperiences instance;

        //// SECTION "Experience Settings"
        [Header("Experience Settings", order = 0)]
        [Helpbox("Experiences offer an alternative to the crossfader, enabling players to experience custom remixes of the different spaces (e.g., specific manifestations of the reality-virtuality continuum). Different than the crossfader, one experience describes one concrete manifestation. Players can switch between different experiences but the switch is not continous as it is for the crossfader. For example, one can implement an experience in which only nearby objects from reality are overlayed on the perceived virtuality (similar to 'A Dose of Reality' from McGill et al.). Experiences need to be implemented with scripts in Unity and enabled below to make them available in-game.", order = 1)]
        [Tooltip("Select the experiences that are activated during game.", order = 2)]
        [Bitmask(typeof(Experience), order = 3)]
        public Experience activatedExperiences;

        [Tooltip("Specify the duration in seconds that it takes to fly into a different perspective.")]
        [Range(0, 5)]
        public float durationOfFlight = 0.5f;

        //// SECTION "Marker Prefabs"
        [Header("Marker Prefabs", order = 0)]
        [Helpbox("An additional functionality that experiences offer are markers. Markers can be placed anywhere (they exist across spaces) and can be assigned one specific experience. Thereby, players are able to quickly jump to a marker position and switch to its assigned experience. Markers have a default representation that can be assigned below (initially a camera-like object) or one can choose a custom prefab to represent a marker (the prefab needs to have the 'Marker Prefab' script attached and can be selected for each marker individually). Thus, one can, for example, represent a marker as a bystander (by using a character prefab) and assign them a fitting experience (e.g., Augmented Reality). Then, in-game player can use these character as simulated bystanders and prototype cross-reality systems with them (e.g., attach an external display to their HMD that shows their face; similar to 'TransparentHMD' by Mai et al.). To add markers to the simulation, one needs to add child gameobjects to this one that have the 'Marker' script attached.", order = 1)]
        [Tooltip("Prefab that contains the symbol gameobject to represent a marker.", order = 2)]
        public GameObject markerDefaultPrefab = null;

        // Internal list of experiences and marker prefabs
        private List<Experience> experiences;
        private List<MarkerPrefab> markerPrefabs;

        // Store the last experience
        private IExperience lastExperience;

        // Pointer for the currently active experience and markerPrefab
        private int indexExperience;
        private int indexMarker;

        // Store last input
        private bool lastXInput;
        private bool lastYInput;

        // Store last player transform and experience
        private GameObject lastPlayerTransform;
        private int indexLastExperience;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Assign singleton object
            ControllerExperiences.instance = this;

            // Clean initialization of index for experience and markerPrefab
            this.indexExperience = -1;
            this.indexMarker = -1;

            // Store last experience in markerless position
            this.indexLastExperience = -1;

            // Initalize list of experiences
            this.experiences = this.GetSelectedExperiences();

            // If there are experiences, ser index to first element
            if(this.experiences.Count > 0)
            {
                this.indexExperience = 0;
                this.indexLastExperience = 0;
            }

            // Initialize list of marker prefabs
            this.markerPrefabs = new List<MarkerPrefab>();

            // Initialize storage of last input
            this.lastXInput = false;
            this.lastYInput = false;

            // Initalize last player transform storage
            this.lastPlayerTransform = new GameObject("LastPlayerPosition");
            this.lastPlayerTransform.transform.parent = this.transform;
            this.lastPlayerTransform.transform.position = Vector3.zero;
            this.lastPlayerTransform.transform.rotation = Quaternion.identity;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Register events
            Settings.instance.onModeTransitionToExperience += OnEnterExperiences;
            Settings.instance.onModeTransitionFromExperience += OnLeaveExperiences;
        }

        // Update is called once per frame
        void Update()
        {
            // Is Settings not initialized yet?
            if(!Settings.instance.IsInitialized())
                return;

            // If not in experience mode, abort
            if(Settings.instance.mode != Mode.EXPERIENCE)
                return;

            // If no experiences are avialable
            if(this.experiences.Count <= 0)
            {
                Settings.instance.mode = Mode.SIMULATION;
                return;
            }

            // Check input for switch in experience
            Vector2 input = Mapping.Axis2DExperience();

            // Switch through experiences
            if((input.x >= Settings.instance.thumbstickTreshold || input.x <= -Settings.instance.thumbstickTreshold))
            {
                if(!this.lastXInput)
                {
                    // Switch to next experience
                    if(input.x >= Settings.instance.thumbstickTreshold)
                    {
                        this.SwitchExperience(true);
                        this.lastXInput = true;
                    }
                    // Switch to previous experience
                    else if(input.x <= -Settings.instance.thumbstickTreshold)
                    {
                        this.SwitchExperience(false);
                        this.lastXInput = true;
                    }
                }
            }
            else
                this.lastXInput = false;

            // Switch through markers
            if((input.y >= Settings.instance.thumbstickTreshold || input.y <= -Settings.instance.thumbstickTreshold))
            {
                if(!this.lastYInput)
                {
                    // Switch to next marker
                    if(input.y >= Settings.instance.thumbstickTreshold)
                    {
                        this.SwitchPerspective(true);
                        this.lastYInput = true;
                    }
                    // Switch to previous marker
                    else if(input.y <= -Settings.instance.thumbstickTreshold)
                    {
                        this.SwitchPerspective(false);
                        this.lastYInput = true;
                    }               
                }
            }
            else
                this.lastYInput = false;
        }

        // Method that gets called as soon as the client enter a Photon room
        public override void OnJoinedRoom()
        {
            // Check if this is the master client
            if(PhotonNetwork.IsMasterClient)
            {
                // Get all markers attached to controller experience
                Marker[] markers = this.GetComponentsInChildren<Marker>();

                // Initialize markers
                foreach(Marker marker in markers)
                {
                    // Is the initial experience not valid?
                    if(marker.initialExperience <= 0 && this.experiences.Count <= 0)
                        continue;

                    // Get correct cloneable for marker
                    Cloneable cloneable;
                    if(marker.useCustomPrefab && marker.customPrefab != null)
                        cloneable = marker.customPrefab.GetComponent<Cloneable>();
                    else
                        cloneable = markerDefaultPrefab.GetComponent<Cloneable>();

                    // Additional information to process instantiate on remote clients
                    object[] info = { Space.SHARED, true, false };

                    // Instantiate marker for experience
                    GameObject markerObject = PhotonNetwork.Instantiate(cloneable.prefabName, Vector3.zero, Quaternion.identity, 0, info);

                    // Apply correct position and rotation
                    if(marker.useTransformComponent)
                    {
                        // Use the transform component of the marker
                        markerObject.transform.position = new Vector3(marker.transform.position.x, 0, marker.transform.position.z);
                        markerObject.transform.rotation = Quaternion.Euler(0, marker.transform.rotation.eulerAngles.y, 0);
                    }
                    else
                    {
                        // Use the values provided
                        markerObject.transform.position = new Vector3(marker.position.x, 0, marker.position.z);
                        markerObject.transform.rotation = Quaternion.Euler(0, marker.rotation, 0);
                    }

                    // Set initial experience of marker
                    MarkerPrefab markerPrefab = markerObject.GetComponent<MarkerPrefab>();

                    // If initial experience is not provided, take the first one
                    if(marker.initialExperience <= 0)
                        markerPrefab.SetExperience(this.experiences[0]);
                    else
                        markerPrefab.SetExperience(marker.initialExperience);
                }
            }
        }

        // Returns an experience object for the specified experience enum
        public IExperience GetExperience(Experience experience)
        {
            switch(experience)
            {
                case Experience.REALITY:
                    return new ExperienceReality();
                case Experience.AUGMENTEDREALITY:
                    return new ExperienceAugmentedReality();
                case Experience.AUGMENTEDVIRTUALITY:
                    return new ExperienceAugmentedVirtuality();
                case Experience.VIRTUALREALITY:
                    return new ExperienceVirtualReality();
                default:
                    return null;
            }
        }

        // Returns a list with all experiences that are currently enabled
        public List<Experience> GetSelectedExperiences()
        {
            List<Experience> selectedExperiences = new List<Experience>();

            // Iterate through all experiences in the enum
            foreach(Experience experience in Enum.GetValues(typeof(Experience)))
                if((this.activatedExperiences & experience) != 0)
                    selectedExperiences.Add(experience);
                
            return selectedExperiences;
        }

        // Method is called as soon as the player transitions to Experience mode
        public void OnEnterExperiences()
        {
            if(!Settings.instance.IsInitialized())
                return;

            // Select start perspective as players current position
            this.indexMarker = -1;
            this.indexExperience = this.indexLastExperience;

            // Get currently selected experience
            IExperience experience = this.GetCurrentExperience();
            if(experience == null)
            {
                Settings.instance.mode = Mode.SIMULATION;
                return;
            }

            // Enter currently selected
            experience.OnExperienceEnter();
        }

        // Method is called as soon as the player leaves the Experience mode
        public void OnLeaveExperiences()
        {
            // Check if the perspective we leave is not a marker
            if(this.indexMarker != -1)
                this.markerPrefabs[this.indexMarker].OnMarkerLeave(this.experiences[this.indexExperience]);

            // Call leave for current experience
            this.GetCurrentExperience().OnExperienceLeave();

            // Change perspective, if user is currently at marker position
            if(this.indexMarker != -1)
                this.SetPerspective(this.lastPlayerTransform.transform.position, this.lastPlayerTransform.transform.rotation);

            // Reset the last stored experience
            this.lastExperience = null;
        }

        // Method updates the player position and rotation to the last known position and rotation
        private void UpdateLastPlayerTransform()
        {
            this.lastPlayerTransform.transform.position = Camera.main.transform.position;
            this.lastPlayerTransform.transform.rotation = Camera.main.transform.rotation;
        }

        // Returns the currently active experience; and automatically creates a new experience if necessary
        public IExperience GetCurrentExperience()
        {
            if(this.experiences.Count <= 0 || this.indexExperience < 0)
                return null;

            IExperience experience = this.GetExperience(this.experiences[this.indexExperience]);
            if(this.lastExperience != null && this.lastExperience.ToString().Equals(experience.ToString()))
                return this.lastExperience;

            this.lastExperience = experience;
            return experience;
        }

        // Returns true, if the player is currently at a marker position (player must have switched to that position using the experiences interactions)
        public bool IsAtMarker()
        {
            return this.indexMarker != -1;
        }

        // Adds a marker prefab to the list of known prefabs
        public void AddMarkerPrefab(MarkerPrefab markerPrefab)
        {
            this.markerPrefabs.Add(markerPrefab);
        }

        // Removes a marker prefab from the list of known prefabs
        public void RemoveMarkerPrefab(MarkerPrefab markerPrefab)
        {
            this.markerPrefabs.Remove(markerPrefab);
        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }

        // Method that allows players to switch the perspective, meaning the travel to the position of a marker or their last known position
        private void SwitchPerspective(bool next)
        {
            this.GetCurrentExperience().OnExperienceLeave();

            // Check if perspective we leave is a marker
            if(this.indexMarker != -1)
                this.markerPrefabs[this.indexMarker].OnMarkerLeave(this.experiences[this.indexExperience]);
            else
                this.UpdateLastPlayerTransform();

            // Switch to correct perspective
            do
            {
                // Switch perspective to next one
                if(next)
                {
                    this.indexMarker++;
                    if(this.indexMarker >= this.markerPrefabs.Count)
                        this.indexMarker = -1; // to include players initial position as a marker
                }
                // Switch perspective to previous one
                else
                {
                    this.indexMarker--;
                    if(this.indexMarker < -1) // to include players initial position as a marker
                        this.indexMarker = this.markerPrefabs.Count - 1; 
                }

            } while(this.indexMarker != -1 && this.markerPrefabs[this.indexMarker].isUsed);

            // Check if perspective is not a marker
            if(this.indexMarker == -1)
            {
                this.indexExperience = this.indexLastExperience;
                this.SetPerspective(this.lastPlayerTransform.transform.position, this.lastPlayerTransform.transform.rotation);
            }
            // Perspective is marker position
            else
            {
                MarkerPrefab currentMarkerPrefab = this.markerPrefabs[this.indexMarker];
                this.SetPerspective(currentMarkerPrefab.transform.position, currentMarkerPrefab.transform.rotation);
            }

            // Check if entered perspective is a marker
            if(this.indexMarker != -1)
            {
                // Send marker enter event
                Experience experience = this.markerPrefabs[this.indexMarker].OnMarkerEnter();

                // Set correct index for experience
                for(int i=0; i < this.experiences.Count; i++)
                    if(this.experiences[i] == experience)
                        this.indexExperience = i;
            }

            this.GetCurrentExperience().OnExperienceEnter();
        }

        // Method allows players to switch to another experience
        private void SwitchExperience(bool next)
        {
            this.GetCurrentExperience().OnExperienceLeave();

            // Switch experience to next one
            if(next)
            {
                this.indexExperience++;
                if(this.indexExperience >= this.experiences.Count)
                    this.indexExperience = 0;
            }
            // Switch experience to previous one
            else
            {
                this.indexExperience--;
                if(this.indexExperience < 0)
                    this.indexExperience = this.experiences.Count - 1;
            }

            // If we are in no marker position at the moment
            if(this.indexMarker == -1)
                this.indexLastExperience = this.indexExperience;

            this.GetCurrentExperience().OnExperienceEnter();
        }

        // Method initiates the coroutine to fly the player to another perspective (marker or players original position)
        private void SetPerspective(Vector3 position, Quaternion rotation)
        {
            StartCoroutine(FlyTowards(position, rotation));
        }

        // Coroutine that calculates the fly animation
        IEnumerator FlyTowards(Vector3 position, Quaternion rotation)
        {
            Transform trackingSpace = Settings.instance.GetReferences().trackingSpace.transform;

            Vector3 startPosition = trackingSpace.position;
            Quaternion startRotation = trackingSpace.rotation;

            // Limit rotation to rotation around the y-Axis
            //rotation = Quaternion.Euler(0, rotation.eulerAngles.y - Camera.main.transform.localRotation.eulerAngles.y, 0);
            rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            Quaternion cameraRotation = Quaternion.Euler(0, Camera.main.transform.localRotation.eulerAngles.y, 0);
            rotation = rotation * Quaternion.Inverse(cameraRotation);

            // Transform points into local space
            Vector3 localCamera = trackingSpace.InverseTransformPoint(Camera.main.transform.position);
            Vector3 localTarget = trackingSpace.InverseTransformPoint(position);

            // Calculate how to move the tracking space to get the target position
            position = startPosition - (startRotation * ((Quaternion.Inverse(startRotation * Quaternion.Inverse(rotation)) * localCamera) - localTarget));

            // No position change on y-Axis
            position.y = Settings.instance.GetReferences().trackingSpace.transform.position.y;

            float progress = 0;
            while (progress < 1)
            {
                progress += Time.deltaTime * this.durationOfFlight * 10f;
                trackingSpace.position = Vector3.Lerp(startPosition, position, progress);
                trackingSpace.rotation = Quaternion.Lerp(startRotation, rotation, progress);
                yield return null;
            }
        }
    }
}