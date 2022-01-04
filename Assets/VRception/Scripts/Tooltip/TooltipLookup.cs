using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Component determines which position the tooltip should be at
    /// </summary>
    public class TooltipLookup : MonoBehaviour
    {
        //// SECTION "Lookup Settings"
        [Header("Lookup Settings", order = 0)]
        [Helpbox("Component determines which position the tooltip should be at.", order = 1)]
        [Tooltip("Specify the type of anchor used for the lookup.", order = 2)]
        public TooltipAnchor anchor = TooltipAnchor.DEFAULT;

        [Tooltip("Reference the responsible tooltip controller.")]
        public ControllerTooltips controller = null;

        // This function is called when the object becomes enabled and active
        void OnEnable()
        {
            // Get anchor location
            Vector3 position = controller.GetPositionOfAnchor(this.anchor);
            
            // Update line renderer with correct anchor location
            LineRenderer lineRenderer = this.GetComponent<LineRenderer>();
            Vector3[] positions = new Vector3[lineRenderer.positionCount];;
            lineRenderer.GetPositions(positions);
            if(positions.Length >= 2)
                positions[1] = position;
            lineRenderer.SetPositions(positions);
        }
    }
}