using UnityEditor;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This drawer transforms the 'Helpbox' attribute annotation into an actual helpbox rendered in the Unity inspector GUI.
    /// </summary>
    [CustomPropertyDrawer(typeof(HelpboxAttribute))]
    public class HelpboxDrawer : DecoratorDrawer
    {
        HelpboxAttribute helpAttribute 
        { 
            get { return (HelpboxAttribute)attribute; } 
        }

        private float height;

        // Method returns the height of this drawer
        public override float GetHeight()
        {
            return height;
        }

        // OnGUI is called for rendering and handling GUI events
        public override void OnGUI(Rect rect)
        {
            // Ignore method if rect is too tiny
            if(rect.width <= 1)
                return;

            // Calculate the space needed for the help text
            GUIContent content = new GUIContent(helpAttribute.text);
            GUIStyle style = GUI.skin.label;
            Vector2 size = style.CalcSize(content);
            var rectHelpText = new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing * 2, rect.width, size.y * ((int)((size.x * 1.03f) / rect.width) + 1));
            this.height = rectHelpText.height + (EditorGUIUtility.standardVerticalSpacing * 5);

            GUI.enabled = false;

            // Display help text
            EditorStyles.textField.wordWrap = true;
            EditorGUI.TextArea(rectHelpText, helpAttribute.text);

            GUI.enabled = true;
        }
    }
}