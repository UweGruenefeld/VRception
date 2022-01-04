using UnityEditor;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This drawer class uses layer attributes (with a valid layer id) and shows a dropdown menu for them in the inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(LayerIndex))]
    public class LayerIndexDrawer : PropertyDrawer
    {
        // OnGUI is called for rendering and handling GUI events
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            SerializedProperty layerIndex = property.FindPropertyRelative("layerIndex");
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            layerIndex.intValue = EditorGUI.LayerField(position, layerIndex.intValue);
            EditorGUI.EndProperty();
        }
    }
}