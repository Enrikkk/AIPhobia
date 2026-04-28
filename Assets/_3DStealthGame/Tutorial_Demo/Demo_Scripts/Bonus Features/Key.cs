using System;
using UnityEngine;

namespace StealthGame
{
    public class Key : MonoBehaviour
    {
        public string KeyName = "key1";

        private void OnTriggerEnter(Collider other)
        {
            // Search the whole player hierarchy — the collider that triggers may be
            // a child object (e.g. CharacterController capsule), not the root with the script.
            PlayerKeys keys = other.GetComponentInParent<PlayerKeys>();
            if (keys == null)
                return;

            keys.AddKey(KeyName);
            Destroy(gameObject);
        }
    }
}