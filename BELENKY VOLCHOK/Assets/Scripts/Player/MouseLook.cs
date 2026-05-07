using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder; // The new empty parent for the camera
    
    [Header("Sensitivity Settings")]
    [SerializeField] private float sensitivity = 1.5f;
    [SerializeField] private float smoothing = 1.5f;
    
    [Header("Vertical Look Limits")]
    [SerializeField] private bool limitVerticalLook = true;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    
    private float currentHorizontalLook;
    private float currentVerticalLook;
    
    private float xMousePos;
    private float yMousePos;
    private float smoothedMouseX;
    private float smoothedMouseY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        // Auto-find camera holder if not assigned
        if (cameraHolder == null)
        {
            // Assume camera is first child and we need to create a holder
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null && cam.transform.parent == transform)
            {
                // Create holder between player and camera
                GameObject holder = new GameObject("CameraHolder");
                holder.transform.SetParent(transform);
                holder.transform.localPosition = Vector3.zero;
                holder.transform.localRotation = Quaternion.identity;
                
                cam.transform.SetParent(holder.transform);
                cameraHolder = holder.transform;
            }
        }
    }

    private void Update()
    {
        GetInput();
        ModifyInput();
        MovePlayer();
    }

    private void GetInput()
    {
        xMousePos = Input.GetAxisRaw("Mouse X");
        yMousePos = Input.GetAxisRaw("Mouse Y");
    }

    private void ModifyInput()
    {
        xMousePos *= sensitivity;
        yMousePos *= sensitivity;
        
        smoothedMouseX = Mathf.Lerp(smoothedMouseX, xMousePos, 1f / smoothing);
        smoothedMouseY = Mathf.Lerp(smoothedMouseY, yMousePos, 1f / smoothing);
    }

    private void MovePlayer()
    {
        // Horizontal rotation - only rotate the player object around Y axis
        currentHorizontalLook += smoothedMouseX;
        transform.localRotation = Quaternion.AngleAxis(currentHorizontalLook, Vector3.up);
        
        // Vertical rotation - only rotate the camera holder around X axis
        currentVerticalLook -= smoothedMouseY;
        
        if (limitVerticalLook)
        {
            currentVerticalLook = Mathf.Clamp(currentVerticalLook, minVerticalAngle, maxVerticalAngle);
        }
        
        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(currentVerticalLook, 0f, 0f);
        }
    }
}