using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float sensitivity = 1.5f;
    [SerializeField] private float smoothing = 1.5f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    
    private float horizontalAngle;
    private float verticalAngle;
    private float smoothedX;
    private float smoothedY;
    
    private void Start()
    {
        if (cameraHolder == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null && cam.transform.parent == transform)
            {
                GameObject mount = new GameObject("CameraHolder");
                mount.transform.SetParent(transform);
                mount.transform.localPosition = Vector3.zero;
                mount.transform.localRotation = Quaternion.identity;
                cam.transform.SetParent(mount.transform);
                cameraHolder = mount.transform;
            }
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        if (ServiceLocator.Get<MinigameStarter>()?.IsMinigameActive == true) return;
        
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
        
        mouseX *= sensitivity;
        mouseY *= sensitivity;
        
        smoothedX = Mathf.Lerp(smoothedX, mouseX, 1f / smoothing);
        smoothedY = Mathf.Lerp(smoothedY, mouseY, 1f / smoothing);
        
        horizontalAngle += smoothedX;
        transform.localRotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
        
        verticalAngle -= smoothedY;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
        
        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
    }
}