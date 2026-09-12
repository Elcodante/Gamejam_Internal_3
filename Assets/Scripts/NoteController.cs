using UnityEngine;

public class NoteController : MonoBehaviour
{
    [HideInInspector] public float noteHitTime;
    public float scrollSpeed = 5f;

    [HideInInspector] public Transform targetHitZone;

    public bool isHit = false;

    // FUNGSI BARU INI WAJIB ADA
    public void ForcePositionUpdate()
    {
        if (targetHitZone == null) return;

        float currentVisualTime = SongManager.instance.visualSongPosition;
        float distance = (noteHitTime - currentVisualTime) * scrollSpeed;

        transform.position = targetHitZone.position + (targetHitZone.up * distance);
        transform.rotation = targetHitZone.rotation;
    }

    void Update()
    {
        if (isHit) return;

        // Panggil fungsi pergerakannya di sini juga
        ForcePositionUpdate();

        if (SongManager.instance.visualSongPosition > noteHitTime + 0.5f)
        {
            UIManager.instance.RegisterHit("Miss");
            Destroy(gameObject);
        }
    }
}