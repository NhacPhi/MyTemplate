using System;
using UnityEngine;

namespace Gameplay.MapCharacter.Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    public class EnemySensor : MonoBehaviour
    {
        public Action<Transform> OnPlayerEnter;
        public Action<Transform> OnPlayerExit;

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.CompareTag("Player"))
            {
                OnPlayerEnter?.Invoke(collision.transform);
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.CompareTag("Player"))
            {
                OnPlayerExit?.Invoke(collision.transform);
            }
        }
    }
}
