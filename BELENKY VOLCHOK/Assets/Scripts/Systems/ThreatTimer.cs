using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThreatTimer : MonoBehaviour
{
    public static ThreatTimer Instance { get; private set; }
    
    [SerializeField] private Image threatOverlay;
    [SerializeField] private float totalTime = 30f;
    [SerializeField] private float fadeStartTime = 15f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Coroutine activeTimer;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (threatOverlay != null)
        {
            Color color = threatOverlay.color;
            color.a = 0f;
            threatOverlay.color = color;
        }
    }
    
    public void StartThreatTimer(float customTime = -1, float customFadeStart = -1, System.Action onTimeout = null)
    {
        if (activeTimer != null) StopCoroutine(activeTimer);
        float duration = customTime > 0 ? customTime : totalTime;
        float fadeStart = customFadeStart > 0 ? customFadeStart : fadeStartTime;
        activeTimer = StartCoroutine(ThreatRoutine(duration, fadeStart, onTimeout));
    }
    
    public void StopThreatTimer()
    {
        if (activeTimer != null) StopCoroutine(activeTimer);
        if (threatOverlay != null)
        {
            Color color = threatOverlay.color;
            color.a = 0f;
            threatOverlay.color = color;
        }
    }
    
    private IEnumerator ThreatRoutine(float duration, float fadeStart, System.Action onTimeout)
    {
        float elapsed = 0f;
        bool fadingStarted = false;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (!fadingStarted && elapsed >= fadeStart)
                fadingStarted = true;
            if (fadingStarted && threatOverlay != null)
            {
                float t = (elapsed - fadeStart) / (duration - fadeStart);
                float alpha = fadeCurve.Evaluate(t);
                Color color = threatOverlay.color;
                color.a = alpha;
                threatOverlay.color = color;
            }
            yield return null;
        }
        onTimeout?.Invoke();
    }
}
