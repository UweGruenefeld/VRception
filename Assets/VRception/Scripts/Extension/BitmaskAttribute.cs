using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Attribute that stores a bit mask. Used for the 'Experience Settings' to store the currently enabled experiences.
    /// </summary>
    public class BitmaskAttribute : PropertyAttribute
    {
        public System.Type propType;

        // Constructor of the Bitmask Attribute
        public BitmaskAttribute(System.Type attributeType)
        {
            propType = attributeType;
        }
    }
}