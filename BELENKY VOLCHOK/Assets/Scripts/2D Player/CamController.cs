using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZoneCamera : MonoBehaviour
{
    public static ZoneCamera Instance;
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private Vector3 targetPosition;
    
    private void Awake()
    {
        Instance = this;
        
        if (fadeImage != null)
        {
            fadeImage.color = Color.clear;
            fadeImage.gameObject.SetActive(true);
        }
        
        targetPosition = transform.position;
        Debug.Log("ZoneCamera initialized");
    }
    
    public void MoveToZone(Vector3 newPosition, bool useFade = true)
    {
        Debug.Log($"MoveToZone called: {newPosition}");
        StopAllCoroutines();
        StartCoroutine(TransitionToZone(newPosition, useFade));
    }
    
    private IEnumerator TransitionToZone(Vector3 newPosition, bool useFade)
    {
        Debug.Log($"Starting transition to: {newPosition}");
        
        // Fade to black
        if (useFade && fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadeImage.color = Color.black;
        }
        
        // Snap camera
        targetPosition = newPosition;
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        Debug.Log($"Camera snapped to: {transform.position}");
        
        yield return new WaitForSeconds(0.1f);
        
        // Fade back
        if (useFade && fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadeImage.color = Color.clear;
        }
        
        Debug.Log("Transition complete");
    }
    
    public Coroutine FadeToBlack()
    {
        return StartCoroutine(FadeRoutine(1));
    }
    
    public Coroutine FadeFromBlack()
    {
        return StartCoroutine(FadeRoutine(0));
    }
    
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (fadeImage == null) yield break;
        
        fadeImage.gameObject.SetActive(true);
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, targetAlpha);
        if (targetAlpha <= 0.01f) fadeImage.gameObject.SetActive(false);
    }
}