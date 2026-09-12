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
        Transform targetHitZone = hitZones[data.lane - 1];

        // Spawn sebagai child dari parent HitZone
        GameObject newNoteObj = Instantiate(notePrefab, targetHitZone.position, targetHitZone.rotation, targetHitZone.parent);

        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.targetHitZone = targetHitZone;
        controller.noteHitTime = data.time;

        inputManager.lanes[data.lane - 1].activeNotes.Add(controller);
    }
}