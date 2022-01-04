using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Class that stores a layer. Used in 'Settings' to store the layer selection
    /// </summary>
    [System.Serializable]
    public class LayerIndex
    {
        public int layerIndex;

        // Returns the stored layer as string
        public override string ToString()
        {
            return LayerMask.LayerToName(this.layerIndex);
        }
    }
}