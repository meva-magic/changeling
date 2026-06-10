using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    [SerializeField] private Transform eyePivot;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private float maxHeadAngle = 20f;
    [SerializeField] private float maxEyeShift = 0.03f;
    [SerializeField] private float smoothSpeed = 8f;
    
    private Transform playerTarget;
    private Quaternion defaultPivotRotation;
    private Vector3 defaultLeftPosition;
    private Vector3 defaultRightPosition;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
        
        if (eyePivot != null)
        {
            defaultPivotRotation = eyePivot.localRotation;
        }
        
        if (leftEye != null)
        {
            defaultLeftPosition = leftEye.localPosition;
        }
        
        if (rightEye != null)
        {
            defaultRightPosition = rightEye.localPosition;
        }
    }
    
    private void Update()
    {
        if (playerTarget == null) return;
        
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        Vector3 localDir = transform.InverseTransformDirection(direction);
        
        if (eyePivot != null)
        {
            float yaw = Mathf.Clamp(localDir.x * maxHeadAngle, -maxHeadAngle, maxHeadAngle);
            float pitch = Mathf.Clamp(-localDir.y * maxHeadAngle * 0.5f, -maxHeadAngle / 2f, maxHeadAngle / 2f);
            
            Quaternion targetRot = defaultPivotRotation * Quaternion.Euler(pitch, yaw, 0);
            eyePivot.localRotation = Quaternion.Slerp(eyePivot.localRotation, targetRot, Time.deltaTime * smoothSpeed);
        }
        
        if (leftEye != null && rightEye != null)
        {
            Vector3 shift = new Vector3(localDir.x * maxEyeShift, localDir.y * maxEyeShift, 0);
            shift = Vector3.ClampMagnitude(shift, maxEyeShift);
            
            leftEye.localPosition = Vector3.Lerp(leftEye.localPosition, defaultLeftPosition + shift, Time.deltaTime * smoothSpeed);
            rightEye.localPosition = Vector3.Lerp(rightEye.localPosition, defaultRightPosition + shift, Time.deltaTime * smoothSpeed);
        }
    }
}