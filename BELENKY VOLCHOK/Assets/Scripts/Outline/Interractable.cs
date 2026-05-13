using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [Header("Outline Override")]
    public float outlineWidth = 0.05f;
    public bool useOutline = true; // Toggle to disable outline
    
    public void Interact()
    {
        Debug.Log("Interacted with: " + gameObject.name);
    }
    
    public string GetInteractionName()
    {
        return gameObject.name;
    }
}