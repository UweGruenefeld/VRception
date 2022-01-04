using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This script allows to show tooltips on the players controllers
    /// Each controller needs one gameobject with this script attached
    /// </summary>
    public class ControllerTooltips : MonoBehaviour
    {
        //// SECTION "Tooltip Settings"
        [Header("Tooltips Settings", order = 0)]
        [Helpbox("This script allows to show tooltips on the players controllers. Each controller needs one gameobject with this script attached. The tooltips should be avialable as gameobjects, one for each mode avilable. This script then activates the according gameobject and adapts the position of the tooltips to the used Oculus device.", order = 1)]
        [Tooltip("Specify the gameobject with tooltips for the simulation mode.", order = 2)]
        public GameObject tooltipsSimulation = null;

        [Tooltip("Specify the gameobject with tooltips for the calibration mode.")]
        public GameObject tooltipsCalibration = null;

        [Tooltip("Specify the gameobject with tooltips for the configuration mode.")]
        public GameObject tooltipsConfiguration = null;

        [Tooltip("Specify the gameobject with tooltips for the experience mode.")]
        public GameObject tooltipsExperience = null;

        [Tooltip("Specify the gameobject that represents the controller for the Quest 1.")]
        public GameObject modelOculusQuest1 = null;

        [Tooltip("Specify the gameobject that represents the controller for the Quest 2.")]
        public GameObject modelOculusQuest2 = null;

        //// SECTION "Quest 1 Settings"
        [Header("Quest 1 Settings", order = 0)]
        [Helpbox("Below the position values for the Quest 1 controller are hardcoded.", order = 1)]
        [Tooltip("Please specify the location of the index trigger.", order = 2)]
        public Vector3 quest1IndexTrigger = Vector3.zero;

        [Tooltip("Please specify the location of the hand trigger.")]
        public Vector3 quest1HandTrigger = Vector3.zero;

        [Tooltip("Please specify the location of the thumbstick.")]
        public Vector3 quest1Thumbstick = Vector3.zero;

        [Tooltip("Please specify the location of the first button.")]
        public Vector3 quest1ButtonOne = Vector3.zero;

        [Tooltip("Please specify the location of the second button.")]
        public Vector3 quest1ButtonTwo = Vector3.zero;

        //// SECTION "Quest 2 Settings"
        [Header("Quest 2 Settings", order = 0)]
        [Helpbox("Below the position values for the Quest 2 controller are hardcoded.", order = 1)]
        [Tooltip("Please specify the location of the index trigger.", order = 2)]
        public Vector3 quest2IndexTrigger = Vector3.zero;

        [Tooltip("Please specify the location of the hand trigger.")]
        public Vector3 quest2HandTrigger = Vector3.zero;

        [Tooltip("Please specify the location of the thumbstick.")]
        public Vector3 quest2Thumbstick = Vector3.zero;

        [Tooltip("Please specify the location of the first button.")]
        public Vector3 quest2ButtonOne = Vector3.zero;

        [Tooltip("Please specify the location of the second button.")]
        public Vector3 quest2ButtonTwo = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            Settings.instance.onModeTransitionFromSimulation += OnExitModeSimulation;
            Settings.instance.onModeTransitionFromCalibration += OnExitModeCalibration;
            Settings.instance.onModeTransitionFromConfiguration += OnExitModeConfiguration;
            Settings.instance.onModeTransitionFromExperience += OnExitModeExperience;

            this.tooltipsSimulation.SetActive(false);
            this.tooltipsCalibration.SetActive(false);
            this.tooltipsConfiguration.SetActive(false);
            this.tooltipsExperience.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            // Is Settings not initialized yet?
            if(!Settings.instance.IsInitialized())
                return;

            // Current tooltips
            GameObject currentTooltips = null;

            switch(Settings.instance.mode)
            {
                case Mode.CALIBRATION:
                    currentTooltips = this.tooltipsCalibration;
                    break;
                case Mode.CONFIGURATION:
                    currentTooltips = this.tooltipsConfiguration;
                    break;
                case Mode.EXPERIENCE:
                    currentTooltips = this.tooltipsExperience;
                    break;
                default:
                    currentTooltips = this.tooltipsSimulation;
                    break;
            }

            // Is button pressed to show tooltips and a controller model is shown?
            if(Mapping.ButtonTooltips() && (this.modelOculusQuest1.activeSelf || this.modelOculusQuest2.activeSelf))
                currentTooltips.SetActive(true);
            else
                currentTooltips.SetActive(false);

        }

        // Get the location for a specific anchor
        public Vector3 GetPositionOfAnchor(TooltipAnchor anchor)
        {
            // Is model of Oculus Quest 1 displayed?
            if(this.modelOculusQuest1 != null && this.modelOculusQuest1.activeSelf)
            {
                switch(anchor)
                {
                    case TooltipAnchor.INDEX_TRIGGER:
                        return this.quest1IndexTrigger;
                    case TooltipAnchor.HAND_TRIGGER:
                        return this.quest1HandTrigger;
                    case TooltipAnchor.THUMBSTICK:
                        return this.quest1Thumbstick;
                    case TooltipAnchor.BUTTON_ONE:
                        return this.quest1ButtonOne;
                    case TooltipAnchor.BUTTON_TWO:
                        return this.quest1ButtonTwo;
                    default:
                        break;
                }
            }

            // Is model of Oculus Quest 2 displayed?
            if(this.modelOculusQuest2 != null && this.modelOculusQuest2.activeSelf)
            {
                switch(anchor)
                {
                    case TooltipAnchor.INDEX_TRIGGER:
                        return this.quest2IndexTrigger;
                    case TooltipAnchor.HAND_TRIGGER:
                        return this.quest2HandTrigger;
                    case TooltipAnchor.THUMBSTICK:
                        return this.quest2Thumbstick;
                    case TooltipAnchor.BUTTON_ONE:
                        return this.quest2ButtonOne;
                    case TooltipAnchor.BUTTON_TWO:
                        return this.quest2ButtonTwo;
                    default:
                        break;
                }
            }

            return Vector3.zero;
        }

        // Method is called if the simulation mode is left
        public void OnExitModeSimulation()
        {
            this.tooltipsSimulation.SetActive(false);
        }

        // Method is called if the calibration mode is left
        public void OnExitModeCalibration()
        {
            this.tooltipsCalibration.SetActive(false);
        }

        // Method is called if the configuration mode is left
        public void OnExitModeConfiguration()
        {
            this.tooltipsConfiguration.SetActive(false);
        }

        // Method is called if the experience mode is left
        public void OnExitModeExperience()
        {
            this.tooltipsExperience.SetActive(false);
        }
    }
}
