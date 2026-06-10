using UnityEngine;
using System.Collections;

public class CameraShake3D : MonoBehaviour
{
    [Header("Single Shake")]
    [SerializeField] private float singleIntensity = 0.2f;
    [SerializeField] private float singleRoughness = 20f;
    [SerializeField] private float singleDuration = 0.3f;

    [Header("Infinite Shake")]
    [SerializeField] private float infiniteIntensity = 0.1f;
    [SerializeField] private float infiniteRoughness = 10f;
    [SerializeField] private float infiniteFadeInTime = 0.5f;

    [Header("Test Buttons")]
    [SerializeField] private bool testSingleShake = false;
    [SerializeField] private bool startInfiniteShake = false;
    [SerializeField] private bool stopInfiniteShake = false;

    private Coroutine infiniteCoroutine;
    private bool infiniteActive;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    private void OnValidate()
    {
        if (testSingleShake)
        {
            testSingleShake = false;
            ShakeOnce();
        }

        if (startInfiniteShake)
        {
            startInfiniteShake = false;
            StartInfiniteShake();
        }

        if (stopInfiniteShake)
        {
            stopInfiniteShake = false;
            StopInfiniteShake();
        }
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

            float x = (Mathf.PerlinNoise(Time.time * singleRoughness, 0) * 2 - 1) * singleIntensity;
            float y = (Mathf.PerlinNoise(0, Time.time * singleRoughness) * 2 - 1) * singleIntensity;
            float z = (Mathf.PerlinNoise(Time.time * singleRoughness * 0.5f, Time.time * singleRoughness * 0.5f) * 2 - 1) * singleIntensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            transform.localRotation = originalRotation * Quaternion.Euler(z * 2, z * 2, 0);

            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
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
            float z = (Mathf.PerlinNoise(Time.time * infiniteRoughness * 0.5f, Time.time * infiniteRoughness * 0.5f) * 2 - 1) * currentIntensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            transform.localRotation = originalRotation * Quaternion.Euler(z * 3, z * 3, 0);

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

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
}