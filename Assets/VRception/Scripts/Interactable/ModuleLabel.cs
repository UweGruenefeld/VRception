using UnityEngine;
using UnityEngine.Rendering;

namespace VRception
{
    /// <summary>
    /// The label module is used to provide information for interactables in-game.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleLabel : MonoBehaviour, IEventCopy
    {
        //// SECTION "Label Settings"
        [Header("Label Settings", order = 0)]
        [Helpbox("The label module is used to provide information for interactables in-game. Below one can specify which label should be displayed and under which circumstances the information is visible to players. The label is presented with white font and a black background above the center of the interactable facing the player.", order = 1)]
        [Tooltip("Specify the label that is displayed for the interactable in-game.", order = 2)]
        public string label = "";

        [Tooltip("If yes, then the label is always visible in-game.")]
        public bool visibleAlways = false;

        [Tooltip("If yes, then the label is only visible when the interactable is hovered. Hovered as child is ignored and always visible overwrites this attribute.")]
        public bool visibleOnHover = true;

        private Interactable interactable = null;
        private Renderer modelRenderer = null;

        private GameObject labelObject;
        private GameObject labelOffset;
        private TextMesh textMesh;

        // Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        void OnValidate()
        {
            if(string.IsNullOrEmpty(this.label))
                this.label = this.gameObject.name;
        }

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();
            this.modelRenderer = this.interactable.modelRenderer;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Create label
            this.CreateLabel();
        }

        // Update is called once per frame
        void Update()
        {
            // Check if label exists
            if(this.labelObject == null)
                return;

            // Make label always face the observer
            this.labelObject.transform.LookAt(Settings.instance.GetCameraTransform(), Vector3.up);

            // Adjust label position
            float maxBound = this.modelRenderer.bounds.extents.magnitude;
            this.labelOffset.transform.localPosition = new Vector3(0, 0, maxBound);

            // Is objected currently selected?
            if(this.interactable.IsSelected())
            {
                this.SetLabelActive(false);
                return;
            }

            // Should it always be active?
            if(this.visibleAlways)
            {
                this.SetLabelActive(true);
                return;
            }

            // Should it be active when highlighted?
            if(this.visibleOnHover)
            {
                // Only if object is highlighted 
                if(this.interactable.IsHovered())
                {
                    this.SetLabelActive(true);
                    return;
                }
            }

            this.SetLabelActive(false);
        }

        // Method gets invoked before the interactable is copied
        public void OnBeforeCopy()
        {
            // Destroy label
            this.DestroyLabel();
        }

        // Method gets invoked after the interactable is copied
        public void OnAfterCopy()
        {
            // Recreate label
            this.CreateLabel();
        }

        // Methods creates the gameobjects and components required to display labels
        private void CreateLabel()
        {
            // Create label game object
            this.labelObject = new GameObject("Label");
            this.labelObject.transform.parent = this.transform;
            this.labelObject.transform.localPosition = Vector3.zero;
            this.labelObject.transform.localRotation = Quaternion.identity;
            this.labelObject.transform.localScale = Vector3.one;
            this.labelObject.SetActive(false);

            // Create label offset object
            this.labelOffset = new GameObject("Offset");
            this.labelOffset.transform.parent = this.labelObject.transform;
            this.labelOffset.transform.localPosition = Vector3.zero;
            this.labelOffset.transform.localRotation = Quaternion.identity;
            this.labelOffset.transform.localScale = Vector3.one * 0.1f;

            // Create object containing the text
            GameObject textObject = new GameObject("Text");
            textObject.transform.parent = this.labelOffset.transform;
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
            textObject.transform.localScale = Vector3.one * 0.02f;

            // Create text mesh
            this.textMesh = textObject.AddComponent<TextMesh>();
            this.textMesh.text = this.label;
            this.textMesh.offsetZ = -0.6f;
            this.textMesh.anchor = TextAnchor.MiddleCenter;
            this.textMesh.alignment = TextAlignment.Center;
            this.textMesh.font = Resources.Load("Fonts/NanumGothic-ExtraBold") as Font;
            this.textMesh.fontSize = 50;
            this.textMesh.fontStyle = FontStyle.Bold;
            this.textMesh.color = new Color(1, 1, 1, 0.8f);

            MeshRenderer meshRenderer = textObject.GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.material = Resources.Load("Materials/Font/FontForeground") as Material;

            // Create object for label background
            GameObject backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backgroundObject.name = "Background";
            backgroundObject.transform.parent = this.labelOffset.transform;
            backgroundObject.transform.localPosition = Vector3.zero;
            backgroundObject.transform.localRotation = Quaternion.identity;
            float width = Utilities.GetWidth(this.textMesh);
            backgroundObject.transform.localScale = new Vector3((width * 10f) + 0.1f, 0.13f, 0.02f);

            // Destroy module box collider
            Destroy(backgroundObject.GetComponent<BoxCollider>());

            meshRenderer = backgroundObject.GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.material = Resources.Load("Materials/Font/FontBackground") as Material;

            // Set correct layer
            Utilities.SetLayerRecursively(this.labelObject, this.interactable.GetSpace());
        }

        // Method to destroy all gameobjects created to display the label
        private void DestroyLabel()
        {
            if(this.labelObject != null)
                DestroyImmediate(this.labelObject);
        }

        // Method to set if the label is currently active and should be visible or not
        private void SetLabelActive(bool active)
        {
            if(active == this.labelObject.activeSelf)
                return;

            this.labelObject.SetActive(active);
        }
    }
}