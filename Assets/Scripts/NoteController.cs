using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float noteHitTime;

    public float scrollSpeed = 5f;

    public float hitZoneY = -4f;

    public bool isHit = false;

    void Update()
    {
        if (isHit) return;

        // PENTING: Gunakan waktu visual yang baru kita buat
        float currentVisualTime = SongManager.instance.visualSongPosition;

        // Hitung posisi Y menggunakan waktu visual
        float yPos = hitZoneY + ((noteHitTime - currentVisualTime) * scrollSpeed);
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

        // Tetap hancurkan jika terlewat
        if (currentVisualTime > noteHitTime + 1f)
        {
            UIManager.instance.RegisterHit("Miss");
            Destroy(gameObject);
        }
    }
}
