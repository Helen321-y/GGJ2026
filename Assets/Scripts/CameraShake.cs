using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private float defaultFrequency = 2f;

    [Header("Debug (optional)")]
    [SerializeField] private bool logDebug = false;

    private CinemachineBrain brain;

    private CinemachineVirtualCamera cachedVcam;
    private CinemachineBasicMultiChannelPerlin cachedPerlin;

    private float shakeTimer;
    private float shakeDuration;
    private float startAmplitude;
    private float startFrequency;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        brain = GetComponent<CinemachineBrain>();
        if (brain == null && logDebug)
            Debug.LogError("CameraShake: This script must be on the Main Camera with a CinemachineBrain.");
    }

    /// <summary>
    /// amplitude: Perlin AmplitudeGain
    /// durationSeconds: shake duration
    /// frequency: Perlin FrequencyGain (<= 0 will use defaultFrequency)
    /// </summary>
    public void Shake(float amplitude, float durationSeconds, float frequency = -1f)
    {
        if (brain == null) return;

        CacheLivePerlin();
        if (cachedPerlin == null)
        {
            if (logDebug)
                Debug.LogWarning("CameraShake: No CinemachineBasicMultiChannelPerlin found on the live virtual camera.");
            return;
        }

        float freq = (frequency > 0f) ? frequency : defaultFrequency;

        startAmplitude = Mathf.Max(0f, amplitude);
        startFrequency = Mathf.Max(0f, freq);

        cachedPerlin.m_FrequencyGain = startFrequency;
        cachedPerlin.m_AmplitudeGain = startAmplitude;

        shakeDuration = Mathf.Max(0.01f, durationSeconds);
        shakeTimer = shakeDuration;
    }

    public void StopShake()
    {
        shakeTimer = 0f;
        if (cachedPerlin != null)
            cachedPerlin.m_AmplitudeGain = 0f;
    }

    private void Update()
    {
        if (shakeTimer <= 0f) return;

        CacheLivePerlin();
        if (cachedPerlin == null)
        {
            shakeTimer = 0f;
            return;
        }

        shakeTimer -= Time.deltaTime;

        float t = 1f - (shakeTimer / shakeDuration); // 0 -> 1
        cachedPerlin.m_AmplitudeGain = Mathf.Lerp(startAmplitude, 0f, t);

        if (shakeTimer <= 0f)
            cachedPerlin.m_AmplitudeGain = 0f;
    }

    private void CacheLivePerlin()
    {
        ICinemachineCamera liveCam = brain.ActiveVirtualCamera;
        if (liveCam == null)
        {
            cachedVcam = null;
            cachedPerlin = null;
            return;
        }

        GameObject liveGo = liveCam.VirtualCameraGameObject;
        if (liveGo == null)
        {
            cachedVcam = null;
            cachedPerlin = null;
            return;
        }

        // Common case: CinemachineVirtualCamera
        CinemachineVirtualCamera vcam = liveGo.GetComponent<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            if (cachedVcam != vcam)
            {
                cachedVcam = vcam;
                cachedPerlin = cachedVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
            return;
        }

        // Fallback: try to find Perlin on the live camera object hierarchy
        cachedVcam = null;
        cachedPerlin = liveGo.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }
}
