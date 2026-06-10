using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerMovement movement;
    
    public enum ControlState { Gameplay, UI }
    private ControlState currentState;
    
    private void Start()
    {
        if (mouseLook == null) mouseLook = GetComponentInChildren<MouseLook>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        
        SwitchToGameplay();
    }
    
    public void SwitchToGameplay()
    {
        currentState = ControlState.Gameplay;
        
        if (mouseLook != null) mouseLook.enabled = true;
        if (movement != null) movement.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void SwitchToUI()
    {
        currentState = ControlState.UI;
        
        if (mouseLook != null) mouseLook.enabled = false;
        if (movement != null) movement.enabled = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public ControlState GetCurrentState() => currentState;
}