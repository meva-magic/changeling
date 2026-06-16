using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeToBlack : MonoBehaviour
{
    public static FadeToBlack Instance { get; private set; }
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (fadeImage == null)
        {
            Debug.LogError("FadeToBlack: fadeImage не назначен в инспекторе!");
            return;
        }
        
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(true);
    }
    
    public void FadeOut(System.Action onComplete = null)
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeToBlack: fadeImage равен null!");
            onComplete?.Invoke();
            return;
        }
        
        StartCoroutine(FadeOutRoutine(onComplete));
    }
    
    private IEnumerator FadeOutRoutine(System.Action onComplete)
    {
        if (fadeImage == null) yield break;
        
        // Убеждаемся, что объект активен
        fadeImage.gameObject.SetActive(true);
        
        float elapsed = 0f;
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        fadeImage.color = color;
        
        onComplete?.Invoke();
    }
    
    public void FadeIn(System.Action onComplete = null)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        StartCoroutine(FadeInRoutine(onComplete));
    }
    
    private IEnumerator FadeInRoutine(System.Action onComplete)
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
        
        onComplete?.Invoke();
    }
}