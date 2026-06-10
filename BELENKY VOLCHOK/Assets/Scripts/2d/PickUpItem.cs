using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PickupableItem : MonoBehaviour
{
    public string itemID;
    public bool slowsPlayer;
    public float pickupRange = 2f;
    [SerializeField] private string sceneToLoadOnPickup = "";

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

    public void OnPickup(Transform carryPoint)
    {
        isBeingCarried = true;
        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (itemCollider != null) itemCollider.enabled = false;
        if (itemRb != null)
        {
            itemRb.simulated = false;
            itemRb.velocity = Vector2.zero;
        }

        if (!string.IsNullOrEmpty(sceneToLoadOnPickup))
        {
            CameraShake cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;
            if (cameraShake != null)
                cameraShake.StartInfiniteShake();

            StartCoroutine(StutterFadeAndLoad());
        }
    }

    private IEnumerator StutterFadeAndLoad()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null) playerController.InputBlocked = true;

        yield return new WaitForSeconds(1f);

        ScreenFader fader = ScreenFader.Instance;
        if (fader == null) fader = FindObjectOfType<ScreenFader>();

        if (fader != null)
        {
            Image fadeImage = fader.FadeImage;
            if (fadeImage != null)
            {
                Color color = fadeImage.color;

                fadeImage.color = new Color(color.r, color.g, color.b, 0.33f);
                yield return new WaitForSeconds(1.5f);

                fadeImage.color = new Color(color.r, color.g, color.b, 0.66f);
                yield return new WaitForSeconds(1.5f);

                fadeImage.color = new Color(color.r, color.g, color.b, 1f);
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(4.5f);
            }
        }
        else
        {
            yield return new WaitForSeconds(4.5f);
        }

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

        if (itemCollider != null) itemCollider.enabled = true;
        if (itemRb != null)
        {
            itemRb.simulated = true;
            itemRb.velocity = Vector2.zero;
        }
    }
}