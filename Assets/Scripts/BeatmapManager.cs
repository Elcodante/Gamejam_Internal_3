using UnityEngine;

public class BeatmapManager : MonoBehaviour
{
    public SongBeatmap currentBeatmap;
    public GameObject notePrefab;
    public float spawnWarningTime = 2f;

    private int nextNoteIndex = 0;
    public InputTester inputManager;

    // BARU: Array untuk menyimpan objek HitZone langsung dari Scene
    public Transform[] hitZones;

    void Update()
    {
        if (nextNoteIndex < currentBeatmap.notes.Length)
        {
            NoteData nextNote = currentBeatmap.notes[nextNoteIndex];
            float spawnTime = nextNote.time - spawnWarningTime;

            if (SongManager.instance.songPosition >= spawnTime)
            {
                SpawnNote(nextNote);
                nextNoteIndex++;
            }
        }
    }

    void SpawnNote(NoteData data)
    {
        // 1. Ambil posisi target langsung dari objek HitZone visual
        Transform targetHitZone = hitZones[data.lane - 1];

        // 2. Munculkan nada lurus di atas target (misal ditambah jarak 10 ke atas dari posisi Y HitZone)
        Vector3 spawnPos = new Vector3(targetHitZone.position.x, targetHitZone.position.y + 10f, 0f);
        GameObject newNoteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.noteHitTime = data.time;

        // 3. BARU: Paksa nada mengenali persis di posisi Y berapa HitZone itu berada!
        controller.hitZoneY = targetHitZone.position.y;

        // Daftarkan ke sistem penilaian
        inputManager.lanes[data.lane - 1].activeNotes.Add(controller);
    }
}