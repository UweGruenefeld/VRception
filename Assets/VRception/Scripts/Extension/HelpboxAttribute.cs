using System;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// Attribute that stores a helptext to display it in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class HelpboxAttribute : PropertyAttribute
    {
        public readonly string text;

        // Constructor of the Helpbox Attribute
        public HelpboxAttribute(string text)
        {
            this.text = text;
        }
    }
}