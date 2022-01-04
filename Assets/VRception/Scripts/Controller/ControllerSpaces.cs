using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRception
{
    /// <summary>
    /// This class bundles all functionality related to spaces. The VRception toolkit supports three different spaces (shared, left, and right).
    /// By default, the spaces represent reality (left space) and virtuality (right space) -- as proposed by the reality-virtuality continuum from Milgram and Kishino.
    /// </summary>
    public class ControllerSpaces : MonoBehaviour
    {
        [HideInInspector]
        public static ControllerSpaces instance;

        //// SECTION "Skybox Settings"
        [Header("Skybox Settings", order = 0)]
        [Helpbox("The VRception toolkit supports three different spaces (shared, left, and right). Here, the left and right space are only (partially) visible depending on the slider position and by default represent reality (left) and virtuality (right) -- as proposed by the reality-virtuality continuum from Milgram and Kishino. For both, the left and right space, one can specify a skybox that is used for rendering the spaces. If the crossfader is in the center or more towards the left, then the left skybox is rendered and otherwise the right skybox is rendered.", order = 1)]
        [Tooltip("Skybox for left space.", order = 2)]
        public Material skyboxLeft = null;

        [Tooltip("Skybox for right space.")]
        public Material skyboxRight = null;

        //// SECTION "Shared Space"
        [Header("Shared Space", order = 0)]
        [Helpbox("The shared space consists of scenes that are always visible and are not affected by the crossfader. One can specify the main shared scene (root scene) and add aditional scenes as shared scenes below. The root scene should not also be listed as an additional scene. To get the identifier for a scene, click on 'File > Build Settings' and then, add the scene to the 'Scenes in Build' list. If the scene is in the list, the identifying number is shown next to the name of the scene in the list.", order = 1)]
        [Tooltip("Index of root shared scene in build settings.", order = 2)]
        public int sharedRootScene = -1;

        [Tooltip("Index of additional shared scenes (root excluded) in build settings.")]
        public int[] sharedScenes;

        //// SECTION "Left Space"
        [Header("Left Space", order = 0)]
        [Helpbox("The left space consists of scenes that are positioned on the left side of the crossfader. By default, the left space represents reality. One can specify the main left scene (root scene) and add aditional scenes as left scenes below. The root scene should not also be listed as an additional scene. If the left space represent something else than reality, one can adjust the name below (e.g., to allow transitioning between two different virtualities).", order = 1)]
        [Tooltip("Name of left spaces in the simulation.", order = 2)]
        public string leftName = "Reality";

        [Tooltip("Index of root left scene in build settings.")]
        public int leftRootScene = -1;

        [Tooltip("Index of additional left scenes (root excluded) in build settings.")]
        public int[] leftScenes;

        //// SECTION "Right Space"
        [Header("Right Space", order = 0)]
        [Helpbox("The right space consists of scenes that are positioned on the right side of the crossfader. By default, the right space represents virtuality. One can specify the main right scene (root scene) and add aditional scenes as right scenes below. The root scene should not also be listed as an additional scene. If the right space represent something else than virtuality, one can adjust the name below.", order = 1)]
        [Tooltip("Name of right spaces in the simulation.", order = 2)]
        public string rightName = "Virtuality";

        [Tooltip("Index of root right scene in build settings.")]
        public int rightRootScene = -1;

        [Tooltip("Index of additional right scenes (root excluded) in build settings.")]
        public int[] rightScenes;

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Singleton reference
            ControllerSpaces.instance = this;

            // Register callback for scene loaded
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Storage for loaded scenes
            List<int> loadedScenes = new List<int>();

            // Check which scenes are already loaded and adjust the layer
            for(int i=0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if(this.IsSceneDefined(scene.buildIndex, Space.SHARED))
                {
                    this.SetLayerForScene(scene, Space.SHARED);
                    loadedScenes.Add(scene.buildIndex);
                }
                else if(this.IsSceneDefined(scene.buildIndex, Space.LEFT))
                {
                    this.SetLayerForScene(scene, Space.LEFT);
                    loadedScenes.Add(scene.buildIndex);
                }
                else if(this.IsSceneDefined(scene.buildIndex, Space.RIGHT))
                {
                    this.SetLayerForScene(scene, Space.RIGHT);
                    loadedScenes.Add(scene.buildIndex);
                }
            }

            // Check for all defined scenes, if they are loaded and load them if they were not
            foreach(int index in this.GetScenesDefined())
                if(!loadedScenes.Contains(index))
                    SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        }

        // Method is called when scene is loaded
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(this.IsSceneDefined(scene.buildIndex, Space.SHARED))
                this.SetLayerForScene(scene, Space.SHARED);
            else if(this.IsSceneDefined(scene.buildIndex, Space.LEFT))
                this.SetLayerForScene(scene, Space.LEFT);
            else if(this.IsSceneDefined(scene.buildIndex, Space.RIGHT))
                this.SetLayerForScene(scene, Space.RIGHT);
        }

        // Method returns the root gameobjects of a space
        public GameObject[] GetRootGameObjects(Space space)
        {
            // Store the relevant scenes
            int[] indexScenes = this.GetScenesDefined(space);

            // Store the relevant game objects
            List<GameObject> rootGameObjects = new List<GameObject>();

            // Look for loaded scenes and extract the root game objects
            foreach(int index in indexScenes)
            {
                // Load scene for index
                Scene sceneFromIndex = SceneManager.GetSceneByBuildIndex(index);

                // Check if scene is currently loaded
                if(sceneFromIndex.isLoaded)
                    rootGameObjects.AddRange(sceneFromIndex.GetRootGameObjects());
            }

            // Return the relevant game objects as an arry
            return rootGameObjects.ToArray();
        }

        // Method returns the space to a given gameobject
        public Space GetSpaceOfGameObject(GameObject obj)
        {
            // Get scene from game object
            Scene scene = obj.scene;

            if(this.IsSceneDefined(scene.buildIndex, Space.SHARED))
                return Space.SHARED;
            else if(this.IsSceneDefined(scene.buildIndex, Space.LEFT))
                return Space.LEFT;
            else if(this.IsSceneDefined(scene.buildIndex, Space.RIGHT))
                return Space.RIGHT;

            return Space.DEFAULT;
        }

        // Method moves a gameobject to the specified space
        public bool MoveGameObjectToSpace(GameObject obj, Space space)
        {
            // To move the object to another scene requires it to be a root game object
            obj.transform.parent = null;

            // Move object to root scene of space
            switch(space)
            {
                case Space.SHARED:
                    if(this.sharedRootScene != -1)
                    {
                        //Utilities.SetLayerRecursively(obj, space);
                        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByBuildIndex(this.sharedRootScene));
                        return true;
                    }
                    break;
                case Space.LEFT:
                    if(this.leftRootScene != -1)
                    {
                        //Utilities.SetLayerRecursively(obj, space);
                        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByBuildIndex(this.leftRootScene));
                        return true;
                    }
                    break;
                case Space.RIGHT:
                    if(this.rightRootScene != -1)
                    {
                        //Utilities.SetLayerRecursively(obj, space);
                        SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByBuildIndex(this.rightRootScene));
                        return true;
                    }
                    break;
                default:
                    //Utilities.SetLayerRecursively(obj, space);
                    SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
                    return true;
            }

            // Not possible to move object to space because space has no scene
            return false;
        }

        // Method returns true if the scene with the specified ID exists
        private bool IsSceneDefined(int buildIndex, Space space)
        {
            // Check if build index is defined for the specified type of scene
            switch(space)
            {
                case Space.SHARED:
                    if(buildIndex == this.sharedRootScene)
                        return true;
                    foreach(int index in this.sharedScenes)
                        if(buildIndex == index)
                            return true;
                    break;
                case Space.LEFT:
                    if(buildIndex == this.leftRootScene)
                        return true;
                    foreach(int index in this.leftScenes)
                        if(buildIndex == index)
                            return true;
                    break;
                case Space.RIGHT:
                    if(buildIndex == this.rightRootScene)
                        return true;
                    foreach(int index in this.rightScenes)
                        if(buildIndex == index)
                            return true;
                    break;
            }

            // Scene is not defined
            return false;
        }

        // Method returns a list of all scene IDs of a certain space
        private int[] GetScenesDefined(Space space)
        {
            // Storage for defined scenes
            List<int> definedScenes = new List<int>();
            
            switch(space)
            {
                case Space.SHARED:
                    if(this.sharedRootScene != -1)
                        definedScenes.Add(this.sharedRootScene);
                    foreach(int index in this.sharedScenes)
                        definedScenes.Add(index);
                    break;
                case Space.LEFT:
                    if(this.leftRootScene != -1)
                        definedScenes.Add(this.leftRootScene);
                    foreach(int index in this.leftScenes)
                        definedScenes.Add(index);
                    break;
                case Space.RIGHT:
                    if(this.rightRootScene != -1)
                        definedScenes.Add(this.rightRootScene);
                    foreach(int index in this.rightScenes)
                        definedScenes.Add(index);
                    break;
            }

            return definedScenes.ToArray();
        }

        // Method returns a list of all scene
        private int[] GetScenesDefined()
        {
            // Storage for defined scenes
            List<int> definedScenes = new List<int>();

            if(this.sharedRootScene != -1)
                definedScenes.Add(this.sharedRootScene);
            if(this.leftRootScene != -1)
                definedScenes.Add(this.leftRootScene);
            if(this.rightRootScene != -1)
                definedScenes.Add(this.rightRootScene);

            foreach(int index in this.sharedScenes)
                definedScenes.Add(index);
            foreach(int index in this.leftScenes)
                definedScenes.Add(index);
            foreach(int index in this.rightScenes)
                definedScenes.Add(index);

            return definedScenes.ToArray();
        }

        // Method sets the layer for a scene
        private void SetLayerForScene(Scene scene, Space space)
        {
            // If no scene is specified, abort
            if(scene == null)
                return;

            GameObject[] objects = this.GetRootGameObjects(space);
            foreach(GameObject obj in objects)
                Utilities.SetLayerRecursively(obj, space);
        }
    }
}