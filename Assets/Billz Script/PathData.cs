using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathSystem.Runtime
{
    [Serializable]
    public class PathData
    {
        [SerializeField] private List<PathPointData> points = new List<PathPointData>();

        public IReadOnlyList<PathPointData> Points => points;
        public int PointCount => points.Count;

        public PathPointData this[int index]
        {
            get
            {
                if (index < 0 || index >= points.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return points[index];
            }
        }

        public void AddPoint(PathPointData point)
        {
            if (point != null) points.Add(point);
        }

        public void AddPoint(Vector3 position, Quaternion rotation, Vector3 handle = default)
        {
            points.Add(new PathPointData(position, rotation, handle));
        }

        public bool RemovePointAt(int index)
        {
            if (index < 0 || index >= points.Count) return false;
            points.RemoveAt(index);
            return true;
        }

        public bool RemoveLastPoint()
        {
            if (points.Count == 0) return false;
            points.RemoveAt(points.Count - 1);
            return true;
        }

        public void Clear()
        {
            points.Clear();
        }

        public bool IsValid() => points != null && points.Count >= 2;

        public int GetNextIndex(int currentIndex, bool isLooping)
        {
            int next = currentIndex + 1;
            if (next >= points.Count)
            {
                return isLooping ? 0 : points.Count - 1;
            }
            return next;
        }

        public void GetBezierPoints(int startIndex, bool isLooping, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
        {
            int nextIndex = GetNextIndex(startIndex, isLooping);

            p0 = points[startIndex].Position;
            p1 = p0 + points[startIndex].Handle;

            p3 = points[nextIndex].Position;
            p2 = p3 - points[nextIndex].Handle;
        }

        public static Vector3 EvaluateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            return (uu * u * p0) + (3f * uu * t * p1) + (3f * u * tt * p2) + (tt * t * p3);
        }

        public static Vector3 EvaluateCubicBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            Vector3 tangent = (3f * u * u * (p1 - p0)) + (6f * u * t * (p2 - p1)) + (3f * t * t * (p3 - p2));
            return tangent.normalized;
        }

        public float GetSegmentDistance(int startIndex, bool isLooping, int sampleCount = 12)
        {
            if (points == null || points.Count < 2) return 0f;
            if (startIndex < 0 || startIndex >= points.Count) return 0f;
            if (!isLooping && startIndex >= points.Count - 1) return 0f;

            GetBezierPoints(startIndex, isLooping, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);

            // Jika handle bernilai 0, jarak Euclidean lurus cukup
            if (points[startIndex].Handle == Vector3.zero && points[GetNextIndex(startIndex, isLooping)].Handle == Vector3.zero)
            {
                return Vector3.Distance(p0, p3);
            }

            // Integrasi numerik panjang kurva Bézier
            float length = 0f;
            Vector3 lastPoint = p0;
            for (int i = 1; i <= sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                Vector3 currentPoint = EvaluateCubicBezier(p0, p1, p2, p3, t);
                length += Vector3.Distance(lastPoint, currentPoint);
                lastPoint = currentPoint;
            }

            return length;
        }
    }
}