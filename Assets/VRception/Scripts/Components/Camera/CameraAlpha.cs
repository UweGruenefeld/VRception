using UnityEngine;

namespace VRception
{
    /// <summary>
    /// This script can be attached to a camera to render objects with transparency.
    /// It is used for the crossfader functionality to alpha-blend different spaces.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraAlpha : MonoBehaviour
    {
        //// SECTION "Alpha Settings"
        [Header("Alpha Settings", order = 0)]
        [Helpbox("This script can be attached to a camera to render objects with transparency. Basically, this script sets the below specified alpha value to all gameobjects of the specified space before it renders and then, sets the alpha values back after it is done rendering. It is used for the crossfader functionality to alpha-blend different spaces. Each of the two spaces (left, right) has their own camera with this script attached that allows to apply alpha-blending to all objects of its space.", order = 1)]
        [Tooltip("Specify the space the alpha-blending is applied to.", order = 2)]
        public Space space = Space.DEFAULT;

        [Tooltip("Specify the alpha value used.")]
        public float alpha = 1;

        // Method gets called before the attached camera component renders a frame
        void OnPreRender()
        {
            // If we are in experience mode, let the experience handle the rendering
            if(Settings.instance.mode == Mode.EXPERIENCE)
            {
                IExperience experience = ControllerExperiences.instance.GetCurrentExperience();
                if(experience != null)
                    experience.OnRender(this.GetComponent<Camera>(), this.space);
                return;
            }

            // We are not in experience mode, apply alpha from crossfader value
            CameraAlpha.SetAlphaToSpace(this.space, this.alpha);            
        }

        // Method gets called after the attached camera component rendered a frame
        void OnPostRender()
        {
            CameraAlpha.SetAlphaToSpace(this.space, 1);
        }

        // Method allows to set an alpha value for a specified space
        public static void SetAlphaToSpace(Space space, float value)
        {
            // Get all game objects from specified space
            GameObject[] objects = ControllerSpaces.instance.GetRootGameObjects(space);
            foreach(GameObject obj in objects)
                CameraAlpha.SetAlphaToGameObjectAndChildren(obj, value);
        }

        // Method allows to recursively set the alpha value to the gameobject, childs, and deep childs.
        public static void SetAlphaToGameObjectAndChildren(GameObject obj, float value)
        {
            // Set alpha value to this game object
            CameraAlpha.SetAlphaToGameObject(obj, value);

            // Set alpha to children of this game object
            foreach (Transform child in obj.transform)
                CameraAlpha.SetAlphaToGameObjectAndChildren(child.gameObject, value);
        }

        // Method that applies the alpha value to one specific gameobject
        public static void SetAlphaToGameObject(GameObject obj, float value)
        {
            // Get all renderer of the gameobject
            Renderer[] allRenderer = obj.GetComponents<Renderer>();

            // Iterate through all renderer
            foreach(Renderer r in allRenderer)
            {
                // Set alpha value for all materials
                for(int i = 0; i < r.materials.Length; i++)
                {
                    Color color = r.material.color;
                    color.a = value;
                    r.materials[i].color = color;
                }
            }
        }
    }
}