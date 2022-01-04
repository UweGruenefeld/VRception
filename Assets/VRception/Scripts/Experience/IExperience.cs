using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Abstract class that every custom experience needs to implement
    /// </summary>
    public abstract class IExperience
    {
        private string name;
        private float lastCrossfader;
 
        // Constructor of the abstract class
        public IExperience(string name)
        {
            this.name = name;
            this.lastCrossfader = 0;
        }

        // Method returns the name of the experience as string
        public override string ToString()
        {
            return name;
        }

        // Method is called when the experience is entered
        public void OnExperienceEnter()
        {
            this.lastCrossfader = Settings.instance.crossfader;
            this.OnEnter();
        }

        // Method is called when the experience is left
        public void OnExperienceLeave()
        {
            Settings.instance.crossfader = this.lastCrossfader;
            this.OnLeave();
        }

        // This method can be overwritten and is called as soon as a player enters this experience
        public virtual void OnEnter()
        {

        }

        // This method can be overwritten and is called as soon as a player leaves this experience
        public virtual void OnLeave()
        {

        }

        // While this experience is active, this method is called in every frame before a camera renders the frame (for each camera; every space has its own camera; so one camera for the left and one for the right space
        public virtual void OnRender(Camera camera, Space space)
        {

        }
    }
}