using UnityEngine;
using UnityEditor;
using Photon.Pun;

namespace VRception
{
    /// <summary>
    /// This class implements the GUI of the "Interactable" script; one of the core functionalities of the VRception toolkit
    /// </summary>
    [CustomEditor(typeof(Interactable))]
    public class InteractableEditor : Editor
    {
        private Interactable interactable;

        // This function is called when the object becomes enabled and active.
        void OnEnable()
        {
            this.interactable = (Interactable)target;

            this.Update();
        }

        // This function is called for rendering and handling the inspector GUI
        public override void OnInspectorGUI()
        {
            bool lastIsSubInteractable = this.interactable.isSubInteractable;
            bool lastIsSynchronized = this.interactable.isSynchronized;
            bool lastAutoGenerate = this.interactable.autoGenerate;

            base.OnInspectorGUI();

            if(lastIsSubInteractable != this.interactable.isSubInteractable || lastIsSynchronized != this.interactable.isSynchronized || lastAutoGenerate != this.interactable.autoGenerate)
                this.Update();
        }

        // This method updates the look of the inspector GUI for the interactable
        private void Update()
        {
            // Auto generation of Photon network components enabled?
            if(!this.interactable.autoGenerate)
                return;

            // Is snychronization enabled?
            if(this.interactable.isSynchronized)
            {
                // Add "Cloneable" component, if modules require it
                if(this.interactable.GetComponent<ModuleDuplicate>() != null || this.interactable.GetComponent<ModuleInterface>() != null || 
                    this.interactable.GetComponent<MarkerPrefab>() != null)
                {
                    Cloneable cloneable = this.interactable.GetComponent<Cloneable>();
                    if(cloneable == null)
                        this.interactable.GetGameObject().AddComponent<Cloneable>();
                }
                // "Cloneable" not required, we remove it
                else
                    this.RemoveComponentCloneable();

                // Get or add "Photon View" component
                PhotonView photonView = this.interactable.GetComponent<PhotonView>();
                if(!this.interactable.isSubInteractable)
                {
                    if(photonView == null)
                        photonView = this.interactable.GetGameObject().AddComponent<PhotonView>();
                
                    // Configure photon view
                    photonView.OwnershipTransfer = OwnershipOption.Request;
                    photonView.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
                    photonView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
                }
                else
                {
                    if(this.interactable.isSubInteractable)
                        this.RemoveComponentView();
                }

                // Add "Photon Transform View" component to this gameobject, if modules require it
                if(this.interactable.GetComponent<ModuleTranslate>() != null || this.interactable.GetComponent<ModuleTranslateLocal>() != null || 
                    this.interactable.GetComponent<ModuleRotate>() != null)
                {
                    PhotonTransformView transformView = this.interactable.GetComponent<PhotonTransformView>();
                    if(transformView == null)
                        transformView = this.interactable.GetGameObject().AddComponent<PhotonTransformView>();

                    if(this.interactable.GetComponent<ModuleTranslate>() != null || this.interactable.GetComponent<ModuleTranslateLocal>() != null)
                        transformView.m_SynchronizePosition = true;
                    else
                        transformView.m_SynchronizePosition = false;

                    if(this.interactable.GetComponent<ModuleRotate>() != null)
                        transformView.m_SynchronizeRotation = true;
                    else
                        transformView.m_SynchronizeRotation = false;

                    transformView.m_SynchronizeScale = false;
                    transformView.m_UseLocal = true;
                }
                else
                {
                    this.RemoveComponentTransformView();
                }

                // Add "Photon Transform View" component to the specified model gameobject, if module requires it
                if(this.interactable.GetComponent<ModuleScale>() != null)
                {
                    if(this.interactable.modelGameObject != null)
                    {
                        PhotonTransformView transformView = this.interactable.modelGameObject.GetComponent<PhotonTransformView>();
                        if(transformView == null)
                            transformView = this.interactable.modelGameObject.AddComponent<PhotonTransformView>();

                        transformView.m_SynchronizePosition = false;
                        transformView.m_SynchronizeRotation = false;
                        transformView.m_SynchronizeScale = true;
                        transformView.m_UseLocal = true;
                    }
                }
                else
                {
                    this.RemoveComponentTransformViewInModel();
                }
            }
            // Synchronization is disabled
            else
            {
                this.RemoveComponentCloneable();
                this.RemoveComponentTransformView();
                this.RemoveComponentTransformViewInModel();
                this.RemoveComponentView();
            }
        }

        // Remove cloneable, if it exists
        private void RemoveComponentCloneable()
        {
            Cloneable cloneable = this.interactable.GetComponent<Cloneable>();
            if(cloneable != null)
                DestroyImmediate(cloneable, true);
        }

        // Remove "Photon Transform View" component in interactable, if it exists
        private void RemoveComponentTransformView()
        {
            PhotonTransformView transformView = this.interactable.GetComponent<PhotonTransformView>();
            if(transformView != null)
                DestroyImmediate(transformView, true);
        }

        // Remove "Photon Transform View" component of the specified model gameobject, if it exists
        private void RemoveComponentTransformViewInModel()
        {
            if(this.interactable.modelGameObject != null)
            {
                PhotonTransformView transformView = this.interactable.modelGameObject.GetComponent<PhotonTransformView>();
                if(transformView != null)
                    DestroyImmediate(transformView, true);
            }
        }

        // Remove "Photon View" component
        private void RemoveComponentView()
        {
            PhotonView photonView = this.interactable.GetComponent<PhotonView>();
            if(photonView != null)
                DestroyImmediate(photonView, true);
        }
    }
}