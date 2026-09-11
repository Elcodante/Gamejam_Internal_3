using UnityEngine;
using UnityEngine.InputSystem;

public class InputTester : MonoBehaviour
{
    private RhythmControls controls;

    // Array untuk menyimpan 6 LaneManager kita
    public LaneManager[] lanes = new LaneManager[6];

    private void Awake()
    {
        controls = new RhythmControls();

        // Menyambungkan tombol ke Lane yang sesuai (Ingat Array mulai dari 0, jadi Lane 1 = index 0)
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
        // Ambil waktu musik saat tombol ditekan
        float currentSongTime = SongManager.Instance.songPosition;

        // Perintahkan Lane yang bersangkutan untuk mencoba memukul nada
        lanes[laneIndex].AttemptHit(currentSongTime);
    }
}