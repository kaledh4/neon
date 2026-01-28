using UnityEngine;

namespace NeonSplash.V0_1
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 15f;
        public float jumpForce = 12f;
        public float groundCheckDistance = 1.2f;
        public LayerMask groundLayer = ~0; // Default to Everything

        [Header("Look Settings")]
        public float mouseSensitivity = 2f;
        public float upDownRange = 80f;

        [Header("Components")]
        private Rigidbody rb;
        private Camera mainCamera;
        private float verticalRotation = 0;
        private bool isGrounded;
        
        public ColorPalette palette;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true; 
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            mainCamera = Camera.main;

            // Lock cursor for shooter feel
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleLook();
            HandleJump();
            HandleShoot();
        }

        private void HandleShoot()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0)) // Left Click
            {
                FireProjectile();
            }
        }

        private void FireProjectile()
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.position = mainCamera.transform.position + mainCamera.transform.forward;
            bullet.transform.rotation = mainCamera.transform.rotation;
            bullet.transform.localScale = Vector3.one * 0.3f;
            
            bullet.AddComponent<Projectile>();
            
            // Apply alternate neon color
            if (palette != null)
            {
                Renderer rend = bullet.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = palette.Shooting;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", palette.Shooting * 5f); // High intensity glow
                rend.material = mat;
            }
            
            Destroy(bullet.GetComponent<SphereCollider>());
            bullet.AddComponent<SphereCollider>().isTrigger = false;
        }

        void FixedUpdate()
        {
            Move();
            CheckGround();
        }

        private void HandleLook()
        {
            // Horizontal rotation (Player stays upright)
            float mouseX = UnityEngine.Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, mouseX, 0);

            // Vertical rotation (Camera tilts)
            verticalRotation -= UnityEngine.Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
            
            if (mainCamera != null)
            {
                mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            }
        }

        private void Move()
        {
            float moveX = UnityEngine.Input.GetAxisRaw("Horizontal");
            float moveZ = UnityEngine.Input.GetAxisRaw("Vertical");

            // Calculate movement direction relative to player rotation
            Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
            Vector3 finalVelocity = moveDirection * moveSpeed;
            
            // Preserve vertical velocity
            finalVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = finalVelocity;
        }

        private void HandleJump()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false; // Immediate state change for snappiness
            }
        }

        private void CheckGround()
        {
            // Use sphere cast for more robust ground detection
            // Increased radius slightly and cast distance
            if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 0.45f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }
    }
}
