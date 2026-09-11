using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public int laneIndex;

    public List<NoteController> activeNotes = new List<NoteController>();

    // KITA PERBESAR JENDELA WAKTU AGAR LEBIH NYAMAN DIMAINKAN
    private float perfectWindow = 0.1f;  // Selisih 0.1 detik
    private float goodWindow = 0.25f;    // Selisih 0.25 detik
    private float badWindow = 0.4f;      // Selisih 0.4 detik (Meleset tapi tetap kena)

    public void AttemptHit(float currentSongTime)
    {
        // 1. SOLUSI "HANTU": Bersihkan daftar dari nada yang sudah hancur karena terlewat!
        // (Ini sangat penting agar list tidak dipenuhi objek kosong/null)
        activeNotes.RemoveAll(note => note == null);

        // Jika tidak ada nada aktif di jalur ini, batalkan
        if (activeNotes.Count == 0) return;

        // 2. Selalu cek nada urutan pertama (yang posisinya paling bawah)
        NoteController targetNote = activeNotes[0];

        // 3. Hitung selisih waktu
        float hitDifference = Mathf.Abs(targetNote.noteHitTime - currentSongTime);

        // LOG DIAGNOSTIK: Akan memberi tahu Anda persisnya berapa detik Anda meleset
        Debug.Log($"[Jalur {laneIndex}] Tombol ditekan. Selisih Waktu: {hitDifference:F3} detik");

        // 4. Sistem Penilaian
        if (hitDifference <= perfectWindow)
        {
            Debug.Log("<color=cyan>PERFECT!</color>");
            HitNote(targetNote);
        }
        else if (hitDifference <= goodWindow)
        {
            Debug.Log("<color=green>GOOD!</color>");
            HitNote(targetNote);
        }
        else if (hitDifference <= badWindow)
        {
            Debug.Log("<color=orange>BAD!</color>");
            HitNote(targetNote);
        }
        else
        {
            // Jika Anda menekan dengan selisih > 0.4 detik, 
            // anggap pencet sembarangan (spam). Nada dibiarkan lewat.
            Debug.Log("<color=red>DIABAIKAN (Terlalu jauh!)</color>");
        }
    }

    private void HitNote(NoteController note)
    {
        note.isHit = true;

        // Hapus dari daftar jalur ini
        activeNotes.Remove(note);

        // Hancurkan objek visualnya
        Destroy(note.gameObject);
    }
}