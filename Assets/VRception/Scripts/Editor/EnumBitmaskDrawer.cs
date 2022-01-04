using UnityEngine;
using UnityEditor;

namespace VRception
{
    /// <summary>
    /// This drawer class uses one-hot encoded attributes and show a dropdown menu for them in the inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(BitmaskAttribute))]
    public class EnumBitmaskDrawer : PropertyDrawer
    {
        // OnGUI is called for rendering and handling GUI events.
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var typeAttr = attribute as BitmaskAttribute;

            // Add the actual int value behind the field name
            label.text = label.text + " (" + prop.intValue + ")";
            prop.intValue = EnumBitmaskDrawer.DrawBitMaskField(position, prop.intValue, typeAttr.propType, label);
        }

        // Method creates the custom created bitmask field
        public static int DrawBitMaskField(Rect position, int mask, System.Type type, GUIContent label)
        {
            // Get item names and values
            var itemNames = System.Enum.GetNames(type);
            var itemValues = System.Enum.GetValues(type) as int[];

            int value = mask;
            int maskValue = 0;

            for (int i = 0; i < itemValues.Length; i++)
            {
                if (itemValues[i] != 0)
                {
                    if ((value & itemValues[i]) == itemValues[i])
                        maskValue |= 1 << i;
                }
                else if (value == 0)
                    maskValue |= 1 << i;
            }
            
            int newMaskValue = EditorGUI.MaskField(position, label, maskValue, itemNames);
            int changes = maskValue ^ newMaskValue;

            for (int i = 0; i < itemValues.Length; i++)
            {
                // Has this list item changed?
                if ((changes & (1 << i)) != 0)            
                {
                    // Has it been set?
                    if ((newMaskValue & (1 << i)) != 0)     
                    {
                        // Special case: if "0" is set, just set the value to 0
                        if (itemValues[i] == 0)           
                        {
                            value = 0;
                            break;
                        }
                        else
                            value |= itemValues[i];
                    }
                    // It has been reset
                    else                                  
                    {
                        value &= ~itemValues[i];
                    }
                }
            }
            return value;
        }
    }
}

