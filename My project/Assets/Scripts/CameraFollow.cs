using UnityEngine;

namespace NeonSplash.V0_1
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 2.5f, -4f); // Behind and slightly above
        public float smoothSpeed = 10f;

        void LateUpdate()
        {
            if (target == null) return;

            // Calculate the desired position based on player's current rotation
            Vector3 desiredPosition = target.position + (target.rotation * offset);

            // Smooth position movement
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // The main camera rotation is handled by PlayerController.HandleLook() 
            // but we need to ensure the Base Position follows correctly if it's not a child.
            // If it's NOT a child, we need to handle rotation here too or let PlayerController do it.
        }
    }
}
