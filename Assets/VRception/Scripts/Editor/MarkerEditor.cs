using UnityEngine;
using UnityEditor;

namespace VRception
{
    /// <summary>
    /// This class implements the GUI of the script "Experience > Marker" visible in the inspector.
    /// </summary>
    [CustomEditor(typeof(Marker))]
    public class MarkerEditor : Editor
    {
        SerializedProperty customPrefab;

        // This function is called when the object becomes enabled and active.
        void OnEnable()
        {
            this.customPrefab = serializedObject.FindProperty("customPrefab");
        }

        // This function is called for rendering and handling the inspector GUI
        public override void OnInspectorGUI()
        {
            Marker marker = (Marker)target;

            ControllerExperiences controllerExperiences = marker.transform.parent.GetComponent<ControllerExperiences>();

            // Is the component not correctly attached as a child component?
            if(controllerExperiences == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("This component is required to be a direct child of the game object with the ControllerExperiences script.", MessageType.Error);
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

                // Marker settings
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Marker Settings", EditorStyles.boldLabel);

                // Display help text
                bool lastEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea("Markers can be placed anywhere (they exist across spaces) and can be assigned one specific experience below. The assigned experience can be changed in-game by the player. Furthermore, marker can be represented by the default prefab specified in 'Experience Settings' or be represented by a custom prefab that can be specified below. In the latter case, player can, for example, use marker to represent bystander to actively integrate them in the prototyping process.");
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                GUI.enabled = lastEnabled;

                // Get selected experiences
                Experience[] experiences = controllerExperiences.GetSelectedExperiences().ToArray();
                
                // Create list with selected experiences
                int selected = 0;
                string[] values = new string[experiences.Length];
                for(int i = 0; i < experiences.Length; i++)
                {
                    values[i] = experiences[i].ToString();
                    if(experiences[i] == marker.initialExperience)
                        selected = i;
                }

                // Show experience selection
                selected = EditorGUILayout.Popup("Initial Experience", selected, values);

                if(selected >= experiences.Length)
                    marker.initialExperience = 0;
                else
                    marker.initialExperience = experiences[selected];

                // Show toggle to switch to custom marker prefab
                marker.useCustomPrefab = EditorGUILayout.Toggle(
                    new GUIContent("Use Prefab Below", "If true, then the prefab specified below is used, otherwise the default marker prefab is used."), marker.useCustomPrefab);

                // Property for custom prefab
                EditorGUILayout.PropertyField(customPrefab, new GUIContent("Custom Prefab", "Specify the custom prefab to use for this marker (requires checkbox 'Use Prefab Below' to be toggled)."));

                // Transform Settings
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Transform Settings", EditorStyles.boldLabel);

                // Display help text
                lastEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea("Markers can be positioned using the transform component of the gameobject the 'Marker' script is attached to or by the attributes below. With the checkbox below one can decide which one is used.");
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                GUI.enabled = lastEnabled;

                marker.useTransformComponent = EditorGUILayout.Toggle(
                    new GUIContent("Use Transform Component", "If true, then the transform of this game object is used to place the marker."), marker.useTransformComponent);
                marker.position = EditorGUILayout.Vector3Field(
                    new GUIContent("Position", "If useTransformComponent is false, this position x- and z-value are used for the marker."), marker.position);
                marker.rotation = EditorGUILayout.FloatField(
                    new GUIContent("Rotation", "If useTransformComponent is false, this rotation around the y-axis is used for the marker."), marker.rotation);

                EditorGUILayout.Space();

                EditorGUI.EndDisabledGroup();
            }

    	    // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
    }
}