using UnityEngine;
using PathSystem.Runtime;

public class TireSkidController : MonoBehaviour
{
    [SerializeField] private PathController pathController;
    [SerializeField] private TrailRenderer[] skidTrails;

    private void Update()
    {
        if (pathController == null || skidTrails == null) return;

        bool drifting = pathController.IsDrifting;

        // Nyalakan jejak hanya jika mobil sedang drifting
        for (int i = 0; i < skidTrails.Length; i++)
        {
            if (skidTrails[i] != null)
            {
                skidTrails[i].emitting = drifting;
            }
        }
    }
}