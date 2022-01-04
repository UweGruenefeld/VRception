using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRception
{
    /// <summary>
    /// The component is attached to a gameobject with a collider to inform the interactable it belongs to, if there has been a collision (with another interactable)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CollisionDetector : MonoBehaviour
    {
        //// SECTION "Collisions"
        [Header("Collisions", order = 0)]
        [Helpbox("This component is attached to a gameobject with a collider to inform the interactable it belongs to, if there has been a collision (with another interactable). All currently ongoing collisions will be automatically stored in the list below.", order = 1)]
        [Tooltip("This variables stores all ongoing collisions for this gameobject. Please do not modify.", order = 2)]
        public List<GameObject> allCollisions = null;

        // Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        void OnValidate()
        {
            // Configure rigidbody
            Rigidbody rigidbody = this.GetComponent<Rigidbody>();
            if(rigidbody != null)
            {
                rigidbody.drag = 0;
                rigidbody.angularDrag = 0;
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
            }

            // Configure collider
            Collider collider = this.GetComponent<Collider>();
            if(collider != null)
                collider.isTrigger = true;
        }

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            this.allCollisions = new List<GameObject>();
        }

        // Frame-rate independent MonoBehaviour.FixedUpdate message for physics calculations.
        void FixedUpdate()
        {
            // Remove all delete objects
            this.allCollisions.RemoveAll(item => item == null);
        }

        // When a GameObject collides with another GameObject, Unity calls OnTriggerEnter.
        void OnTriggerEnter(Collider collider)
        {
            if (this.allCollisions.Contains(collider.gameObject)) 
                return;

            this.allCollisions.Add(collider.gameObject);
        }

        // OnTriggerExit is called when the Collider other has stopped touching the trigger.
        void OnTriggerExit(Collider collider)
        {
            if (!this.allCollisions.Contains(collider.gameObject)) 
                return;
            
            this.allCollisions.Remove(collider.gameObject);
        }

        // Returns the list of currently ongoing collisions in reverse order (longest ongoing collision will be first element)
        public GameObject[] GetCurrentCollisions()
        {
            GameObject[] results = this.allCollisions.ToArray();
            Array.Reverse(results);
            return results;
        }
    }
}