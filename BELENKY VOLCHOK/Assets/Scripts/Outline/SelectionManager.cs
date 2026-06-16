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
    private bool isMinigameActive = false;
    private GameObject lastOutlineTarget;
    private Transform playerTransform;
    
    public GameObject GetHoveredObject()
    {
        return hoveredObj;
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void Update()
    {
        if (mainCamera == null) return;
        
        bool wasMinigameActive = isMinigameActive;
        isMinigameActive = ServiceLocator.Get<MinigameStarter>()?.IsMinigameActive == true;
        
        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive;
        
        if (!wasMinigameActive && (isMinigameActive || isDialogueActive))
        {
            if (hoveredObj != null) ClearOutline();
            hoveredObj = null;
            lastHoveredObj = null;
            return;
        }
        
        if (isMinigameActive || isDialogueActive) return;
        
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        GameObject hitObject = null;
        float hitDistance = maxDistance;
        
        if (Physics.Raycast(ray, out hit, maxDistance, targetLayer))
        {
            IClickable interactable = hit.collider.GetComponent<IClickable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IClickable>();
            
            if (interactable != null)
            {
                float range = interactable.GetInteractionRange();
                float distanceToPlayer = Vector3.Distance(hit.collider.transform.position, playerTransform.position);
                
                if (hit.distance <= range && distanceToPlayer <= range)
                {
                    hitObject = hit.collider.gameObject;
                    hitDistance = hit.distance;
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
            if (hoveredObj != null) ClearOutline();
            hoveredObj = hitObject;
            if (hoveredObj != null)
            {
                IClickable interactable = hoveredObj.GetComponent<IClickable>();
                if (interactable == null)
                    interactable = hoveredObj.GetComponentInParent<IClickable>();
                
                if (interactable != null)
                {
                    GameObject outlineTarget = interactable.GetOutlineTarget();
                    SetOutline(outlineTarget, true);
                    lastOutlineTarget = outlineTarget;
                }
            }
        }
        
        if (Input.GetMouseButtonDown(0) && hoveredObj != null)
        {
            if (selectedObj != null && selectedObj != hoveredObj)
            {
                IClickable oldInteractable = selectedObj.GetComponent<IClickable>();
                if (oldInteractable != null)
                {
                    SetOutline(oldInteractable.GetOutlineTarget(), false);
                }
            }
            selectedObj = hoveredObj;
            IClickable newInteractable = selectedObj.GetComponent<IClickable>();
            if (newInteractable != null)
            {
                SetOutline(newInteractable.GetOutlineTarget(), true);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    private void ClearOutline()
    {
        if (lastOutlineTarget != null)
        {
            SetOutline(lastOutlineTarget, false);
            lastOutlineTarget = null;
        }
    }
    
    private void SetOutline(GameObject obj, bool enabled)
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