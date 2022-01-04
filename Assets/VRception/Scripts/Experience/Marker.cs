using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Markers can be placed anywhere and can be assigned one specific experience. Thereby, players are able to quickly jump to a marker position and switch to its assigned experience.
    /// This script must be assigned to a child gameobject of the "Experience Settings" gameobject to create a marker. 
    /// The GUI of this script visble in the Editor is implemented in "Editor > MarkerEditor."
    /// </summary>
    public class Marker : MonoBehaviour
    {
        //// SECTION "Marker Settings"
        public Experience initialExperience;
        public bool useCustomPrefab = false;
        public GameObject customPrefab = null;

        //// SECTION "Transform Settings"
        public bool useTransformComponent = true;
        public Vector3 position = Vector3.zero;
        public float rotation = 0;
    }
}