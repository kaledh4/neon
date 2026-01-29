using UnityEngine;

namespace NeonSplash.V0_1
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(-10, 15, -10); // High up, looking down-diagonal
        public float smoothSpeed = 5f;
        public bool lockZAxis = true; // Keep camera centered on the main track

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            // If we want to ignore side movement and focus on the "Center Track" (Z=0)
            if (lockZAxis)
            {
                desiredPosition.z = offset.z; 
            }

            // Smooth move
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Look at player (but keep the look target centered on the track too?)
            Vector3 lookTarget = target.position;
            if (lockZAxis) lookTarget.z = 0;
            
            transform.LookAt(lookTarget);
        }
    }
}
