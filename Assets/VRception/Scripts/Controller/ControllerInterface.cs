using System;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This controller script is responsible for the interface of the virtual menu attached to players left controller
    /// TODO add possibility to have menu on the other hand and use the left hand as the dominant one
    /// </summary>
    public class ControllerInterface : MonoBehaviour
    {
        // Factor used to convert the crossfader value into movement in space
        // TODO should be calculated automatically
        public static readonly float SLIDER_CONVERSION_FACTOR = 0.0839f;

        //// SECTION "Interface References"
        [Header("Interface References", order = 0)]
        [Helpbox("This controller script is responsible for the interface of the virtual menu attached to players left controller. Below, one can specify the references to the required gameobjects. Settings of the interface are outsourced to the 'General Settings.'", order = 1)]
        [Tooltip("Specify reference to the anchor gameobject.", order = 2)]
        public GameObject anchor;

        [Tooltip("Specify reference to the slider knob gameobject.")]
        public GameObject sliderKnob;

        [Tooltip("Specify reference to the left label gameobject.")]
        public GameObject labelLeft;

        [Tooltip("Specify reference to the right label gameobject.")]
        public GameObject labelRight;

        [Tooltip("Specify reference to the gameobject that provides the position for the prefab in the first slot.")]
        public GameObject prefabSlotA;

        [Tooltip("Specify reference to the gameobject that provides the position for the prefab in the second slot.")]
        public GameObject prefabSlotB;

        [Tooltip("Specify reference to the gameobject that provides the position for the prefab in the third slot.")]
        public GameObject prefabSlotC;

        [Tooltip("Specify reference to the gameobject that provides the position for the prefab in the fourth slot.")]
        public GameObject prefabSlotD;

        [Tooltip("Specify reference to the upwards indicator gameobject.")]
        public GameObject indicatorUpwards;

        [Tooltip("Specify reference to the downwards indicator gameobject.")]
        public GameObject indicatorDownwards;

        // Store all instantiated predefined objects (prefabs)
        private GameObject[] loadedPrefabs;

        // Store current state of the continuum menu
        private bool active;
        private Vector3 lastSliderKnobPosition;
        private int listPosition;
        private int lastListPosition;
        private bool lastScrollInput;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.active = false;

            this.lastSliderKnobPosition = Vector3.zero;

            this.listPosition = 0;
            this.lastListPosition = -1;
            this.lastScrollInput = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Make sure that menu closes for modes in which it should not be accessable
            Settings.instance.onModeTransitionToCalibration += this.OnCloseMenu;
            Settings.instance.onModeTransitionToExperience += this.OnCloseMenu;

            // Register for crossfader change
            Settings.instance.onCrossfaderChange += this.OnCrossfaderChange;

            // Set label on above slider
            labelLeft.GetComponent<TextMesh>().text = ControllerSpaces.instance.leftName;
            labelRight.GetComponent<TextMesh>().text = ControllerSpaces.instance.rightName;

            // Instantiate the predefined objects
            GameObject[] prefabs = Settings.instance.predefinedObjects;
            if(prefabs != null)
            {
                // Initialize array with loaded prefabs
                this.loadedPrefabs = new GameObject[prefabs.Length];

                // Iterate through all predefined objects
                for(int i=0; i < prefabs.Length; i++)
                {
                    // Check if prefab exists
                    if(prefabs[i] == null)
                        continue;

                    // Check if prefab has a cloneable component
                    Cloneable cloneable = prefabs[i].GetComponent<Cloneable>();

                    if(cloneable == null || String.IsNullOrEmpty(cloneable.prefabName))
                    {
                        Debug.LogError("Prefabs shown in the interface require a cloneable component referencing their prefab.");
                        continue;
                    }

                    // Instantiate prefab
                    this.loadedPrefabs[i] = Instantiate(prefabs[i], Vector3.zero, Quaternion.identity);
                    this.loadedPrefabs[i].SetActive(false);

                    // Attach to correct space
                    ControllerSpaces.instance.MoveGameObjectToSpace(this.loadedPrefabs[i], Space.SHARED);

                    // Set correct layer and parent
                    Utilities.SetLayerRecursively(this.loadedPrefabs[i], Space.SHARED);
                    this.loadedPrefabs[i].transform.parent = this.transform;
                }
            }

            // Update interface
            this.OnRefresh();
        }

        // Update is called once per frame
        void Update()
        {
            // Is Settings not initialized yet?
            if(!Settings.instance.IsInitialized())
                return;

            // If in wrong mode, skip this frame
            if(Settings.instance.mode == Mode.CALIBRATION || Settings.instance.mode == Mode.EXPERIENCE)
                return;

            // Menu button pressed?
            if (Mapping.ButtonInterfaceOpen())
            {
                this.active = !this.active;
                this.OnRefresh();
            }

            // Menu active?
            if(this.active)
            {
                this.UpdateSlider();
                this.UpdateList();
            }
        }

        // Method is called if menu should be closed
        public void OnCloseMenu()
        {
            this.active = false;
            this.OnRefresh();
        }

        // Method is called if the knob position of the crossfader should be updated
        public void OnCrossfaderChange()
        {
            // Get original crossfader position
            Vector3 position = this.sliderKnob.transform.localPosition;

            // Update knob position
            this.sliderKnob.transform.localPosition = new Vector3(this.CrossfaderToPosition(), position.y, position.z);
        }

        // Method refreshes the menu by applying the current status (is active or not) to the main gameobject
        private void OnRefresh()
        {
            this.anchor.SetActive(this.active);
        }

        // Internal method to update the gameobjects assembling the crossfader
        private void UpdateSlider()
        {
            // Did someone move the slider knob?
            if(this.lastSliderKnobPosition != this.sliderKnob.transform.localPosition)
            {
                this.lastSliderKnobPosition = this.sliderKnob.transform.localPosition;
                Settings.instance.crossfader = this.PositionToCrossfader(this.lastSliderKnobPosition.x);
            }

            // Get controller input
            float input = Mapping.Axis1DCrossfader();
            input *= Settings.instance.faderSensitivity;

            // Control within Unity
            if(Mapping.ButtonCrossfaderToLeft())
                input = -0.25f;
            else if(Mapping.ButtonCrossfaderToRight())
                input = 0.25f;  

            // Is there an input?
            if(input != 0)
            {
                // Calculate value of slider
                float tempCrossfader = Settings.instance.crossfader + input;

                // Adjust value to be in range
                if(tempCrossfader >= 1)
                    tempCrossfader = 1;
                else if(tempCrossfader <= -1)
                    tempCrossfader = -1;

                // Automatically snap to center of crossfader
                if(tempCrossfader <= Settings.instance.snapToCenter && tempCrossfader >= -Settings.instance.snapToCenter)
                    tempCrossfader = 0;

                // Set new value
                Settings.instance.crossfader = tempCrossfader;
            }
        }

        // Internal method to update the gameobjects assembling the list with predefined objects (prefabs)
        private void UpdateList()
        {
            // Check for input
            float input = Mapping.Axis1DScrolling();

            // Scroll through items in the list
            if((input >= Settings.instance.thumbstickTreshold || input <= -Settings.instance.thumbstickTreshold))
            {
                if(!this.lastScrollInput)
                {
                    // Switch to previous items
                    if(input >= Settings.instance.thumbstickTreshold)
                    {
                        if(this.listPosition >= Settings.instance.numberOfScrolledObjects)
                            this.listPosition -= Settings.instance.numberOfScrolledObjects;

                        this.lastScrollInput = true;
                    }
                    // Switch to next items
                    else if(input <= -Settings.instance.thumbstickTreshold)
                    {
                        if(this.listPosition < this.loadedPrefabs.Length - 4)
                            this.listPosition += Settings.instance.numberOfScrolledObjects;

                        this.lastScrollInput = true;
                    }
                }
            }
            else
                this.lastScrollInput = false;

            // Check if list has changed
            if(this.listPosition == this.lastListPosition)
                return;

            // Refresh indicators
            if(this.listPosition > 0)
                this.indicatorUpwards.SetActive(true);
            else
                this.indicatorUpwards.SetActive(false);
            
            if(this.listPosition < this.loadedPrefabs.Length - 4)
                this.indicatorDownwards.SetActive(true);
            else
                this.indicatorDownwards.SetActive(false);

            // Disable and remove all predefined objects currently visible in the list
            GameObject[] slots = new GameObject[]{ this.prefabSlotA, this.prefabSlotB, this.prefabSlotC, this.prefabSlotD };
            foreach(GameObject obj in slots)
            {
                if(obj.transform.childCount > 0)
                {
                    Transform prefab = obj.transform.GetChild(0);
                    prefab.gameObject.SetActive(false);
                    prefab.parent = this.transform;
                }
            }

            // Enable all predefined objects that should be visible
            for(int i = this.listPosition; i < this.listPosition + 4 && i < this.loadedPrefabs.Length; i++)
            {
                // Get next loaded predefined object
                GameObject prefab = this.loadedPrefabs[i];

                // If slot is empty, ignore it
                if(prefab == null)
                    continue;

                // Assign it to the correct slot
                int index = i - this.listPosition;
                prefab.transform.parent = slots[index].transform;

                // Adjust position and rotation
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localRotation = Quaternion.identity;

                prefab.SetActive(true);
            }

            // Store the changed position
            this.lastListPosition = this.listPosition;
        }

        // Return the space position of the crossfader from its value
        private float CrossfaderToPosition()
        {
            return Settings.instance.crossfader * ControllerInterface.SLIDER_CONVERSION_FACTOR;
        }

        // Return the crossfader value for a position in space
        private float PositionToCrossfader(float position)
        {
            return position / ControllerInterface.SLIDER_CONVERSION_FACTOR;
        }
    }
}