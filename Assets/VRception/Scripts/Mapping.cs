using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This class bundles all controller bindings / function to button maaping. 
    /// Since every interactions in the VRception toolkit is capsuled by a method below, all controller bindings can be changed by alternating this file.
    /// </summary>
    public class Mapping
    {
        //
        // Simulation
        //

        public static bool ButtonSimulationPlayPause()
        {
            return
                Input.GetKeyDown(KeyCode.P); // || 
                //OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        }

        // 
        // Mode
        //

        public static bool ButtonModeCalibration()
        {
            return 
                Input.GetKeyDown(KeyCode.Alpha1) || 
                OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick);
        }

        public static bool ButtonModeConfiguration()
        {
            return
                Input.GetKeyDown(KeyCode.Alpha2) || 
                OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick);
        }

        public static bool ButtonModeExperience()
        {
            return
                Input.GetKeyDown(KeyCode.Alpha0) || 
                OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        }

        //
        // Interface
        //

        public static bool ButtonInterfaceOpen()
        {
            return
                Input.GetKeyDown(KeyCode.Space) || 
                OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);
        }

        public static float Axis1DCrossfader()
        {
            return
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).x;
        }

        public static float Axis1DScrolling()
        {
            return
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).y;
        }

        public static bool ButtonCrossfaderToLeft()
        {
            return
                Input.GetKeyDown(KeyCode.A);
        }

        public static bool ButtonCrossfaderToRight()
        {
            return
                Input.GetKeyDown(KeyCode.S);
        }

        //
        // Experiences
        //

        public static Vector2 Axis2DExperience()
        {
            return
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        }

        //
        // Calibration
        //

        public static Quaternion QuaternionCalibrationControllerOrientationForTranslateXY()
        {
            return
                OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
        }

        public static Vector2 Axis2DCalibrationTranslateXY()
        {
            return
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        }

        public static float Axis1DCalibrationTranslateZ()
        {
            return
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;
        }

        public static float Axis1DCalibrationRotate()
        {
            return
                OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
        }

        public static bool ButtonPlayerCalibration()
        {
            return
                Input.GetKeyDown(KeyCode.C) || 
                OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);
        }

        //
        // Player
        //

        public static bool ButtonPlayerSwitchCharacter()
        {
            return false;
                //OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        }

        public static bool ButtonPlayerVomiting()
        {
            return false;
                //OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        }

        //
        // Interactable
        //

        public static Quaternion Axis3DInteractableControllerOrientation()
        {
            return Quaternion.Inverse(
                Quaternion.Inverse(Settings.instance.GetReferences().trackingSpace.transform.rotation)) * 
                OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        }

        public static Vector3 Axis3DInteractableControllerPosition()
        {
            if(!Settings.instance.IsInitialized())
                return Vector3.zero;

            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            return Settings.instance.GetReferences().trackingSpace.transform.TransformPoint(controllerPosition);
        }

        public static Vector3 Axis3DInteractableControllerRotation()
        {
            if(!Settings.instance.IsInitialized())
                return Vector3.zero;

            Vector3 controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch).eulerAngles;
            return Settings.instance.GetReferences().trackingSpace.transform.TransformDirection(controllerRotation);
        }

        public static Vector3 Axis3DInteractableScale()
        {
            if(!Settings.instance.IsInitialized())
                return Vector3.zero;

            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            return Settings.instance.GetReferences().trackingSpace.transform.TransformPoint(controllerPosition);
        }

        public static bool ButtonInteractableSelect()
        {
            return
                Input.GetKey(KeyCode.M) || 
                OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) >= Settings.instance.triggerTreshold;
        }

        public static bool ButtonInteractableScale()
        {
            return
                OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) >= Settings.instance.triggerTreshold;
        }

        public static bool ButtonInteractableDelete()
        {
            return
                OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        }

        public static bool ButtonInteractableDuplicate()
        {
            return
                OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        }

        //
        // Tooltips
        //
        public static bool ButtonTooltips()
        {
            return
                Input.GetKey(KeyCode.T) || 
                OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch);
        }

        //
        // Voice 
        // 
        public static bool ButtonVoice()
        {
            return
                Input.GetKeyDown(KeyCode.V) || 
                OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
        }
    }
}
