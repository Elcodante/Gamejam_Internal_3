using UnityEngine;
using System.Collections;

public class CarVisualReaction : MonoBehaviour
{
    public static CarVisualReaction instance;

    public enum SpinAxis
    {
        Y_Axis, // Putar mendatar di aspal (Flat Spin / Donut)
        Z_Axis, // Putar guling ke samping (Barrel Roll)
        X_Axis  // Salto depan/belakang (Flip)
    }

    [Header("--- 360 SPIN SETTINGS (Z & > .) ---")]
    [Tooltip("Pilih sumbu putaran 360: Pilih Z_Axis jika ingin guling samping, atau Y_Axis jika ingin putar aspal.")]
    public SpinAxis spinAxis = SpinAxis.Z_Axis; // Default kita arahkan ke sumbu Z sesuai keinginanmu

    [Tooltip("Durasi waktu (detik) untuk menyelesaikan 1 kali putaran 360.")]
    public float spinDuration = 0.45f;

    [Tooltip("Efek mobil sedikit melompat saat berputar 360.")]
    public float spinHopHeight = 0.2f;

    [Header("--- TILT & STEER SETTINGS (X, C, M, < ,) ---")]
    public float frontSteerAngle = 12f;
    public float bodyRollAngle = 8f;
    public float pitchAngle = 10f;

    [Header("--- SHIFT & SUSPENSION (GESERAN) ---")]
    public float frontSwayDistance = 0.15f;
    public float liftDistance = 0.1f;
    public float hitSnapSpeed = 35f;
    public float returnSpeed = 12f;

    [Header("--- PULSE / BREATHING (MENGEMBANG MENGEMPIS) ---")]
    public bool enablePulse = true;
    public float pulseSpeed = 6f;
    [Range(0.01f, 0.25f)]
    public float pulseIntensity = 0.06f;
    public bool squashAndStretch = true;

    // Variabel kalkulasi reaksi
    private float targetYaw = 0f;
    private float targetPitch = 0f;
    private float targetRoll = 0f;
    private float targetSway = 0f;
    private float targetLift = 0f;

    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float currentSway = 0f;
    private float currentLift = 0f;

    // Nilai putaran 360
    private float spinAngleOffset = 0f;
    private float spinLiftOffset = 0f;
    private Coroutine spinRoutine;

    // Cache orientasi awal (menjaga rotasi bawaan seperti X = -90)
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;
    private Vector3 initialLocalScale;

    private void Awake()
    {
        if (instance == null) instance = this;

        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;
        initialLocalScale = transform.localScale;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1. Interpolasi reaksi biasa (X, C, M, ,)
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, dt * hitSnapSpeed);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, dt * hitSnapSpeed);
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, dt * hitSnapSpeed);
        currentSway = Mathf.Lerp(currentSway, targetSway, dt * hitSnapSpeed);
        currentLift = Mathf.Lerp(currentLift, targetLift, dt * hitSnapSpeed);

        // 2. Efek Pegas kembali ke normal
        targetYaw = Mathf.Lerp(targetYaw, 0f, dt * returnSpeed);
        targetPitch = Mathf.Lerp(targetPitch, 0f, dt * returnSpeed);
        targetRoll = Mathf.Lerp(targetRoll, 0f, dt * returnSpeed);
        targetSway = Mathf.Lerp(targetSway, 0f, dt * returnSpeed);
        targetLift = Mathf.Lerp(targetLift, 0f, dt * returnSpeed);

        // 3. Terapkan putaran 360 derajat ke sumbu yang dipilih di Inspector
        float finalPitch = currentPitch;
        float finalYaw = currentYaw;
        float finalRoll = currentRoll;

        switch (spinAxis)
        {
            case SpinAxis.Z_Axis:
                finalRoll += spinAngleOffset; // Putar guling pada sumbu Z!
                break;
            case SpinAxis.Y_Axis:
                finalYaw += spinAngleOffset;  // Putar mendatar pada sumbu Y
                break;
            case SpinAxis.X_Axis:
                finalPitch += spinAngleOffset; // Salto depan pada sumbu X
                break;
        }

        Quaternion reactionRot = Quaternion.Euler(finalPitch, finalYaw, finalRoll);
        transform.localRotation = initialLocalRotation * reactionRot;

        // 4. Posisi lokal + lompatan saat berputar
        float totalLift = currentLift + spinLiftOffset;
        transform.localPosition = initialLocalPosition + new Vector3(currentSway, totalLift, 0f);

        // 5. Animasi mengembang-mengempis
        ApplyPulseEffect();
    }

    private void ApplyPulseEffect()
    {
        if (!enablePulse)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, initialLocalScale, Time.deltaTime * 10f);
            return;
        }

        float sinWave = Mathf.Sin(Time.time * pulseSpeed);
        Vector3 scaleMultiplier;

        if (squashAndStretch)
        {
            float yChange = sinWave * pulseIntensity;
            float xzChange = -sinWave * (pulseIntensity * 0.4f);
            scaleMultiplier = new Vector3(1f + xzChange, 1f + yChange, 1f + xzChange);
        }
        else
        {
            float uniformChange = sinWave * pulseIntensity;
            scaleMultiplier = Vector3.one * (1f + uniformChange);
        }

        transform.localScale = Vector3.Scale(initialLocalScale, scaleMultiplier);
    }

    public void TriggerLaneReaction(int laneIndex)
    {
        switch (laneIndex)
        {
            // Z -> Putar 360 ke Kiri
            case 0:
                StartSpin(-1f);
                break;

            // X -> Depan ke Kiri
            case 1:
                targetSway = -frontSwayDistance;
                targetYaw = -frontSteerAngle;
                targetRoll = bodyRollAngle * 0.7f;
                break;

            // C -> Depan Naik / Belakang Turun
            case 2:
                targetPitch = -pitchAngle;
                targetLift = liftDistance * 0.5f;
                break;

            // M -> Depan Turun / Belakang Naik
            case 3:
                targetPitch = pitchAngle;
                targetLift = -liftDistance * 0.5f;
                break;

            // , / < -> Depan ke Kanan
            case 4:
                targetSway = frontSwayDistance;
                targetYaw = frontSteerAngle;
                targetRoll = -bodyRollAngle * 0.7f;
                break;

            // . / > -> Putar 360 ke Kanan
            case 5:
                StartSpin(1f);
                break;
        }
    }

    private void StartSpin(float direction)
    {
        if (spinRoutine != null)
        {
            StopCoroutine(spinRoutine);
        }
        spinRoutine = StartCoroutine(SpinRoutine(direction));
    }

    private IEnumerator SpinRoutine(float direction)
    {
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Putar 360 derajat
            spinAngleOffset = direction * 360f * smoothT;

            // Mobil melompat sedikit saat berputar
            spinLiftOffset = Mathf.Sin(smoothT * Mathf.PI) * spinHopHeight;

            yield return null;
        }

        spinAngleOffset = 0f;
        spinLiftOffset = 0f;
        spinRoutine = null;
    }
}