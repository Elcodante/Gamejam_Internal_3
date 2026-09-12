using UnityEngine;
using Unity.Cinemachine;

namespace PathSystem.Runtime
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class DriftCameraJuice : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PathController pathController;

        [Header("Dynamic Dutch (Camera Roll/Tilt)")]
        [Tooltip("Derajat kamera ikut miring saat mobil drifting.")]
        [Range(0f, 8f)]
        [SerializeField] private float maxDutchTilt = 3.5f;

        [Header("Dynamic FOV Kick")]
        [Tooltip("Pertambahan FOV saat mobil sedang drifting tajam.")]
        [Range(0f, 15f)]
        [SerializeField] private float driftFovBonus = 6f;

        [Range(1f, 15f)]
        [SerializeField] private float juiceSmoothness = 6f;

        private CinemachineCamera cineCam;
        private float baseFov;
        private float currentDutch = 0f;
        private float currentFovOffset = 0f;

        private void Awake()
        {
            cineCam = GetComponent<CinemachineCamera>();
            baseFov = cineCam.Lens.FieldOfView;
        }

        private void LateUpdate()
        {
            if (pathController == null || cineCam == null) return;

            // Hitung persentase drift mobil (-1.0 s/d 1.0)
            float driftRatio = pathController.CurrentDriftAngle / 45f;
            float absRatio = Mathf.Clamp01(Mathf.Abs(driftRatio));

            // 1. Dynamic Dutch: Miringkan kamera berlawanan/searah gaya sentrifugal
            float targetDutch = -driftRatio * maxDutchTilt;
            currentDutch = Mathf.Lerp(currentDutch, targetDutch, juiceSmoothness * Time.deltaTime);

            // 2. Dynamic FOV: Lebarkan lensa saat drifting
            float targetFov = absRatio * driftFovBonus;
            currentFovOffset = Mathf.Lerp(currentFovOffset, targetFov, juiceSmoothness * Time.deltaTime);

            // Terapkan ke Cinemachine
            cineCam.Lens.Dutch = currentDutch;
            cineCam.Lens.FieldOfView = baseFov + currentFovOffset;
        }
    }
}