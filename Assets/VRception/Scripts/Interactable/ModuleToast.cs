using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The toast module allows to show toast messages to inform players that certain interactions are not supported by the selected interactable when players try to perform them.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ModuleToast : MonoBehaviour
    {
        //// SECTION "Toast Settings"
        [Header("Toast Settings", order = 0)]
        [Helpbox("The toast module allows to show toast messages to inform players that certain interactions are not supported by the selected interactable when players try to perform them.", order = 1)]
        [Tooltip("Show toast when player tries to scale the interactable.", order = 2)]
        public bool toastForScaling = true;

        [Tooltip("Show toast when player tries to delete the interactable.")]
        public bool toastForDeleting = true;

        [Tooltip("Show toast when player tries to duplicate the interactable.")]
        public bool toastForDuplicating = true;

        private Interactable interactable = null;
        
        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.interactable.IsSelected() && !this.interactable.IsPrefabInInterface())
            {
                // Scale interaction
                if(Mapping.ButtonInteractableScale() && this.toastForScaling)
                {
                    if(this.GetComponent<ModuleScale>() == null)
                        Settings.instance.SetToast("Object cannot be scaled");
                }

                // Delete interaction
                if(Mapping.ButtonInteractableDelete() && this.toastForDeleting)
                {
                    if(this.GetComponent<ModuleDelete>() == null)
                        Settings.instance.SetToast("Object cannot be deleted");
                }

                // Duplicate interaction
                if(Mapping.ButtonInteractableDuplicate() && this.toastForDuplicating)
                {
                    if(this.GetComponent<ModuleDuplicate>() == null)
                        Settings.instance.SetToast("Object cannot be duplicated");
                }
            }
        }
    }
}
