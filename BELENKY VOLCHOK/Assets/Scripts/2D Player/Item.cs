using UnityEngine;
using System.Collections;

public class PickupableItem : MonoBehaviour
{
    public string itemID;
    public bool slowsPlayer;
    public float pickupRange = 2f;
    [SerializeField] private string sceneToLoadOnPickup = "";
    [SerializeField] private float screamerDelay = 1f;
    [SerializeField] private GameObject screamerPanel;
    [SerializeField] private float screamerDuration = 1.5f;
    [SerializeField] private string screamerSoundName = "";
    
    private bool isBeingCarried;
    private Collider2D itemCollider;
    private Rigidbody2D itemRb;
    
    public bool IsBeingCarried => isBeingCarried;
    
    private void Awake() 
    { 
        gameObject.tag = "Item";
        
        itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null)
            itemCollider = gameObject.AddComponent<BoxCollider2D>();
        itemCollider.isTrigger = true;
        
        itemRb = GetComponent<Rigidbody2D>();
        if (itemRb == null)
            itemRb = gameObject.AddComponent<Rigidbody2D>();
        itemRb.bodyType = RigidbodyType2D.Kinematic;
        itemRb.gravityScale = 0;
        itemRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        itemRb.simulated = true;
    }
    
    private void Start()
    {
        if (screamerPanel != null)
            screamerPanel.SetActive(false);
    }
    
    public void OnPickup(Transform carryPoint)
    {
        isBeingCarried = true;
        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        if (itemCollider != null) 
            itemCollider.enabled = false;
        
        if (itemRb != null)
        {
            itemRb.simulated = false;
            itemRb.velocity = Vector2.zero;
        }
        
        if (itemID == "Changeling")
        {
            StartCoroutine(ScreamerSequence());
        }
        else if (!string.IsNullOrEmpty(sceneToLoadOnPickup))
        {
            SceneLoader loader = FindObjectOfType<SceneLoader>();
            if (loader != null)
                loader.LoadScene(sceneToLoadOnPickup);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnPickup);
        }
    }
    
    private IEnumerator ScreamerSequence()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.InputBlocked = true;
        
        yield return new WaitForSeconds(screamerDelay);
        
        if (screamerPanel != null)
            screamerPanel.SetActive(true);
        
        if (!string.IsNullOrEmpty(screamerSoundName) && AudioManager.instance != null)
            AudioManager.instance.Play(screamerSoundName);
        
        yield return new WaitForSeconds(screamerDuration);
        
        if (screamerPanel != null)
            screamerPanel.SetActive(false);
        
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
        transform.rotation = Quaternion.identity;
        
        if (itemCollider != null) 
            itemCollider.enabled = true;
        
        if (itemRb != null)
        {
            itemRb.simulated = true;
            itemRb.velocity = Vector2.zero;
        }
    }
}