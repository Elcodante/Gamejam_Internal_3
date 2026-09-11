using UnityEngine;

public class NoteController : MonoBehaviour
{
    public float noteHitTime;

    public float scrollSpeed = 5f;

    public float hitZoneY = -4f;

    public bool isHit = false;

    private void Update()
    {
        if (isHit) return;

        float currentSongTime = SongManager.Instance.songPosition;
        float yPos = hitZoneY + ((noteHitTime - currentSongTime) * scrollSpeed);
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

        if ( currentSongTime >= noteHitTime + 1f)
        {
            Debug.Log("Nada Terlewat (Miss)");
            Destroy(gameObject);
        }
    }
}
