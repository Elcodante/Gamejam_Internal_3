using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public int laneIndex;
    public List<NoteController> activeNotes = new List<NoteController>();

    private float perfectWindow = 0.1f;
    private float goodWindow = 0.25f;
    private float badWindow = 0.4f;

    // BARU: Slot untuk memasukkan Prefab Partikel
    [Header("Visual Effects")]
    public GameObject perfectEffectPrefab;
    public GameObject goodEffectPrefab;

    public void AttemptHit(float currentSongTime)
    {
        activeNotes.RemoveAll(note => note == null);
        if (activeNotes.Count == 0) return;

        NoteController targetNote = activeNotes[0];
        float hitDifference = Mathf.Abs(targetNote.noteHitTime - currentSongTime);

        // KITA UBAH SEDIKIT: Kirim tipe pukulannya ke fungsi HitNote
        if (hitDifference <= perfectWindow)
        {
            UIManager.instance.RegisterHit("Perfect");
            HitNote(targetNote, "Perfect");
        }
        else if (hitDifference <= goodWindow)
        {
            UIManager.instance.RegisterHit("Good");
            HitNote(targetNote, "Good");
        }
        else if (hitDifference <= badWindow)
        {
            UIManager.instance.RegisterHit("Miss");
            HitNote(targetNote, "Miss"); // Miss tidak akan mengeluarkan partikel
        }
    }

    // Tipe parameter ditambahkan di sini
    private void HitNote(NoteController note, string hitType)
    {
        note.isHit = true;
        activeNotes.Remove(note);

        GameObject fxToSpawn = null;

        if (hitType == "Perfect")
        {
            fxToSpawn = perfectEffectPrefab;
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX("Perfect");
        }
            
        else if (hitType == "Good")
        {
            fxToSpawn = goodEffectPrefab;
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX("Great");
        }

        if (fxToSpawn != null)
        {
            // Parent ke container HitZone agar partikel ikut bergerak bersama kamera
            Transform fxParent = note.targetHitZone != null ? note.targetHitZone.parent : null;
            Instantiate(fxToSpawn, note.transform.position, note.transform.rotation, fxParent);
        }

        Destroy(note.gameObject);
    }
}