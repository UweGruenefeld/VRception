using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Simple implementation of the Reality 'manifestation' as an experience
    /// </summary>
    public class ExperienceReality : IExperience
    {
        // Constructor of the experience
        public ExperienceReality() : base("Reality")
        {

        }

        // Method is called when the player enters this experience
        public override void OnEnter()
        {
            // Specify position on crossfader to have the correct camera order
            Settings.instance.crossfader = -1f;
        }

        // While this experience is active, this method is called in every frame before a camera renders the frame
        public override void OnRender(Camera camera, Space space)
        {
            // Apply alpha to correct game objects
            switch(space)
            {
                case Space.RIGHT:
                    CameraAlpha.SetAlphaToSpace(space, 0);
                    break;
            }
        }
    }
}