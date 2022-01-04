namespace VRception
{
    /// <summary>
    /// The VRception toolkit supports different modes. All available modes are defined and explained in this enum.
    /// </summary>
    public enum Mode
    {
        SIMULATION,     // default mode: simulation is running
        CALIBRATION,    // calibration mode: simulation is running, translation and rotation of spaces (reality/virtuality) adjustable
        CONFIGURATION,  // configuration state: simulation is running, cameras (from displays and projectors) can be configured - their translation and where they look at
        EXPERIENCE      // experience state: simulation running, one can switch between different user experiences (R/AR/AV/VR/custom) and menu is disabled
    }
}