using UnityEngine;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    
    private void Start()
    {
        if (fadeImage == null) return;
        fadeImage.color = Color.black;
        StartCoroutine(FadeInRoutine());
    }
    
    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
}
