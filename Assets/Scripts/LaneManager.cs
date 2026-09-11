using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public int laneIndex; // Jalur ke berapa (1-6)

    // List (daftar) untuk menyimpan semua nada yang sedang berjalan di jalur ini
    public List<NoteController> activeNotes = new List<NoteController>();

    // Jendela waktu (dalam detik) untuk penilaian
    private float perfectWindow = 0.05f;
    private float goodWindow = 0.15f;

    // Fungsi ini dipanggil dari InputTester saat tombol jalur ini ditekan
    public void AttemptHit(float currentSongTime)
    {
        // Jika tidak ada nada di jalur ini, hentikan (pemain menekan tanpa ada nada)
        if (activeNotes.Count == 0) return;

        // Ambil nada paling pertama (paling bawah) di dalam daftar
        NoteController targetNote = activeNotes[0];

        // Hitung selisih waktu antara kapan ditekan vs kapan seharusnya ditekan
        float hitDifference = Mathf.Abs(targetNote.noteHitTime - currentSongTime);

        // Cek Penilaian berdasarkan selisih waktu
        if (hitDifference <= perfectWindow)
        {
            Debug.Log($"PERFECT! (Selisih: {hitDifference:F3} detik)");
            HitNote(targetNote);
        }
        else if (hitDifference <= goodWindow)
        {
            Debug.Log($"GOOD! (Selisih: {hitDifference:F3} detik)");
            HitNote(targetNote);
        }
        else
        {
            // Jika selisih masih sangat besar, berarti menekan terlalu cepat.
            Debug.Log($"Terlalu Cepat!");
        }
    }

    private void HitNote(NoteController note)
    {
        note.isHit = true; // Tandai sudah ditekan
        activeNotes.Remove(note); // Keluarkan dari daftar jalur

        // Hancurkan objek nada secara visual
        Destroy(note.gameObject);
    }
}