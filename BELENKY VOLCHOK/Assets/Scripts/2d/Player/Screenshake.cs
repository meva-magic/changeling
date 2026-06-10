using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Single Shake")]
    [SerializeField] private float singleIntensity = 2f;
    [SerializeField] private float singleSpeed = 20f;
    [SerializeField] private float singleDuration = 0.3f;

    [Header("Infinite Shake")]
    [SerializeField] private float infiniteIntensity = 1f;
    [SerializeField] private float infiniteRoughness = 10f;
    [SerializeField] private float infiniteFadeInTime = 0.5f;

    private Coroutine infiniteCoroutine;
    private bool infiniteActive;
    private float originalZ;
    private Vector3 startPosition;

    private void Start()
    {
        originalZ = transform.eulerAngles.z;
        startPosition = transform.position;
    }

    public void ShakeOnce()
    {
        StartCoroutine(SingleShakeRoutine());
    }

    private IEnumerator SingleShakeRoutine()
    {
        float elapsed = 0;

        while (elapsed < singleDuration)
        {
            elapsed += Time.deltaTime;
            float z = originalZ + Mathf.Sin(Time.time * singleSpeed) * singleIntensity;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
            yield return null;
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, originalZ);
    }

    public void StartInfiniteShake()
    {
        if (infiniteActive) return;
        infiniteActive = true;
        infiniteCoroutine = StartCoroutine(InfiniteShakeRoutine());
    }

    private IEnumerator InfiniteShakeRoutine()
    {
        float elapsed = 0;
        float currentIntensity = 0;

        while (infiniteActive)
        {
            elapsed += Time.deltaTime;

            if (elapsed < infiniteFadeInTime)
                currentIntensity = Mathf.Lerp(0, infiniteIntensity, elapsed / infiniteFadeInTime);
            else
                currentIntensity = infiniteIntensity;

            float x = (Mathf.PerlinNoise(Time.time * infiniteRoughness, 0) * 2 - 1) * currentIntensity;
            float y = (Mathf.PerlinNoise(0, Time.time * infiniteRoughness) * 2 - 1) * currentIntensity;
            float z = (Mathf.PerlinNoise(Time.time * infiniteRoughness * 0.5f, Time.time * infiniteRoughness * 0.5f) * 2 - 1) * currentIntensity * 0.3f;

            transform.position = startPosition + new Vector3(x, y, 0);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, originalZ + z);

            yield return null;
        }
    }

    public void StopInfiniteShake()
    {
        infiniteActive = false;

        if (infiniteCoroutine != null)
        {
            StopCoroutine(infiniteCoroutine);
            infiniteCoroutine = null;
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, originalZ);
        transform.position = startPosition;
    }
}