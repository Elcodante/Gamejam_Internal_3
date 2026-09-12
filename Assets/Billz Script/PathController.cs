using System;
using UnityEngine;

namespace PathSystem.Runtime
{
    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused
    }

    [DisallowMultipleComponent]
    public class PathController : MonoBehaviour
    {
        [Header("Path Configuration")]
        [SerializeField] private PathData pathData = new PathData();

        [Header("Movement Settings")]
        [Tooltip("Kecepatan dasar pergerakan (unit dunia per detik).")]
        [Min(0.01f)]
        [SerializeField] private float baseSpeed = 8f;

        [Tooltip("Pengali kecepatan.")]
        [Min(0f)]
        [SerializeField] private float speedMultiplier = 1f;

        [Tooltip("Otomatis membuat rotasi mobil menghadap ke arah lengkungan kurva.")]
        [SerializeField] private bool autoOrientToCurve = true;

        [Tooltip("Apakah pergerakan akan mengulang kembali ke awal?")]
        [SerializeField] private bool loop = true;

        // KUNCI: Ubah default menjadi false agar tidak mendahului countdown!
        [Tooltip("Otomatis mulai bergerak saat Start().")]
        [SerializeField] private bool playOnStart = false;

        [Header("--- SPEED MODES ---")]
        [Tooltip("Kecepatan di tikungan dan jalan lurus 100% konstan.")]
        [SerializeField] private bool useConstantSpeed = true;

        [Tooltip("Mobil otomatis mengerem halus saat melewati tikungan tajam.")]
        [SerializeField] private bool autoBrakeOnCorners = false;

        [Range(0.1f, 1f)]
        [SerializeField] private float minCornerSpeedRatio = 0.5f;

        [Range(15f, 90f)]
        [SerializeField] private float sharpCornerAngle = 55f;

        [Range(1f, 20f)]
        [SerializeField] private float brakingSmoothness = 6f;

        [Header("--- DRIFT SETTINGS ---")]
        [Tooltip("Aktifkan fitur visual drifting.")]
        [SerializeField] private bool enableDrifting = true;

        [Tooltip("Otomatis drifting saat mendeteksi tikungan tajam.")]
        [SerializeField] private bool autoDriftOnCorners = true;

        [Tooltip("Seberapa jauh (dalam meter) mobil mengintip tikungan di depannya untuk ancang-ancang.")]
        [Range(1.5f, 12f)]
        [SerializeField] private float lookaheadDistance = 4.5f;

        [Tooltip("Sudut belokan minimum (derajat) untuk mulai memicu drifting.")]
        [Range(3f, 30f)]
        [SerializeField] private float driftCornerThreshold = 8f;

        [Tooltip("Maksimum sudut buangan bodi mobil saat drifting (Yaw Angle).")]
        [Range(10f, 65f)]
        [SerializeField] private float maxDriftAngle = 36f;

        [Tooltip("Kemiringan bodi mobil akibat suspensi (Roll Angle).")]
        [Range(0f, 8f)]
        [SerializeField] private float bodyRollAngle = 3.5f;

        [Tooltip("Jarak terseret ke sisi luar lintasan saat drifting.")]
        [Range(0f, 2.5f)]
        [SerializeField] private float outwardSlideDistance = 0.6f;

        [Header("--- DRIFT SMOOTHNESS (INERTIA) ---")]
        [Tooltip("Waktu (detik) bagi mobil untuk masuk ke posisi drift secara luwes. Semakin besar, semakin halus (tidak menyentak).")]
        [Range(0.05f, 0.6f)]
        [SerializeField] private float driftEnterSmoothTime = 0.22f;

        [Tooltip("Waktu (detik) bagi mobil meluruskan bodi saat keluar dari drift.")]
        [Range(0.05f, 0.6f)]
        [SerializeField] private float driftExitSmoothTime = 0.28f;

        [Header("Target")]
        [SerializeField] private Transform targetTransform;

        [Header("Playback Status")]
        [SerializeField] private PlaybackState currentState = PlaybackState.Stopped;

        public event Action<int> OnWaypointReached;
        public event Action OnPathCompleted;
        public event Action OnPathLooped;
        public event Action<bool, float> OnDriftChanged;

        private int currentSegmentIndex = 0;
        private float distanceTraveledInSegment = 0f;
        private float currentBrakeFactor = 1f;

        private float currentDriftAngle = 0f;
        private float driftAngleVelocity = 0f;

        private float currentSlideDistance = 0f;
        private float slideVelocity = 0f;

        private bool isDrifting = false;
        private bool manualDriftOverride = false;
        private float manualDriftDir = 0f;
        private float manualDriftIntensity = 1f;

        public PathData Path => pathData;
        public float BaseSpeed
        {
            get => baseSpeed;
            set => baseSpeed = Mathf.Max(0.01f, value);
        }
        public float SpeedMultiplier
        {
            get => speedMultiplier;
            set => speedMultiplier = Mathf.Max(0f, value);
        }
        public bool EnableDrifting
        {
            get => enableDrifting;
            set => enableDrifting = value;
        }
        public bool AutoDriftOnCorners
        {
            get => autoDriftOnCorners;
            set => autoDriftOnCorners = value;
        }
        public bool IsDrifting => isDrifting;
        public float CurrentDriftAngle => currentDriftAngle;
        public float EffectiveSpeed => baseSpeed * speedMultiplier * (autoBrakeOnCorners ? currentBrakeFactor : 1f);
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }
        public bool PlayOnStart
        {
            get => playOnStart;
            set => playOnStart = value;
        }
        public PlaybackState CurrentState => currentState;
        public Transform Target => targetTransform != null ? targetTransform : transform;
        public int CurrentSegmentIndex => currentSegmentIndex;

        private void Awake()
        {
            if (targetTransform == null) targetTransform = transform;
        }

        private void Start()
        {
            if (playOnStart && pathData.IsValid()) Play();
        }

        private void Update()
        {
            if (currentState != PlaybackState.Playing) return;
            if (!pathData.IsValid()) return;

            UpdateMovement(Time.deltaTime);
        }

        private void UpdateMovement(float deltaTime)
        {
            pathData.EvaluateByDistance(currentSegmentIndex, loop, distanceTraveledInSegment, out _, out Vector3 currentTangent);

            float actualLookahead = Mathf.Max(lookaheadDistance, baseSpeed * 0.4f);
            pathData.EvaluateLookahead(currentSegmentIndex, distanceTraveledInSegment, actualLookahead, loop, out _, out Vector3 aheadTangent);

            float signedAngle = Vector3.SignedAngle(currentTangent, aheadTangent, Vector3.up);
            float cornerAngle = Mathf.Abs(signedAngle);

            if (autoBrakeOnCorners)
            {
                float cornerSharpness = Mathf.Clamp01(cornerAngle / sharpCornerAngle);
                float targetBrakeRatio = Mathf.Lerp(1f, minCornerSpeedRatio, cornerSharpness);
                currentBrakeFactor = Mathf.MoveTowards(currentBrakeFactor, targetBrakeRatio, brakingSmoothness * deltaTime);
            }
            else
            {
                currentBrakeFactor = 1f;
            }

            if (enableDrifting)
            {
                UpdateDriftSmoothDamp(signedAngle, deltaTime);
            }
            else
            {
                currentDriftAngle = 0f;
                currentSlideDistance = 0f;
                SetDriftState(false, 0f);
            }

            float distanceToTravel = EffectiveSpeed * deltaTime;

            while (distanceToTravel > 0f && currentState == PlaybackState.Playing)
            {
                int maxSegments = loop ? pathData.PointCount : pathData.PointCount - 1;
                if (!loop && currentSegmentIndex >= maxSegments)
                {
                    HandlePathCompleted();
                    break;
                }

                float segmentDistance = pathData.GetSegmentDistance(currentSegmentIndex, loop);

                if (segmentDistance <= Mathf.Epsilon)
                {
                    AdvanceToNextSegment();
                    continue;
                }

                float remainingInSegment = segmentDistance - distanceTraveledInSegment;

                if (distanceToTravel < remainingInSegment)
                {
                    distanceTraveledInSegment += distanceToTravel;

                    Vector3 targetPos;
                    Vector3 tangentDir;

                    if (useConstantSpeed)
                    {
                        pathData.EvaluateByDistance(currentSegmentIndex, loop, distanceTraveledInSegment, out targetPos, out tangentDir);
                    }
                    else
                    {
                        float progress = Mathf.Clamp01(distanceTraveledInSegment / segmentDistance);
                        pathData.GetBezierPoints(currentSegmentIndex, loop, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
                        targetPos = PathData.EvaluateCubicBezier(p0, p1, p2, p3, progress);
                        tangentDir = PathData.EvaluateCubicBezierTangent(p0, p1, p2, p3, progress);
                    }

                    if (enableDrifting && Mathf.Abs(currentSlideDistance) > 0.001f && tangentDir != Vector3.zero)
                    {
                        Vector3 rightVector = Vector3.Cross(Vector3.up, tangentDir).normalized;
                        Target.position = targetPos + (-rightVector * currentSlideDistance);
                    }
                    else
                    {
                        Target.position = targetPos;
                    }

                    if (tangentDir != Vector3.zero)
                    {
                        Quaternion baseRotation = Quaternion.LookRotation(tangentDir, Vector3.up);

                        if (enableDrifting && Mathf.Abs(currentDriftAngle) > 0.01f)
                        {
                            float normalizedDrift = Mathf.Clamp(currentDriftAngle / maxDriftAngle, -1f, 1f);
                            float rollTilt = -normalizedDrift * bodyRollAngle;

                            Quaternion driftRotation = Quaternion.Euler(0f, currentDriftAngle, rollTilt);
                            Target.rotation = baseRotation * driftRotation;
                        }
                        else if (autoOrientToCurve)
                        {
                            Target.rotation = baseRotation;
                        }
                    }

                    distanceToTravel = 0f;
                }
                else
                {
                    distanceToTravel -= remainingInSegment;
                    int nextIndex = pathData.GetNextIndex(currentSegmentIndex, loop);
                    Target.position = pathData[nextIndex].Position;
                    AdvanceToNextSegment();
                }
            }
        }

        private void UpdateDriftSmoothDamp(float signedAngle, float deltaTime)
        {
            float targetAngle = 0f;
            float absAngle = Mathf.Abs(signedAngle);

            if (manualDriftOverride)
            {
                targetAngle = manualDriftDir * maxDriftAngle * manualDriftIntensity;
            }
            else if (autoDriftOnCorners && absAngle >= driftCornerThreshold)
            {
                float t = Mathf.Clamp01((absAngle - driftCornerThreshold) / (sharpCornerAngle - driftCornerThreshold));
                float smoothT = t * t * (3f - 2f * t);

                targetAngle = Mathf.Sign(signedAngle) * Mathf.Lerp(0f, maxDriftAngle, smoothT);
            }

            bool isEnteringDrift = Mathf.Abs(targetAngle) > Mathf.Abs(currentDriftAngle);
            float smoothTime = isEnteringDrift ? driftEnterSmoothTime : driftExitSmoothTime;

            currentDriftAngle = Mathf.SmoothDamp(currentDriftAngle, targetAngle, ref driftAngleVelocity, smoothTime, Mathf.Infinity, deltaTime);

            float targetSlide = (currentDriftAngle / maxDriftAngle) * outwardSlideDistance;
            currentSlideDistance = Mathf.SmoothDamp(currentSlideDistance, targetSlide, ref slideVelocity, smoothTime, Mathf.Infinity, deltaTime);

            bool nowDrifting = Mathf.Abs(currentDriftAngle) > (maxDriftAngle * 0.25f);
            if (nowDrifting != isDrifting)
            {
                SetDriftState(nowDrifting, Mathf.Sign(currentDriftAngle));
            }
        }

        private void SetDriftState(bool drifting, float dir)
        {
            isDrifting = drifting;
            OnDriftChanged?.Invoke(isDrifting, dir);
        }

        public void TriggerManualDrift(float direction, float intensity = 1f)
        {
            manualDriftOverride = true;
            manualDriftDir = Mathf.Sign(direction);
            manualDriftIntensity = Mathf.Clamp01(intensity);
        }

        public void StopManualDrift()
        {
            manualDriftOverride = false;
        }

        private void AdvanceToNextSegment()
        {
            distanceTraveledInSegment = 0f;
            currentSegmentIndex++;

            if (currentSegmentIndex >= pathData.PointCount)
            {
                if (loop)
                {
                    currentSegmentIndex = 0;
                    OnPathLooped?.Invoke();
                    OnWaypointReached?.Invoke(0);
                }
                else
                {
                    HandlePathCompleted();
                }
            }
            else
            {
                OnWaypointReached?.Invoke(currentSegmentIndex);
            }
        }

        private void HandlePathCompleted()
        {
            Target.position = pathData[pathData.PointCount - 1].Position;
            Stop();
            OnPathCompleted?.Invoke();
        }

        public virtual void Play()
        {
            if (!pathData.IsValid()) return;
            if (currentState == PlaybackState.Stopped && currentSegmentIndex >= pathData.PointCount - 1 && !loop)
            {
                ResetPlayback();
            }
            currentState = PlaybackState.Playing;
        }

        public virtual void Pause() => currentState = PlaybackState.Paused;
        public virtual void Stop() => currentState = PlaybackState.Stopped;

        public virtual void ResetPlayback()
        {
            Stop();
            currentSegmentIndex = 0;
            distanceTraveledInSegment = 0f;
            currentBrakeFactor = 1f;

            currentDriftAngle = 0f;
            driftAngleVelocity = 0f;
            currentSlideDistance = 0f;
            slideVelocity = 0f;
            manualDriftOverride = false;
            SetDriftState(false, 0f);

            if (pathData != null && pathData.PointCount > 0)
            {
                Target.position = pathData[0].Position;
                if (pathData.PointCount >= 2 && autoOrientToCurve)
                {
                    pathData.GetBezierPoints(0, loop, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
                    Vector3 dir = PathData.EvaluateCubicBezierTangent(p0, p1, p2, p3, 0f);
                    if (dir != Vector3.zero) Target.rotation = Quaternion.LookRotation(dir, Vector3.up);
                }
                else
                {
                    Target.rotation = pathData[0].Rotation;
                }
            }
        }

        private void OnValidate()
        {
            baseSpeed = Mathf.Max(0.01f, baseSpeed);
            speedMultiplier = Mathf.Max(0f, speedMultiplier);
            brakingSmoothness = Mathf.Max(0.1f, brakingSmoothness);
            driftEnterSmoothTime = Mathf.Max(0.01f, driftEnterSmoothTime);
            driftExitSmoothTime = Mathf.Max(0.01f, driftExitSmoothTime);
        }
    }
}