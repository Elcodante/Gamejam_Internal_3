using UnityEngine;
using UnityEngine.InputSystem;

public class InputTester : MonoBehaviour
{
    private RhythmControls controls;

    public LaneManager[] lanes = new LaneManager[6];
    public AutoBeatmapGenerator autoBeatmapManager;

    // BARU: Sambungkan referensi visual mobil (opsional jika pakai singleton instance)
    [Header("Car Visual Juice")]
    public CarVisualReaction carVisual;

    private void Awake()
    {
        controls = new RhythmControls();

        controls.Gameplay.Lane1.performed += ctx => CheckLaneHit(0);
        controls.Gameplay.Lane2.performed += ctx => CheckLaneHit(1);
        controls.Gameplay.Lane3.performed += ctx => CheckLaneHit(2);
        controls.Gameplay.Lane4.performed += ctx => CheckLaneHit(3);
        controls.Gameplay.Lane5.performed += ctx => CheckLaneHit(4);
        controls.Gameplay.Lane6.performed += ctx => CheckLaneHit(5);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void CheckLaneHit(int laneIndex)
    {
        float currentSongTime = SongManager.instance.songPosition;

        // 1. Cek dan eksekusi ketukan nada
        if (lanes[laneIndex] != null)
        {
            lanes[laneIndex].AttemptHit(currentSongTime);
        }

        // 2. Efek HitZone berkedip
        if (autoBeatmapManager != null && autoBeatmapManager.hitZones[laneIndex] != null)
        {
            HitZoneVisual visual = autoBeatmapManager.hitZones[laneIndex].GetComponent<HitZoneVisual>();
            if (visual != null)
            {
                visual.Flash();
            }
        }

        // 3. BARU: Picu gerakan miring bodi mobil secara visual!
        if (carVisual != null)
        {
            carVisual.TriggerLaneReaction(laneIndex);
        }
        else if (CarVisualReaction.instance != null)
        {
            CarVisualReaction.instance.TriggerLaneReaction(laneIndex);
        }
    }
}