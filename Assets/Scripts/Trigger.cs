using System;
using UnityEngine;
using UnityEngine.Events;

namespace poetools
{
    [Serializable]
    // Events that provide information about a collision; these handled well by scripts
    public struct CollisionEventsWithData 
    {
        public UnityEvent<Collider> collisionEnterWithData;
        public UnityEvent<Collider> collisionExitWithData;
        public UnityEvent<Collider> collisionStayWithData;
    }
    
    [Serializable]
    // Events that don't provide collision information; these are handled well by the editor
    public struct CollisionEvents 
    {
        public UnityEvent collisionEnter;
        public UnityEvent collisionExit;
        public UnityEvent collisionStay;
    }

    // Wraps a trigger Collider and provides UnityEvents for OnTriggerEnter / Exit / Stay
    public class Trigger : MonoBehaviour
    {
        public Collider colliderComponent = default;
        public CollisionEvents events = new CollisionEvents();
        public CollisionEventsWithData eventsWithData = new CollisionEventsWithData();
    
        private void Awake()
        {
            if (colliderComponent == null)
                FixTriggerDependency();

            colliderComponent.isTrigger = true;
        }

        private void FixTriggerDependency()
        {
            colliderComponent = GetComponent<Collider>();
        
            if (colliderComponent == null)
                colliderComponent = CreateTrigger();
        }

        private Collider CreateTrigger()
        {
            return gameObject.AddComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            events.collisionEnter?.Invoke();
            eventsWithData.collisionEnterWithData?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            events.collisionExit.Invoke();
            eventsWithData.collisionExitWithData?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            events.collisionStay?.Invoke();
            eventsWithData.collisionStayWithData?.Invoke(other);
        }
    }
}