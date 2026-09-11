using UnityEngine;
using UnityEngine.InputSystem;

public class InputTester : MonoBehaviour
{
    private RhythmControls controls;

    public LaneManager[] lanes = new LaneManager[6];

    // BARU: Tambahkan referensi ke pengelola Beatmap untuk mengambil visual HitZones
    public AutoBeatmapGenerator autoBeatmapManager;

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
        // 1. TAMBAHKAN LOG INI: Agar kita tahu input keyboard benar-benar masuk!
        Debug.Log($"[Input Tester] Tombol untuk Jalur {laneIndex} ditekan!");

        float currentSongTime = SongManager.instance.songPosition;

        // 2. Cek dan eksekusi pukulan nada
        if (lanes[laneIndex] != null)
        {
            lanes[laneIndex].AttemptHit(currentSongTime);
        }

        // 3. Efek visual berkedip
        if (autoBeatmapManager != null && autoBeatmapManager.hitZones[laneIndex] != null)
        {
            HitZoneVisual visual = autoBeatmapManager.hitZones[laneIndex].GetComponent<HitZoneVisual>();
            if (visual != null)
            {
                visual.Flash();
            }
        }
        else
        {
            Debug.LogWarning($"Peringatan: autoBeatmapManager atau HitZone di jalur {laneIndex} belum dihubungkan!");
        }
    }
}