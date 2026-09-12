using UnityEngine;

public class NoteController : MonoBehaviour
{
    [HideInInspector] public float noteHitTime;
    public float scrollSpeed = 5f;

    [HideInInspector] public Transform targetHitZone;

    public bool isHit = false;

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
        // PENTING: Jangan update posisi atau hitung Miss saat game berhenti/pause!
        if (isHit || Time.timeScale == 0f) return;

        ForcePositionUpdate();

        if (SongManager.instance.visualSongPosition > noteHitTime + 0.5f)
        {
            UIManager.instance.RegisterHit("Miss");
            Destroy(gameObject);
        }
    }
}