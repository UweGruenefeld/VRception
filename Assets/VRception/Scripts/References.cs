using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This class bundles the references to the most important gameobjects. An instance of this class is assigned to the player and from there linjked to the Settings script. 
    /// Thereby, it is accessible by every script (singleton pattern).
    /// </summary>
    public class References : MonoBehaviour
    {
        [Header("References", order = 0)]
        [Helpbox("This class bundles the references to the most important gameobjects related to the main player.")]
        [Tooltip("Tracking Space of OVRCameraSpace.")]
        public GameObject trackingSpace = null;

        [Tooltip("Camera rendering all gameobjects in the left space.")]
        public GameObject cameraLeftLayer = null;

        [Tooltip("Camera rendering all gameobjects in the right space.")]
        public GameObject cameraRightLayer = null;

        [Tooltip("Gameobject containing the model for the left controller.")]
        public GameObject leftController = null;

        [Tooltip("Gameobject containing the model for the right controller.")]
        public GameObject rightController = null;
    }
}