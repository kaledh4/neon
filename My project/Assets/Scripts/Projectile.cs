using UnityEngine;

namespace NeonSplash.V0_1
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 50f;
        public float lifetime = 3f;
        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            
            rb.useGravity = false;
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Add impact effect here if needed
            Destroy(gameObject);
        }
    }
}
