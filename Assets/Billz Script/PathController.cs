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

        [Tooltip("Otomatis mulai bergerak saat Start().")]
        [SerializeField] private bool playOnStart = true;

        [Header("Target")]
        [SerializeField] private Transform targetTransform;

        [Header("Playback Status")]
        [SerializeField] private PlaybackState currentState = PlaybackState.Stopped;

        public event Action<int> OnWaypointReached;
        public event Action OnPathCompleted;
        public event Action OnPathLooped;

        private int currentSegmentIndex = 0;
        private float distanceTraveledInSegment = 0f;

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
        public float EffectiveSpeed => baseSpeed * speedMultiplier;
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
            float distanceToTravel = EffectiveSpeed * deltaTime;

            while (distanceToTravel > 0f && currentState == PlaybackState.Playing)
            {
                int maxSegments = loop ? pathData.PointCount : pathData.PointCount - 1;
                if (!loop && currentSegmentIndex >= maxSegments)
                {
                    HandlePathCompleted();
                    break;
                }

                pathData.GetBezierPoints(currentSegmentIndex, loop, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
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
                    float progress = Mathf.Clamp01(distanceTraveledInSegment / segmentDistance);

                    // Posisi kurva Bézier
                    Target.position = PathData.EvaluateCubicBezier(p0, p1, p2, p3, progress);

                    // Rotasi halus menghadap arah kurva Bézier
                    if (autoOrientToCurve)
                    {
                        Vector3 tangentDir = PathData.EvaluateCubicBezierTangent(p0, p1, p2, p3, progress);
                        if (tangentDir != Vector3.zero)
                        {
                            Target.rotation = Quaternion.LookRotation(tangentDir, Vector3.up);
                        }
                    }
                    else
                    {
                        int nextIndex = pathData.GetNextIndex(currentSegmentIndex, loop);
                        Target.rotation = Quaternion.Slerp(pathData[currentSegmentIndex].Rotation, pathData[nextIndex].Rotation, progress);
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
        }
    }
}