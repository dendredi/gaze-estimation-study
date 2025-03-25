using UnityEngine;
using UnityEngine.PlayerLoop;

public class GazeController : MonoBehaviour
{
    // Assign in inspector
    public AvatarRig avatarRig;
    public Transform target;

    // Configuration
    public float eyeSpeed = 5f; // Adjust how fast eyes move
    
    // Limit eye rotation
    private const float MAX_ROTATION_ANGLE_HORIZONTAL = 30f;
    private const float MAX_ROTATION_ANGLE_VERTICAL = 30f;

    // Gaze Strategies
    private NaiveEyesHeadNeckStrategy strategy = new NaiveEyesHeadNeckStrategy();
    
    void Start()
    {
        if (avatarRig == null || !avatarRig.IsAssigned())
        {
            throw new System.NullReferenceException("Avatar rig not properly assigned.");
        }
        if (target == null)
        {
            throw new System.NullReferenceException("Target is not assigned.");
        }
    }

    void Update()
    {
        strategy.Update(avatarRig, target);
    }

    [System.Serializable]
    public class AvatarRig
    {
        public Transform leftEye;
        public Transform rightEye;
        public Transform head;
        public Transform neck;

        public bool IsAssigned()
        {
            return leftEye != null && rightEye != null && head != null && neck != null;
        }
    }

    private class EyesOnlyStrategy
    {
        public void Update(AvatarRig avatarRig, Transform target)
        {
            var targetPosition = target.position;
            
            RotateEyeTowards(avatarRig.leftEye, targetPosition);
            RotateEyeTowards(avatarRig.rightEye, targetPosition);
        }
        
        private static void RotateEyeTowards(Transform eye, Vector3 targetPosition)
        {
            var direction = targetPosition - eye.position;
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            
            targetRotation = ClampRotation(targetRotation);

            //eye.rotation = Quaternion.Slerp(eye.rotation, targetRotation, Time.deltaTime * eyeSpeed);
            eye.rotation = targetRotation;
        }

        private static Quaternion ClampRotation(Quaternion targetRotation)
        {
            var eulerRotation = targetRotation.eulerAngles;
            
            // Normalize angles using DeltaAngle to keep them in the range [-180, 180]
            eulerRotation.x = Mathf.DeltaAngle(0, eulerRotation.x);
            eulerRotation.y = Mathf.DeltaAngle(0, eulerRotation.y);
            
            // Clamp the normalized angles
            eulerRotation.x = Mathf.Clamp(eulerRotation.x, -MAX_ROTATION_ANGLE_VERTICAL, MAX_ROTATION_ANGLE_VERTICAL);
            eulerRotation.y = Mathf.Clamp(eulerRotation.y, -MAX_ROTATION_ANGLE_HORIZONTAL, MAX_ROTATION_ANGLE_HORIZONTAL);
            
            return Quaternion.Euler(eulerRotation);
        }
    }

    private class NaiveEyesHeadNeckStrategy
    {
        private const float EYES_WEIGHT = 1.0f;
        private const float HEAD_WEIGHT = 0.5f;
        private const float NECK_WEIGHT = 0.3f;
        
        public void Update(AvatarRig avatarRig, Transform target)
        {
            if (Done(avatarRig, target))
            {
                return;
            }
            
            var targetPosition = target.position;
            
            // Calculate total rotation towards target for each bone
            var neckRotation = Quaternion.LookRotation(targetPosition - avatarRig.neck.position);
            var headRotation = Quaternion.LookRotation(targetPosition - avatarRig.head.position);
            var leftEyeRotation = Quaternion.LookRotation(targetPosition - avatarRig.leftEye.position);
            var rightEyeRotation = Quaternion.LookRotation(targetPosition - avatarRig.rightEye.position);
            
            // Apply weighted rotations
            avatarRig.neck.rotation = Quaternion.Slerp(Quaternion.identity, neckRotation, NECK_WEIGHT);
            avatarRig.head.rotation = Quaternion.Slerp(Quaternion.identity, headRotation, HEAD_WEIGHT);
            avatarRig.leftEye.rotation = Quaternion.Slerp(Quaternion.identity, leftEyeRotation, EYES_WEIGHT);
            avatarRig.rightEye.rotation = Quaternion.Slerp(Quaternion.identity, rightEyeRotation, EYES_WEIGHT);
        }

        private static bool Done(AvatarRig avatarRig, Transform target)
        {
            var currentDirectionLeftEye = avatarRig.leftEye.forward;
            var targetDirectionLeftEye = (target.position - avatarRig.leftEye.position).normalized;
            return Vector3.Angle(currentDirectionLeftEye, targetDirectionLeftEye) < 0.1f;
        }

    }
}