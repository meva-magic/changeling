using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZoneCamera : MonoBehaviour
{
    public static ZoneCamera Instance;
    
    [SerializeField] private float snapSpeed = 5f;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private Vector3 targetPosition;
    private bool isTransitioning;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (fadeImage != null)
        {
            fadeImage.color = Color.clear;
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!isTransitioning)
        {
            Vector3 target = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * snapSpeed);
        }
    }
    
    public void MoveToZone(Vector3 newPosition, bool useFade = true)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToZone(newPosition, useFade));
    }
    
    public void SnapToZone(Vector3 position)
    {
        targetPosition = position;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
    
    private IEnumerator TransitionToZone(Vector3 newPosition, bool useFade)
    {
        isTransitioning = true;
        
        if (useFade && fadeImage != null)
            yield return StartCoroutine(Fade(1f));
        
        targetPosition = newPosition;
        
        yield return new WaitForSeconds(0.1f);
        
        if (useFade && fadeImage != null)
            yield return StartCoroutine(Fade(0f));
        
        isTransitioning = false;
    }
    
    private IEnumerator Fade(float targetAlpha)
    {
        fadeImage.gameObject.SetActive(true);
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;
        Color color = fadeImage.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }
        
        fadeImage.color = new Color(color.r, color.g, color.b, targetAlpha);
        if (targetAlpha <= 0.01f) fadeImage.gameObject.SetActive(false);
    }
    
    public Coroutine FadeToBlack(float duration = -1)
    {
        return StartCoroutine(Fade(1f));
    }
    
    public Coroutine FadeFromBlack(float duration = -1)
    {
        return StartCoroutine(Fade(0f));
    }
}