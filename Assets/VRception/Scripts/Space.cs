namespace VRception
{
    /// <summary>
    /// The VRception toolit supports different spaces. Below, all of them are listed and explained.
    /// <</summary>
    public enum Space
    {
        DEFAULT,    // the default space is used for the main scene (the one that contains all settings scripts); nothing of the default space is rendered
        SHARED,     // the shared space is for scenes and gameobjects that should always be visible (e.g., they exist beyond reality and virtuality)
        LEFT,       // the left space; per default refers to the reality space 
        RIGHT       // the right space; per default refers to the virtuality space
    }
}