using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This script allows to turn any gameobject into a billboard, meaning it is always facing the main camera
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        //// SECTION "Billboard Settings"
        [Header("Billboard Settings", order = 0)]
        [Helpbox("This script allows to turn any gameobject into a billboard, meaning it is always facing the main camera. Below, the billboard feature can be enabled and disabled.", order = 1)]
        [Tooltip("Specify if the billboard feature is enabled.", order = 2)]
        public bool active = true;

        // Update is called once per frame
        void Update()
        {
            if(Camera.main == null || this.active == false)
                return;
                
            Vector3 cameraPosition = Camera.main.transform.position;
            this.transform.LookAt(cameraPosition, Vector3.up);
            this.transform.Rotate(new Vector3(0, 180, 0));
        }
    }
}
