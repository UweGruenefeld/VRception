using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The interface IEventSelect can be implemented by modules of interactables to listen to select events
    /// </summary>
    public interface IEventSelect
    {
        // Event interactable has been selected
        void OnSelectEnter(bool isChild);

        // Event interactable has been unselected
        void OnSelectExit(bool isChild);

        // Returns the gameobject of this interactable
        GameObject GetGameObject();
    }
}