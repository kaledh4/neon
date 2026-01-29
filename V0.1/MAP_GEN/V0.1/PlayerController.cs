using UnityEngine;

namespace NeonSplash.V0_1
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float jumpForce = 5f;
        public float groundCheckDistance = 1.1f;
        public LayerMask groundLayer;

        [Header("Components")]
        private Rigidbody rb;
        private bool isGrounded;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true; // Prevent character from falling over
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        void Update()
        {
            HandleJump();
        }

        void FixedUpdate()
        {
            Move();
            CheckGround();
        }

        private void Move()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            // Movement relative to camera or world? 
            // For now, let's use world coordinates since the map is linear along X axis mostly? 
            // Actually the map generates along X, let's assume world space is fine for this test.
            
            Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;
            
            // Preserve vertical velocity (gravity/jumping)
            Vector3 finalVelocity = new Vector3(movement.x, rb.velocity.y, movement.z);
            
            rb.velocity = finalVelocity;

            // Optional: Rotate towards move direction
            if (movement.x != 0 || movement.z != 0)
            {
                transform.forward = Vector3.Lerp(transform.forward, new Vector3(moveX, 0, moveZ), 0.15f);
            }
        }

        private void HandleJump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void CheckGround()
        {
            // Simple raycast down from center
            // Ensure we only check against floors (default layer or specified)
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                isGrounded = true;
                Debug.DrawLine(transform.position, hit.point, Color.green);
            }
            else
            {
                isGrounded = false;
                Debug.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance, Color.red);
            }
        }
    }
}
