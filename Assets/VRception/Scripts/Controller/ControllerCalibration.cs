using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This controller script enables players to adjust their tracking space (ie. translate on x, y, z, and rotate on y)
    /// If activated, the tracking space can be adjusted in 'Calibration Mode' and 'Experience Mode' (only x and z)
    /// </summary>
    public class ControllerCalibration : MonoBehaviour
    {
        //// SECTION "Calibration Settings"
        [Header("Calibration Settings", order = 0)]
        [Helpbox("This controller script enables players to adjust their tracking space (ie. translate on x, y, z, and rotate on y). If activated below, the tracking space can be adjusted in 'Calibration Mode' and 'Experience Mode' (only x and z). The latter is to simulate movement and should be used when the physical space is not calibrated to the simulation.", order = 1)]
        [Tooltip("If yes, then the tracking space can be adjusted in calibration mode.", order = 2)]
        public bool activeInCalibrationMode = true;

        [Tooltip("If yes, then movement can be simulated in 'Experience Mode.'")]
        public bool activeInExperienceMode = true;

        // Update is called once per frame
        void LateUpdate()
        {
            // Is Settings not initialized yet?
            if(!Settings.instance.IsInitialized())
                return;

            // Is simulation not in calibration or experience mode?
            if(Settings.instance.mode != Mode.CALIBRATION && Settings.instance.mode != Mode.EXPERIENCE)
                return;

            // Is simulation in calibration mode but it is not activated during that mode?
            if(Settings.instance.mode == Mode.CALIBRATION && !this.activeInCalibrationMode)
                return;

            // Is simulation in calibration mode but it is not activated during that mode?
            if(Settings.instance.mode == Mode.EXPERIENCE && !this.activeInExperienceMode)
                return; 

            // Get controller rotation
            Quaternion orientationController = Mapping.QuaternionCalibrationControllerOrientationForTranslateXY();
            orientationController = orientationController * Settings.instance.GetReferences().trackingSpace.transform.rotation;

            // Get translation and rotation
            Vector2 translateXY = Mapping.Axis2DCalibrationTranslateXY();
            float translateZ = Mapping.Axis1DCalibrationTranslateZ();
            float rotation = Mapping.Axis1DCalibrationRotate();

            // If in experience mode, do not allow rotation and y-axis change
            if(Settings.instance.mode == Mode.EXPERIENCE)
            {
                translateZ = 0;
                rotation = 0;
            }

            // Get current position
            Vector3 position = Settings.instance.GetReferences().trackingSpace.transform.position;

            // Adjust thumbstick input with regard to controller rotation
            translateXY = translateXY.Rotate(-orientationController.eulerAngles.y);

            Vector3 manipulation = new Vector3(
                translateXY.x * Settings.instance.speedTranslate * Time.deltaTime, 
                translateZ * Settings.instance.speedTranslate * Time.deltaTime, 
                translateXY.y * Settings.instance.speedTranslate * Time.deltaTime
            );

            // Translate the room
            Settings.instance.GetReferences().trackingSpace.transform.position = new Vector3(
                position.x + manipulation.x, 
                position.y + manipulation.y, 
                position.z + manipulation.z
            );

            // Rotate the room
            Settings.instance.GetReferences().trackingSpace.transform.RotateAround(
                Camera.main.transform.position, 
                Vector3.up, 
                rotation * Settings.instance.speedRotate * Time.deltaTime * 45
            );
        }
    }
}
