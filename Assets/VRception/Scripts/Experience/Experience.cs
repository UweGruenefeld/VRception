namespace VRception
{
    /// <summary>
    /// All experiences available should be listed here; Moreover, they require an implementation of the IExperience class.
    /// </summary>
    public enum Experience
    {
        // Used for bitmasks; thus, we need binary one hot encoding
        REALITY = 1 << 0,
        AUGMENTEDREALITY = 1 << 1,
        AUGMENTEDVIRTUALITY = 1 << 3,
        VIRTUALREALITY = 1 << 4
    }
}