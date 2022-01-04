using UnityEngine;
using UnityEngine.Rendering;

namespace VRception
{
    /// <summary>
    /// The module line enables one to draw a line between this interactable and any target gameobject.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleLine : MonoBehaviour, IEventCopy
    {
        //// SECTION "Line Settings"
        [Header("Line Settings", order = 0)]
        [Helpbox("The module line enables one to draw a line between this interactable and any target gameobject. Thus, one can express a logical connection between an interactable and another gameobject/interactable. For example, we use this module to show that the subinteractables 'LookAt' and 'Position' belong to the interactable 'Display.'", order = 1)]
        [Tooltip("Specify the target to which the line is drawn.", order = 2)]
        public GameObject target = null;

        private Interactable interactable = null;

        private GameObject lineObject = null;
        private LineRenderer lineRenderer = null;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();
        }

        // Start is called before the first frame update
        void Start()
        {
            this.CreateLine();
        }

        // Update is called once per frame
        void Update()
        {
            // Is line renderer not existing?
            if(this.lineRenderer == null)
                return;

            // Update line renderer
            var points = new Vector3[2];
            points[0] = this.transform.position;
            points[1] = this.target.transform.position;
            lineRenderer.SetPositions(points);
        }

        // Method gets invoked before the interactable is copied
        public void OnBeforeCopy()
        {
            // Destroy line
            this.DestroyLine();
        }

        // Method gets invoked after the interactable is copied
        public void OnAfterCopy()
        {
            // Recreate line
            this.CreateLine();
        }

        // Method to create all gameobjects and components relevant to the line
        private void CreateLine()
        {
            // Create game object for line
            this.lineObject = new GameObject("Line");
            this.lineObject.transform.parent = this.transform;
            this.lineObject.transform.localPosition = Vector3.zero;
            this.lineObject.transform.localRotation = Quaternion.identity;
            this.lineObject.transform.localScale = Vector3.one;

            // Add line renderer to game object
            this.lineRenderer = this.lineObject.AddComponent<LineRenderer>();
            this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            this.lineRenderer.material.renderQueue = 3050;
            this.lineRenderer.startColor = Color.black;
            this.lineRenderer.endColor = Color.black;
            this.lineRenderer.startWidth = 0.01f;
            this.lineRenderer.endWidth = 0.01f;
            this.lineRenderer.receiveShadows = false;
            this.lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            this.lineRenderer.useWorldSpace = true;
            this.lineRenderer.positionCount = 2;

            // Set correct layer
            Utilities.SetLayerRecursively(this.lineObject, this.interactable.GetSpace());
        }

        // Method to remove all gameobjects relevant to the line
        private void DestroyLine()
        {
            if(this.lineObject != null)
                DestroyImmediate(this.lineObject);
        }
    }
}