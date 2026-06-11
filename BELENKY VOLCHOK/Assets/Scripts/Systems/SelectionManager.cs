using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.white;
    public float outlineWidth = 0.05f;
    public LayerMask targetLayer = -1;
    public float maxDistance = 5f;
    
    [Header("Cursor Settings")]
    public bool lockCursor = true;
    
    private GameObject selectedObj;
    private GameObject hoveredObj;
    private GameObject lastHoveredObj;
    private Camera mainCamera;
    private float hoverConfirmTime = 0.05f;
    private float currentHoverTime = 0f;
    
    public GameObject GetHoveredObject()
    {
        return hoveredObj;
    }
    
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
        
        if (Physics.Raycast(ray, out hit, maxDistance, targetLayer))
        {
            IClickable interactable = hit.collider.GetComponent<IClickable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IClickable>();
            
            if (interactable != null)
            {
                float range = interactable.GetInteractionRange();
                if (hit.distance <= range)
                {
                    hitObject = hit.collider.gameObject;
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
        
        if (Input.GetMouseButtonDown(0) && hoveredObj != null)
        {
            if (selectedObj != null && selectedObj != hoveredObj)
                SetOutline(selectedObj, false);
            
            selectedObj = hoveredObj;
            SetOutline(selectedObj, true);
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
        
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
            outline.enabled = enabled;
        }
    }
}