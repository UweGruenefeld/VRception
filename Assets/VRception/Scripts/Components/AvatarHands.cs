using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This script allows to reconfigure the hands of the Oculus avatar
    /// </summary>
    public class AvatarHands : MonoBehaviour
    {
        //// SECTION "Hands Settings"
        [Header("Hands Settings", order = 0)]
        [Helpbox("This script allows to reconfigure the hands of the Oculus avatar. Below, one can specify which material the hands should use for rendering.", order = 1)]
        [Tooltip("Specify the material used to render the hands.", order = 2)]
        public Material material;

        // Store if hands are initialized already
        private bool initialized;

        // Start is called before the first frame update
        void Start()
        {
            this.initialized = false;
        }

        // Update is called once per frame
        void Update()
        {
            // Wait for the hands to be loaded
            if(!this.initialized && this.transform.childCount > 0)
            {
                this.SetMaterialRecursive(this.gameObject);
                this.initialized = true;
            }
        }

        // Recursivly set the correct material
        private void SetMaterialRecursive(GameObject obj)
        {
            if(obj.name.Contains("hand"))
            {
                SkinnedMeshRenderer meshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
                if(meshRenderer != null)
                    meshRenderer.material = this.material;
            }

            foreach(Transform transform in obj.transform)
                this.SetMaterialRecursive(transform.gameObject);
        }
    }
}