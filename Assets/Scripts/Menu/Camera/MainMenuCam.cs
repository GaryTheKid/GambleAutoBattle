using UnityEngine;

public class MainMenuCam : MonoBehaviour
{
    [Header("Camera Shake")]
    public float shakeMagnitude = 0.05f;
    public float shakeSpeed = 1.5f;

    [Header("Lighting Flicker")]
    public Light light1;
    public Light light2;
    public float baseIntensity1 = 1f;
    public float baseIntensity2 = 1f;
    public float flickerAmount = 0.1f;
    public float flickerSpeed = 2f;

    private Vector3 originalCamPos;
    private float shakeTime;

    private void Start()
    {
        originalCamPos = transform.localPosition;
    }

    private void Update()
    {
        ApplyCameraShake();
        ApplyLightFlicker();
    }

    private void ApplyCameraShake()
    {
        shakeTime += Time.deltaTime * shakeSpeed;
        float offsetX = Mathf.PerlinNoise(shakeTime, 0f) - 0.5f;
        float offsetY = Mathf.PerlinNoise(0f, shakeTime) - 0.5f;

        transform.localPosition = originalCamPos + new Vector3(offsetX, offsetY, 0f) * shakeMagnitude;
    }

    private void ApplyLightFlicker()
    {
        if (light1 != null)
            light1.intensity = baseIntensity1 + (Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) - 0.5f) * flickerAmount;
        if (light2 != null)
            light2.intensity = baseIntensity2 + (Mathf.PerlinNoise(0f, Time.time * flickerSpeed) - 0.5f) * flickerAmount;
    }
}
