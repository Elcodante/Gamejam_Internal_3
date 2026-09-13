using UnityEngine;
using UnityEditor;
using PathSystem.Runtime;

namespace PathSystem.Editor
{
    [CustomEditor(typeof(PathController))]
    public class PathControllerEditor : UnityEditor.Editor
    {
        private PathController controller;
        private int selectedPointIndex = -1;

        // Variabel untuk interaksi Click & Drag ala Krita
        private bool isCreatingPoint = false;
        private Vector3 tempAnchorPos;

        private void OnEnable()
        {
            controller = (PathController)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Krita-Style Bézier Tools", EditorStyles.boldLabel);

            PathData path = controller.Path;

            EditorGUILayout.HelpBox(
                "CARA MEMBUAT KURVA BÉZIER (ALA KRITA):\n" +
                "1. Tahan [SHIFT] + KLIK KIRI di Scene View, lalu TAHAN & SERET (DRAG) MOUSE untuk menarik gagang lengkungan (tangent handle).\n" +
                "2. Semakin jauh diseret, semakin lebar belokan kurvanya.\n" +
                "3. Klik bulatan magenta pada titik untuk mengubah lengkungan kapan saja.",
                MessageType.Info);

            if (selectedPointIndex >= 0 && selectedPointIndex < path.PointCount)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Selected: Point #{selectedPointIndex}", EditorStyles.boldLabel);

                if (GUILayout.Button($"Delete Selected Point #{selectedPointIndex}"))
                {
                    Undo.RecordObject(controller, "Delete Waypoint");
                    path.RemovePointAt(selectedPointIndex);
                    selectedPointIndex = Mathf.Clamp(selectedPointIndex - 1, 0, path.PointCount - 1);
                    EditorUtility.SetDirty(controller);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove Last Point"))
            {
                Undo.RecordObject(controller, "Remove Last Waypoint");
                path.RemoveLastPoint();
                selectedPointIndex = Mathf.Clamp(selectedPointIndex - 1, 0, path.PointCount - 1);
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("Clear All Points"))
            {
                if (EditorUtility.DisplayDialog("Clear Path", "Hapus semua jalur?", "Ya", "Batal"))
                {
                    Undo.RecordObject(controller, "Clear Path");
                    path.Clear();
                    selectedPointIndex = -1;
                    EditorUtility.SetDirty(controller);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play")) controller.Play();
                if (GUILayout.Button("Pause")) controller.Pause();
                if (GUILayout.Button("Stop")) controller.Stop();
                if (GUILayout.Button("Reset")) controller.ResetPlayback();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Snap Target to Start", GUILayout.Height(26)))
                {
                    Undo.RecordObject(controller.Target, "Snap Target");
                    controller.ResetPlayback();
                }
            }
        }

        private void OnSceneGUI()
        {
            if (controller == null) return;
            PathData path = controller.Path;
            if (path == null) return;

            Event currentEvent = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            // ==========================================
            // INTERAKSI CLICK & DRAG BÉZIER ALA KRITA
            // ==========================================
            if (currentEvent.shift)
            {
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    Vector3 worldPos = GetWorldPosFromMouse(currentEvent.mousePosition);
                    tempAnchorPos = worldPos;
                    isCreatingPoint = true;

                    Undo.RecordObject(controller, "Create Bézier Point");
                    path.AddPoint(tempAnchorPos, Quaternion.identity, Vector3.zero);
                    selectedPointIndex = path.PointCount - 1;
                    EditorUtility.SetDirty(controller);

                    GUIUtility.hotControl = controlID;
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.MouseDrag && isCreatingPoint && currentEvent.button == 0)
                {
                    Vector3 currentWorldPos = GetWorldPosFromMouse(currentEvent.mousePosition);
                    Vector3 handleVector = currentWorldPos - tempAnchorPos;

                    // Mengatur gagang tangen saat drag
                    path[selectedPointIndex].Handle = handleVector;
                    EditorUtility.SetDirty(controller);
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.MouseUp && isCreatingPoint && currentEvent.button == 0)
                {
                    isCreatingPoint = false;
                    GUIUtility.hotControl = 0;
                    currentEvent.Use();
                }
            }

            // ==========================================
            // MENGGAMBAR KURVA BÉZIER & HANDLES
            // ==========================================
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 11;
            labelStyle.fontStyle = FontStyle.Bold;

            int count = path.PointCount;
            for (int i = 0; i < count; i++)
            {
                PathPointData point = path[i];

                // Gambar garis lengkung Bézier ke titik berikutnya
                if (i < count - 1 || (controller.Loop && count >= 2))
                {
                    path.GetBezierPoints(i, controller.Loop, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);

                    Color curveColor = (i == count - 1 && controller.Loop) ? new Color(0f, 1f, 0.4f, 0.9f) : Color.cyan;
                    Handles.DrawBezier(p0, p3, p1, p2, curveColor, null, 4f);
                }

                // Label nomor titik
                Handles.Label(point.Position + Vector3.up * 0.4f, $"P{i}", labelStyle);

                // Tombol seleksi titik utama (Kuning/Hijau)
                float handleSize = HandleUtility.GetHandleSize(point.Position) * 0.16f;
                Handles.color = (i == selectedPointIndex) ? Color.green : Color.yellow;
                if (Handles.Button(point.Position, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                {
                    selectedPointIndex = i;
                    Repaint();
                }
            }

            // ==========================================
            // KONTROL TITIK & TANGENT HANDLE TERPILIH
            // ==========================================
            if (selectedPointIndex >= 0 && selectedPointIndex < path.PointCount)
            {
                PathPointData selectedPoint = path[selectedPointIndex];

                // 1. Panah 3D Posisi Anchor
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(selectedPoint.Position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(controller, "Move Waypoint");
                    selectedPoint.Position = newPos;
                    EditorUtility.SetDirty(controller);
                }

                // 2. Gagang Tangent Handle (Bézier Handle)
                Vector3 handleWorldPos = selectedPoint.Position + selectedPoint.Handle;
                Vector3 oppositeHandleWorldPos = selectedPoint.Position - selectedPoint.Handle;

                Handles.color = Color.magenta;
                Handles.DrawLine(oppositeHandleWorldPos, handleWorldPos, 1.5f);

                float tangentSize = HandleUtility.GetHandleSize(handleWorldPos) * 0.12f;
                EditorGUI.BeginChangeCheck();
                Vector3 newHandlePos = Handles.FreeMoveHandle(handleWorldPos, tangentSize, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(controller, "Adjust Bézier Handle");
                    selectedPoint.Handle = newHandlePos - selectedPoint.Position;
                    EditorUtility.SetDirty(controller);
                }
            }
        }

        private Vector3 GetWorldPosFromMouse(Vector2 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            // Cek raycast mesh collider 3D terlebih dahulu
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }

            // Fallback ke bidang horizontal setinggi target
            Plane groundPlane = new Plane(Vector3.up, controller.Target.position);
            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }
    }
}