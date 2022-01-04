namespace VRception
{
    /// <summary>
    /// The interface IEventCopy can be implemented by modules of interactables to listen to copy events
    /// </summary>
    public interface IEventCopy
    {
        // Event interactable is about to be copied
        void OnBeforeCopy();

        // Event interactable has been copied
        void OnAfterCopy();
    }
}