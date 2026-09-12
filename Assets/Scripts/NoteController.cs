using UnityEngine;

public class NoteController : MonoBehaviour
{
    [HideInInspector] public float noteHitTime;
    public float scrollSpeed = 5f;

    // Referensi ke target HitZone milik jalur ini
    [HideInInspector] public Transform targetHitZone;

    public bool isHit = false;

    void Update()
    {
        if (isHit) return;
        if (targetHitZone == null) return;

        // Menggunakan waktu visual yang sinkron dengan frame monitor
        float currentVisualTime = SongManager.instance.visualSongPosition;

        // Jarak offset sepanjang jalur menuju HitZone
        // Saat currentVisualTime == noteHitTime, distance bernilai 0 (tepat di HitZone)
        float distance = (noteHitTime - currentVisualTime) * scrollSpeed;

        // Selalu sinkronkan posisi dan rotasi dengan HitZone yang sedang bergerak di kurva 3D
        transform.position = targetHitZone.position + (targetHitZone.up * distance);
        transform.rotation = targetHitZone.rotation;

        // Hancurkan jika terlewat (melewati batas toleransi miss)
        if (currentVisualTime > noteHitTime + 0.5f)
        {
            UIManager.instance.RegisterHit("Miss");
            Destroy(gameObject);
        }
    }
}