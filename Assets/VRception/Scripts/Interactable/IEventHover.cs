using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The interface IEventHover can be implemented by modules of interactables to listen to hover events
    /// </summary>
    public interface IEventHover
    {
        // Event interactable has been hovered
        void OnHoverEnter(bool isChild);

        // Event interactable has been unhovered
        void OnHoverExit(bool isChild);

        // Returns the gameobject of this interactable
        GameObject GetGameObject();
    }
}