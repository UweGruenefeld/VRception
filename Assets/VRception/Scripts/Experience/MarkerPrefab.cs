using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace VRception
{
    /// <summary>
    /// This script needs to attached to a marker prefab gameobject. 
    /// These gameobjects are prefabs that can be assigned as a symbol or character marker in the 'Experience Settings' gameobject.
    /// </summary>
    [RequireComponent(typeof(Cloneable))]
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(ModuleHighlight))]
    [RequireComponent(typeof(ModuleTranslate))]
    [RequireComponent(typeof(ModuleRotate))]
    [RequireComponent(typeof(ModuleToast))]
    public class MarkerPrefab : MonoBehaviourPunCallbacks, IPunObservable
    {
        //// SECTION "Marker Prefab Settings"
        [Header("Marker Prefab Settings", order = 0)]
        [Helpbox("Markers can be represented by a prefab gameobject. Every such prefab gameobject needs to have this script attached as a component as well as the interactable and cloneable component.", order = 1)]
        [Tooltip("Reference to the TextMesh component that displays the currently active experience for this marker.", order = 2)]
        public TextMesh labelText;
        [Tooltip("Reference to the Gameobject that represents the background to the TextMesh component that displays the currently active experience for this marker.", order = 2)]
        public GameObject labelBackground;

        private Interactable interactable;

        private Experience experience;
        public bool isUsed { get; private set; }

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.interactable = this.GetComponent<Interactable>();
            this.isUsed = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Register the marker prefab in the experience controller
            ControllerExperiences.instance.AddMarkerPrefab(this);
        }

        // Destroying the attached Behaviour will result in the game or Scene receiving OnDestroy
        void OnDestroy()
        {
            // Unregister the marker prefab in the experience controller
            ControllerExperiences.instance.RemoveMarkerPrefab(this);
        }

        // Method sets the experience
        public void SetExperience(Experience experience)
        {
            this.SetMarkerText(ControllerExperiences.instance.GetExperience(experience).ToString());
            this.experience = experience;
        }

        // Receive and send experiences from other clients
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // IMPORTANT: Read and write in the exact same order
            if (stream.IsWriting) 
            {
                stream.SendNext ((int)this.experience);
                stream.SendNext (this.isUsed);
            } 
            else 
            {
                this.experience = (Experience)stream.ReceiveNext();
                this.isUsed = (bool)stream.ReceiveNext();

                this.interactable.enabled = !this.isUsed;
                this.interactable.modelGameObject.SetActive(!this.isUsed);
                if(!isUsed)
                    this.SetMarkerText(ControllerExperiences.instance.GetExperience(this.experience).ToString());
            }
        }

        // Method is called when a marker is entered
        public Experience OnMarkerEnter()
        {
            this.isUsed = true;
            this.interactable.enabled = false;
            this.interactable.modelGameObject.SetActive(false);

            return this.experience;
        }

        // Method is called when a marker is left
        public void OnMarkerLeave(Experience experience)
        {
            this.isUsed = false;
            this.interactable.enabled = true;
            this.interactable.modelGameObject.SetActive(true);

            this.experience = experience;
            this.SetMarkerText(ControllerExperiences.instance.GetExperience(this.experience).ToString());
        }

        // Method sets the text of a marker
        private void SetMarkerText(string text)
        {
            // Set text
            if(this.labelText != null)
                this.labelText.text = text;

            // Adjust background
            if(this.labelBackground != null)
            {
                float width = Utilities.GetWidth(this.labelText);
                this.labelBackground.transform.localScale = new Vector3((width / 10f) + 0.004f, 1, 0.006f);
            }
        }
    }
}