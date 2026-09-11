using System;
using UnityEngine;

namespace PathSystem.Runtime
{
    [Serializable]
    public class PathPointData
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private Vector3 handle; // Offset kontrol kurva Bézier

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        public Vector3 Handle
        {
            get => handle;
            set => handle = value;
        }

        public PathPointData()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            handle = Vector3.zero;
        }

        public PathPointData(Vector3 position, Quaternion rotation, Vector3 handle = default)
        {
            this.position = position;
            this.rotation = rotation;
            this.handle = handle;
        }
    }
}