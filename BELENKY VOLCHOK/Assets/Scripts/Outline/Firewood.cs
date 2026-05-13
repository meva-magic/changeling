using UnityEngine;

public class FirewoodInteract : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Clicked on " + gameObject.name);
    }
    
    public string GetInteractionName()
    {
        return "Firewood";
    }
}
