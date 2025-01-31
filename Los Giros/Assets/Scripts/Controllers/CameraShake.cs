using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    public float shakeIntensity = 5f;
    public float shakeDuration = 0.5f;
    private float shakeTimer;
    private bool isShaking = false;

    void Start()
    {
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera()
    {
        noise.m_AmplitudeGain = shakeIntensity;
        shakeTimer = shakeDuration;
        isShaking = true;
    }

    void Update()
    {
        if (isShaking)
        {
            if (shakeTimer > 0)
                shakeTimer -= Time.deltaTime;
            else
            {
                noise.m_AmplitudeGain = 0f; // Detener el temblor
                isShaking = false;
            }
        }
    }
}
