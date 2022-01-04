using System.Collections;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The highlight module provides feedback on the internal state of an interactable to players.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleHighlight : MonoBehaviour, IEventHover, IEventSelect, IEventCopy
    {
        //// SECTION "Highlight Settings"
        [Header("Highlight Settings", order = 0)]
        [Helpbox("The highlight module provides feedback on the internal state of an interactable to players. It highlights interactables that are hovered or selected directly or for which a parent interactable exists that is hovered or selected (e.g., when one interactable is attached to another one and the parent interactable is selected by the player). Moreover, as interactables can be attached to each other, it highlights interactables that are currently selected as parent interactable.", order = 1)]
        [Tooltip("Color used to communicate a hovered interactable to the player.", order = 2)]
        public Color colorHovered = new Color(.5f, 1, .5f);

        [Tooltip("Color used to communicate that a parent interactable is hovered interactables.")]
        public Color colorHoveredChild = new Color(.7f, .7f, 1);

        [Tooltip("Color used to communicate a selected interactable to the player.")]
        public Color colorSelected = new Color(0, 1, 0);

        [Tooltip("Color used to communicate that a parent interactable is selected.")]
        public Color colorSelectedChild = new Color(.5f, .5f, 1);

        [Tooltip("Color used to communicate that this interactable is currently considered a parent interactable.")]
        public Color colorSelectedParent = new Color(.5f, .5f, 1);

/*
        [Tooltip("If enabled, controller is rendered invisible when object is selected.")]
        public bool hideControllerOnSelect = true;
*/

        private Interactable interactable = null;

        private bool isHighlighted;

        // Store original attributes
        private Color originalColor = Color.black;
        private Color copyColor = Color.black;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();

            this.isHighlighted = false;

            // Store the original color of the meshrenderer from the highlighting object
            this.originalColor = this.GetColor();
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if(this.isHighlighted || this.interactable.IsPrefabInInterface() || this.interactable.ignoreAsParent)
                return;

            // Set color to original - will potentially be overwritten below
            this.SetColor(this.originalColor);

            // Get current colliding objects
            GameObject[] collidingObjects = this.interactable.modelCollisionDetector.GetCurrentCollisions();

            foreach(GameObject obj in collidingObjects)
            {
                Interactable interactable = obj.GetComponentInParent<Interactable>();
                
                if(interactable == null || !interactable.changeParent)
                    continue;

                if(interactable.IsSelected() || interactable.IsSelectedAsChild())
                    this.SetColor(Color.Lerp(this.colorSelectedParent, this.originalColor, Mathf.PingPong(Time.time, 1)));
            }
        }

        // Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy.
        void OnDestroy()
        {
            //this.SetControllerVisible(true);
        }

        // Event method that is invoked when this interactable is hovered
        public void OnHoverEnter(bool isChild)
        {
            this.UpdateHighlighting();
        }

        // Event method that is invoked when this interactable is not hovered anymore
        public void OnHoverExit(bool isChild)
        {
            // Stop vibration
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            
            this.UpdateHighlighting();
        }

        // Event method that is invoked when this interactable is selected
        public void OnSelectEnter(bool isChild)
        {
            // Start short vibration of controller
            OVRInput.SetControllerVibration(1, 0.6f, OVRInput.Controller.RTouch);
            StartCoroutine(DeactivateVibration(0.2f));

            //this.SetControllerVisible(false);

            this.UpdateHighlighting();
        }

        // Event method that is invoked when this interactable is not selected anymore
        public void OnSelectExit(bool isChild)
        {
            // Stop vibration
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);

            //this.SetControllerVisible(true);

            this.UpdateHighlighting();
        }

        // Method gets invoked before the interactable is copied
        public void OnBeforeCopy()
        {
            this.copyColor = this.GetColor();
            this.SetColor(this.originalColor);
        }

        // Method gets invoked after the interactable is copied
        public void OnAfterCopy()
        {
            this.SetColor(this.copyColor);
            this.copyColor = Color.black;
        }

        // Internal method that updates the highlighting of the interactable
        private void UpdateHighlighting()
        {
            this.isHighlighted = true;

            if(this.interactable.IsSelected())
                this.SetColor(this.colorSelected);
            else if(this.interactable.IsSelectedAsChild())
                this.SetColor(this.colorSelectedChild);
            else if(this.interactable.IsHovered())
                this.SetColor(this.colorHovered);
            else if(this.interactable.IsHoveredAsChild())
                this.SetColor(this.colorHoveredChild);
            else
            {
                this.SetColor(this.originalColor);
                this.isHighlighted = false;
            }
        }

        // Internal method that gets the color currently assigned to the renderer of the interactable
        private Color GetColor()
        {
            if(this.interactable.modelRenderer != null)
                return this.interactable.modelRenderer.material.color;

            return Color.black;
        }

        // Internal method that sets the color of the interactable's renderer
        private void SetColor(Color color)
        {
            if(this.interactable.modelRenderer != null)
                this.interactable.modelRenderer.material.color = color;
        }

/*
        // Internal method that sets the visibility of the player controllers
        private void SetControllerVisible(bool visible)
        {
            GameObject controller = Settings.instance.rightController;
            if(controller != null)
                controller.GetComponent<MeshRenderer>().enabled = visible;
        }
*/

        // Corountine that disables the controller vibration after a period of time
        private IEnumerator DeactivateVibration(float seconds)
        {
            yield return new WaitForSeconds (seconds);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            yield return null;
        }

        // Returns the gameobject of this interactable
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}