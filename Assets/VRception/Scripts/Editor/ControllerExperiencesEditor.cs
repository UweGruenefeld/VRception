using UnityEngine;
using UnityEditor;

namespace VRception
{
    /// <summary>
    /// This editor class ensures that the inspector of the 'Experience Settings' gameobject is locked when the Unity Editor is playing.
    /// </summary>
    [CustomEditor(typeof(ControllerExperiences))]
    public class ControllerExperiencesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            base.OnInspectorGUI();
            
            EditorGUI.EndDisabledGroup();
        }
    }
}