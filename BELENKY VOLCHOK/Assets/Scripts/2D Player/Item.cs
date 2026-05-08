using UnityEngine;

public class PickupableItem : MonoBehaviour
{
    public string itemID;
    public bool slowsPlayer;
    public float pickupRange = 2f;
    [SerializeField] private string sceneToLoadOnPickup = "";
    
    private bool isBeingCarried;
    private Collider2D itemCollider;
    
    public bool IsBeingCarried => isBeingCarried;
    
    private void Awake() { itemCollider = GetComponent<Collider2D>(); }
    
    public void OnPickup(Transform carryPoint)
    {
        isBeingCarried = true;
        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        if (itemCollider != null) itemCollider.enabled = false;
        
        if (!string.IsNullOrEmpty(sceneToLoadOnPickup))
        {
            SceneLoader loader = FindObjectOfType<SceneLoader>();
            if (loader != null)
                loader.LoadScene(sceneToLoadOnPickup);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnPickup);
        }
    }
    
    public void OnDrop(Vector3 position)
    {
        isBeingCarried = false;
        transform.SetParent(null);
        transform.position = position;
        if (itemCollider != null) itemCollider.enabled = true;
    }
}