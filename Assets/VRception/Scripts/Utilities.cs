using System;
using UnityEngine;
using Photon.Pun;

namespace VRception
{
    /// <summary>
    /// This class offers some static methods for frequently reoccuring tasks
    /// </summary>
    public class Utilities : MonoBehaviour
    {
        // Method creates a clone of the specified gameobject
        public static GameObject CreateClone(GameObject obj, Vector3 position, Vector3 rotation)
        {
            // Ensure game object has a cloneable component attached
            Cloneable cloneable = obj.GetComponent<Cloneable>();

            if(cloneable == null || String.IsNullOrEmpty(cloneable.prefabName))
            {
                Debug.LogError("Game object has no cloneable component referencing their prefab.");
                return null;
            }

            // Define the space to spawn the object in
            Space cloneSpace = Settings.instance.GetCurrentSpace();

            // Send events for before copy
            Utilities.OnBeforeCopy(obj);

            // Additional information to process instantiate on remote clients
            object[] info = { cloneSpace, true };

            GameObject clone = PhotonNetwork.Instantiate(cloneable.prefabName, Vector3.zero, Quaternion.identity, 0, info);
            ControllerSpaces.instance.MoveGameObjectToSpace(clone, cloneSpace);
            clone.transform.position = position;
            clone.transform.rotation = Quaternion.Euler(rotation);
            clone.transform.localScale = Vector3.one;

            Interactable interactable = clone.GetComponent<Interactable>();

            // Destroy module interface if exists
            DestroyImmediate(clone.GetComponent<ModuleInterface>());

            // Destroy module parent if exists
            DestroyImmediate(clone.GetComponent<ModuleParent>());

            // Store the space in which the prefab is spawned
            interactable.SetSpace(cloneSpace);

            // Set scale of model to correct size
            Interactable templateInteractable = obj.GetComponent<Interactable>();
            if(templateInteractable != null)
                interactable.modelGameObject.transform.localScale = templateInteractable.modelGameObject.transform.localScale;

            // Set correct layer of clone
            Utilities.SetLayerRecursively(clone, cloneSpace);

            // Send events for after copy
            Utilities.OnAfterCopy(obj);

            // Register object for Logging
            //ControllerLogger.instance.RegisterObjectForLogging(clone);

            // Return clone
            return clone;
        }

        // Method helps sending the OnBeforeCopy event
        public static void OnBeforeCopy(GameObject obj)
        {
            IEventCopy[] modules = obj.GetComponentsInChildren<IEventCopy>();
            foreach (IEventCopy module in modules)
                module.OnBeforeCopy();
        }

        // Method helps sending the OnAfterCopy event
        public static void OnAfterCopy(GameObject obj)
        {
            IEventCopy[] modules = obj.GetComponentsInChildren<IEventCopy>();
            foreach (IEventCopy module in modules)
                module.OnAfterCopy();
        }

        // Method returns the culling mask for the specified space
        public static int GetCullingMaskOfSpace(Space space)
        {
            switch(space)
            {
                case Space.SHARED:
                    return 
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerLeft.layerIndex) | 
                        (1 << Settings.instance.layerRight.layerIndex);
                case Space.LEFT:
                    return
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerLeft.layerIndex);
                case Space.RIGHT:
                    return
                        (1 << Settings.instance.layerShared.layerIndex) | 
                        (1 << Settings.instance.layerRight.layerIndex);
                default:
                    return 0; // culling mask to nothing
            }
        }

        // Returns space for the specified layer
        public static Space GetSpaceByLayer(int layer)
        {
            if (layer == Settings.instance.layerShared.layerIndex)
                return Space.SHARED;
            if (layer == Settings.instance.layerLeft.layerIndex)
                return Space.LEFT;
            if (layer == Settings.instance.layerRight.layerIndex)
                return Space.RIGHT;
            return Space.DEFAULT;
        }

        // Returns the layer for the specified space
        public static int GetLayerOfSpace(Space space)
        {
            return LayerMask.NameToLayer(Utilities.GetLayerNameOfSpace(space));
        }

        // Returns the name of the layer for the specified space
        public static string GetLayerNameOfSpace(Space space)
        {
            switch (space)
            {
                case Space.SHARED:
                    return Settings.instance.layerShared.ToString();
                case Space.LEFT:
                    return Settings.instance.layerLeft.ToString();
                case Space.RIGHT:
                    return Settings.instance.layerRight.ToString();
                default:
                    return Settings.instance.layerDefault.ToString();
            }
        }

        // Sets the specified layer to the gameobject, its children, and deep children recursively
        public static void SetLayerRecursively(GameObject obj, Space space)
        {
            // Get name of layer
            string name = Utilities.GetLayerNameOfSpace(space);

            // Set layer for current game object
            obj.layer = LayerMask.NameToLayer(name);

            // Set layer for all children objects
            foreach (Transform child in obj.transform)
                Utilities.SetLayerRecursively(child.gameObject, space);
        }

        // Method helps finding a gameobject by name (not very performant; be careful)
        public static GameObject FindGameObjectByName(GameObject obj, string name)
        {
            if (obj.name.Equals(name))
                return obj;

            foreach (Transform transform in obj.transform)
            {
                GameObject result = Utilities.FindGameObjectByName(transform.gameObject, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        // Method returns the closest interactable in all parent gameobjects
        public static Interactable FindFirstInteractableInGameObjectAndParents(GameObject obj)
        {
            Interactable interactable = obj.GetComponent<Interactable>();
            if (interactable != null)
                return interactable;

            if (obj.transform.parent != null)
                return Utilities.FindFirstInteractableInGameObjectAndParents(obj.transform.parent.gameObject);

            return null;
        }

        // Method sets the alpha value of a camera
        public static void SetAlpha(GameObject camera, float value)
        {
            CameraAlpha cameraAlpha = camera.GetComponent<CameraAlpha>();
            if (cameraAlpha != null)
                cameraAlpha.alpha = value;
        }

        // Method calculates the width of a TextMesh
        public static float GetWidth(TextMesh mesh)
        {
            float width = 0;
            foreach(char symbol in mesh.text)
            {
                CharacterInfo info;
                mesh.font.RequestCharactersInTexture(mesh.text, mesh.fontSize, mesh.fontStyle);
                if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize))
                    width += info.advance;
            }
            return width * mesh.characterSize * 0.1f * mesh.transform.lossyScale.x;
        }

        // Return all children of gameobject as array
        public static Transform[] FindChildrens(Transform parent)
        {
            // Declaring an array the size of the number of children
            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                // Populating the array
                children[i] = parent.GetChild(i);
            }
            // returning the array. If there is no children it will be an empty array instead of null.
            return children;
        }

        // Method that allows to terminate the VRception toolkit
        public static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
