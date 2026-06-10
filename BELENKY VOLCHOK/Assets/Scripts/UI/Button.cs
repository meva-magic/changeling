using UnityEngine;
using UnityEngine.UI;

public class MiniTest : MonoBehaviour
{
    [SerializeField] private Button testButton;
    
    private void Start()
    {
        if (testButton != null)
            testButton.onClick.AddListener(() => Debug.Log("BUTTON WORKING!"));
        else
            Debug.LogError("Button not assigned!");
    }
}