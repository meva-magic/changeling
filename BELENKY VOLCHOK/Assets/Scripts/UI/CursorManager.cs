using UnityEngine;

public class CursorManager : MonoBehaviour, CursorController
{
    public static CursorManager Instance { get; private set; }
    
    private CursorLockMode previousLockState;
    private bool previousVisibility;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void LockForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void UnlockForUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void SaveAndUnlock()
    {
        previousLockState = Cursor.lockState;
        previousVisibility = Cursor.visible;
        UnlockForUI();
    }
    
    public void Restore()
    {
        Cursor.lockState = previousLockState;
        Cursor.visible = previousVisibility;
    }
}
