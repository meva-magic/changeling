using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    
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
        
        if (cameraHolder == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null && cam.transform.parent == transform)
            {
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
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive) return;
        
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
        currentHorizontalLook += smoothedMouseX;
        transform.localRotation = Quaternion.AngleAxis(currentHorizontalLook, Vector3.up);
        
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