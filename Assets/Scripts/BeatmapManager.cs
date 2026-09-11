using UnityEngine;

public class BeatmapManager : MonoBehaviour
{
    public SongBeatmap currentBeatmap; // File data yang baru kita buat
    public GameObject notePrefab;      // Prefab nada visual kita
    public float spawnWarningTime = 2f; // Berapa detik sebelum waktu tekan nada harus dimunculkan?

    private int nextNoteIndex = 0; // Melacak nada mana yang sedang kita cek untuk dispawn
    public InputTester inputManager; // Menyimpan referensi ke pengelola jalur

    // Referensi posisi X untuk ke-6 jalur (Sesuaikan dengan posisi X HitZone Anda di Fase 1)
    private float[] laneXPositions = { -3f, -1.8f, -0.6f, 0.6f, 1.8f, 3f };

    void Update()
    {
        // 1. Cek apakah masih ada nada yang tersisa di daftar
        if (nextNoteIndex < currentBeatmap.notes.Length)
        {
            // Ambil data nada berikutnya
            NoteData nextNote = currentBeatmap.notes[nextNoteIndex];

            // 2. Kapan nada ini harus dimunculkan?
            // (Waktu Tekan) dikurangi (Waktu Peringatan Awal)
            float spawnTime = nextNote.time - spawnWarningTime;

            // 3. Jika posisi lagu saat ini sudah mencapai (atau melewati) waktu spawn...
            if (SongManager.Instance.songPosition >= spawnTime)
            {
                SpawnNote(nextNote);
                nextNoteIndex++; // Pindah ke nada berikutnya di daftar
            }
        }
    }

    void SpawnNote(NoteData data)
    {
        // Tentukan posisi X berdasarkan jalur (Ingat, lane di data adalah 1-6, sedangkan array index 0-5)
        float xPos = laneXPositions[data.lane - 1];

        // Spawn objek nada tinggi di atas layar (misal Y = 6)
        Vector3 spawnPos = new Vector3(xPos, 6f, 0f);
        GameObject newNoteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        // Ambil komponen NoteController dan berikan waktu tekannya
        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.noteHitTime = data.time;

        // PENTING: Daftarkan nada ini ke LaneManager yang benar agar bisa dinilai saat ditekan!
        inputManager.lanes[data.lane - 1].activeNotes.Add(controller);
    }
}