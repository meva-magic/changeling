using UnityEngine;
using UnityEngine.UI;

public class CreditsAutoScroll : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollSpeed = 30f;
    [SerializeField] private bool autoStart = true;

    private bool isScrolling;

    private void Start()
    {
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
        if (autoStart) StartScrolling();
    }

    private void Update()
    {
        if (!isScrolling || scrollRect == null) return;
        scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime / 1000f;
        if (scrollRect.verticalNormalizedPosition <= 0f) StopScrolling();
    }

    public void StartScrolling()
    {
        isScrolling = true;
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }

    public void StopScrolling() { isScrolling = false; }
    public void ResetScroll()
    {
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
        isScrolling = false;
    }
}
