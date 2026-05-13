using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.05f;
    public LayerMask targetLayer = -1;
    public float maxDistance = 10f;
    
    [Header("Cursor Settings")]
    public bool lockCursor = true;
    
    private GameObject selectedObj;
    private GameObject hoveredObj;
    private GameObject lastHoveredObj;
    private Camera mainCamera;
    private float hoverConfirmTime = 0.05f;
    private float currentHoverTime = 0f;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void Update()
    {
        if (mainCamera == null) return;
        
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        GameObject hitObject = null;
        
        Vector3[] offsets = new Vector3[] {
            Vector3.zero,
            mainCamera.transform.up * 0.01f,
            -mainCamera.transform.up * 0.01f,
            mainCamera.transform.right * 0.01f,
            -mainCamera.transform.right * 0.01f
        };
        
        foreach (Vector3 offset in offsets)
        {
            Ray offsetRay = new Ray(mainCamera.transform.position + offset, mainCamera.transform.forward);
            if (Physics.Raycast(offsetRay, out hit, maxDistance, targetLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null)
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                
                if (interactable != null)
                {
                    hitObject = hit.collider.gameObject;
                    break;
                }
            }
        }
        
        if (hitObject != lastHoveredObj)
        {
            lastHoveredObj = hitObject;
            currentHoverTime = 0f;
        }
        else
        {
            currentHoverTime += Time.deltaTime;
        }
        
        if (currentHoverTime >= hoverConfirmTime && hitObject != hoveredObj)
        {
            if (hoveredObj != null && hoveredObj != selectedObj)
                SetOutline(hoveredObj, false);
            
            hoveredObj = hitObject;
            
            if (hoveredObj != null && hoveredObj != selectedObj)
                SetOutline(hoveredObj, true);
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (hoveredObj != null)
            {
                IInteractable interactable = hoveredObj.GetComponent<IInteractable>();
                if (interactable == null)
                    interactable = hoveredObj.GetComponentInParent<IInteractable>();
                
                if (interactable != null)
                {
                    if (selectedObj != null && selectedObj != hoveredObj)
                        SetOutline(selectedObj, false);
                    
                    selectedObj = hoveredObj;
                    SetOutline(selectedObj, true);
                    interactable.Interact();
                }
            }
            else if (selectedObj != null)
            {
                SetOutline(selectedObj, false);
                selectedObj = null;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void SetOutline(GameObject obj, bool enabled)
    {
        if (obj == null) return;
        
        TightSpriteOutline tight = obj.GetComponent<TightSpriteOutline>();
        if (tight == null)
            tight = obj.AddComponent<TightSpriteOutline>();
        
        // Use per-object width if available
        float width = outlineWidth;
        SimpleInteractable interactable = obj.GetComponent<SimpleInteractable>();
        if (interactable != null && interactable.outlineWidth > 0)
        {
            width = interactable.outlineWidth;
        }
        
        tight.EnableOutline(enabled, outlineColor, width);
    }
}