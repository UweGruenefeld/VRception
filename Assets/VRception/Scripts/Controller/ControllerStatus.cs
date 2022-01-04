using System;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The status controller is responsible for the overlay that is used to display currently active modes and the toast messages that give the player more information
    /// </summary>
    public class ControllerStatus : MonoBehaviour
    {
        //// SECTION "Status Settings"
        [Header("Status Settings", order = 0)]
        [Helpbox("The status controller is responsible for the overlay that is used to display currently active modes (except for the default Simulation Mode) and the toast messages that give the player more information about the VRception toolkit. Toast messages are similar to the toasts send in Android.", order = 1)]
        [Tooltip("Reference for text that displays the currently active mode.", order = 2)]
        public TextMesh overlayText;

        [Tooltip("Reference to renderer for transparent overlay plane.")]
        public Renderer overlayRenderer;

        [Tooltip("Reference for the text mesh that shows the toast message.")]
        public TextMesh toastText;

        [Tooltip("Reference for background gameobject on which the toast is presented.")]
        public GameObject toastBackground;

        // Start is called before the first frame update
        void Start()
        {
            // Register to listen on all mode changes
            Settings.instance.onModeTransitionToSimulation += SetModeToSimulation;
            Settings.instance.onModeTransitionToCalibration += SetModeToCalibration;
            Settings.instance.onModeTransitionToConfiguration += SetModeToConfiguration;
            Settings.instance.onModeTransitionToExperience += SetModeToExperience;

            // Set current mode
            this.SetMode(Settings.instance.mode);
        }

        // Update is called once per frame
        void Update()
        {
            // Is Settings not initialized yet?
            if(!Settings.instance.IsInitialized())
                return;

            // Show experience mode status
            if(Settings.instance.mode == Mode.EXPERIENCE)
            {
                IExperience experience = ControllerExperiences.instance.GetCurrentExperience();
                if(experience == null)
                    this.overlayText.text = "Experience: None specified";
                else
                    this.overlayText.text = "Experience: " + experience.ToString();

                bool isAtMarker = ControllerExperiences.instance.IsAtMarker();
                if(isAtMarker)
                    this.overlayText.text += "\nAt Marker";
            }

            // Update toast message
            String toast = Settings.instance.GetToast();
            this.toastText.text = toast;

            if(!String.IsNullOrEmpty(toast))
            {
                float width = Utilities.GetWidth(this.toastText);
                this.toastBackground.SetActive(true);
                this.toastBackground.transform.localScale = new Vector3((width / 10f) + 0.004f, 1, 0.006f);
            }
            else
                this.toastBackground.SetActive(false);
        }

        // Method called when mode is transitioning to simulation mode
        public void SetModeToSimulation()
        {
            this.SetMode(Mode.SIMULATION);
        }

        // Method called when mode is transitioning to calibration mode
        public void SetModeToCalibration()
        {
            this.SetMode(Mode.CALIBRATION);
        }

        // Method called when mode is transitioning to configuration mode
        public void SetModeToConfiguration()
        {
            this.SetMode(Mode.CONFIGURATION);
        }

        // Method called when mode is transitioning to experience mode
        public void SetModeToExperience()
        {
            this.SetMode(Mode.EXPERIENCE);
        }

        // Method that changes the mode displayed to the one specfied
        private void SetMode(Mode mode)
        {
            switch(mode)
            {
                case Mode.CALIBRATION:
                    this.overlayText.text = "Calibration Mode";
                    this.overlayRenderer.material.color = new Color(1,0,0,0.55f);
                    break;
                case Mode.CONFIGURATION:
                    this.overlayText.text = "Configuration Mode";
                    this.overlayRenderer.material.color = new Color(0,0,1,0.55f);
                    break;
                case Mode.EXPERIENCE:
                    this.overlayText.text = "";
                    this.overlayRenderer.material.color = new Color(0,0,0,0.35f);
                    break;
                default:
                    this.overlayText.text = "Default Mode";
                    this.overlayRenderer.material.color = new Color(0,0,0,0f);
                    break;
            }

            if(mode == Mode.SIMULATION)
            {
                this.overlayRenderer.gameObject.SetActive(false);
                this.overlayText.gameObject.SetActive(false);
            }
            else
            {
                this.overlayRenderer.gameObject.SetActive(true);
                this.overlayText.gameObject.SetActive(true);
            }
        }
    }
}