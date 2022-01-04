using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Simple implementation of the Augmented Reality manifestation as an experience
    /// </summary>
    public class ExperienceAugmentedReality : IExperience
    {
        private GameObject filter;

        // Constructor of the experience
        public ExperienceAugmentedReality() : base("Augmented Reality")
        {

        }

        // Method is called when the player enters this experience
        public override void OnEnter()
        {
            // To limit the FOV for a more realistic Augmented Reality experience,
            // we generate a mesh below (basically a rectangle with a rectabgle cut out in the center)
            // to use it to limit the FOV; this works because we use a material for the mesh that
            // is processed very early and sets the depth buffer for the virtuality camera 
            // to a very close value so that nothing behind that value (basically all AR content)
            // is not rendered anymore where the mesh is positioned.
            // As a result, only the cutout rectangle in the center of the mesh is rendered.
            // An alternative version, in which we adjusted the FOV and View Rect of the virtuality camera
            // did not work because in VR all cameras that use stereo eye targets (both) are
            // automatically set to the same FOV and a different view rect destroys the rendering.

            // Specify position on crossfader to have the correct camera order
            Settings.instance.crossfader = -0.5f;

            // Get camera of virtuality
            Camera camera = Settings.instance.GetReferences().cameraRightLayer.GetComponent<Camera>();

            // Create filter gameobject and adjust its psoition
            this.filter = new GameObject("Filter");
            this.filter.transform.parent = camera.transform;
            this.filter.transform.localPosition = new Vector3(0, 0, .4f);
            this.filter.transform.localRotation = Quaternion.identity;

            // Set the filter gameobject to the correct layer (so it is rendered by the virtuality camera)
            Utilities.SetLayerRecursively(this.filter, Space.RIGHT);

            // Create the mesh
            Mesh mesh = new Mesh();
            MeshFilter meshFilter = this.filter.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            // Generate mesh that basically is a rectangle with a smaller rectangle cut out in the center
            Vector3[] newVertices = new Vector3[]{ new Vector3(-2, -1, 0), new Vector3(-2, 1, 0), new Vector3(2, 1, 0), new Vector3(2, -1, 0),
                                            new Vector3(-.15f, -.1f, 0), new Vector3(-.15f, .05f, 0), new Vector3(.15f, .05f, 0), new Vector3(.15f, -.1f, 0) };
            int[] newTriangles = new int[] { 0, 1, 4, 1, 5, 4, 1, 2, 5, 2, 6, 5, 2, 3, 6, 3, 7, 6, 3, 0, 7, 0, 4, 7 };

            // Assign the mesh to the filter ganmeobject meshfilter component
            mesh.vertices = newVertices;
            mesh.triangles = newTriangles;

            // Set the correct material for the filter gameobject
            MeshRenderer meshRenderer = this.filter.AddComponent<MeshRenderer>();
            meshRenderer.material = Resources.Load("Materials/Invisible", typeof(Material)) as Material;
        }

        // Method is called when the player leaves this experience
        public override void OnLeave()
        {
            // Remove the added filter gameobject
            MonoBehaviour.Destroy(this.filter);
        }

        // While this experience is active, this method is called in every frame before a camera renders the frame
        public override void OnRender(Camera camera, Space space)
        {
            // Apply alpha to correct game objects
            switch(space)
            {
                case Space.RIGHT:
                    CameraAlpha.SetAlphaToSpace(space, 0.5f);
                    break;
            }
        }
    }
}