using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    public Image FadeImage => fadeImage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1;
            fadeImage.color = c;
        }
    }

    private void Start()
    {
        if (fadeImage != null)
            StartCoroutine(FadeFromBlack());
    }

    public void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }

    public IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;

        float elapsed = 0;
        Color startColor = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            t = t * t * (3f - 2f * t);
            float alpha = Mathf.Lerp(startColor.a, 1, t);
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 1);
    }

    public IEnumerator FadeFromBlack()
    {
        if (fadeImage == null) yield break;

        float elapsed = 0;
        Color startColor = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            t = t * t * (3f - 2f * t);
            float alpha = Mathf.Lerp(startColor.a, 0, t);
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 0);
    }
}