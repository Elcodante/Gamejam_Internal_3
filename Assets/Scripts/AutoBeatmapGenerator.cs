using UnityEngine;

public class AutoBeatmapGenerator : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource ghostAudio;

    [Header("Settings")]
    public float spawnWarningTime = 2f;

    // BARU: Kalibrasi waktu (Offset). 
    // Jika nada selalu terasa TERLAMBAT dari lagu, isi dengan angka minus (misal -0.1).
    // Jika nada terasa TERLALU CEPAT, isi dengan angka plus (misal 0.1).
    public float audioOffset = 0f;

    [Header("Detection Tuning")]
    public float beatThreshold = 0.02f;
    public float cooldown = 0.25f;

    [Header("References")]
    public GameObject notePrefab;

    public float noteScale = 1f;

    public InputTester inputManager;
    public Transform[] hitZones;

    [Header("Visual Assets")]
    public Sprite[] noteSprites;

    private float[] spectrumData = new float[256];
    private float lastSpawnTime = 0f;

    // BARU: Menyimpan tingkat volume bass di frame sebelumnya untuk mencari hentakan
    private float previousBassEnergy = 0f;

    void Start()
    {
        if (SongManager.instance.musicSource.clip != null)
        {
            ghostAudio.clip = SongManager.instance.musicSource.clip;
        }

        double mainStartTime = AudioSettings.dspTime + SongManager.instance.songDelayInSeconds;
        ghostAudio.PlayScheduled(mainStartTime - spawnWarningTime);
    }

    void Update()
    {
        if (!ghostAudio.isPlaying) return;

        ghostAudio.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // 1. KOREKSI FREKUENSI: Hanya ambil 3 pita pertama (0, 1, 2). 
        // Ini murni area Sub-Bass dan Kick Drum (sekitar 0 - 250 Hz). Bebas dari vokal/melodi.
        float bassEnergy = 0f;
        for (int i = 0; i < 3; i++)
        {
            bassEnergy += spectrumData[i];
        }
        bassEnergy /= 3f; // Rata-rata dari 3 pita

        // 2. DETEKSI PUNCAK HENTAKAN (ATTACK DETECTION)
        // Kita hanya mencatat "ketukan" jika energi saat ini LEBIH BESAR dari frame sebelumnya.
        bool isSpiking = bassEnergy > previousBassEnergy;
        bool isLoudEnough = bassEnergy > beatThreshold;
        bool isCooldownReady = Time.time > lastSpawnTime + cooldown;

        // Jika suara sedang menghentak naik + cukup keras + tidak sedang cooldown
        if (isSpiking && isLoudEnough && isCooldownReady)
        {
            lastSpawnTime = Time.time;
            SpawnRandomNote();
        }

        // 3. Simpan energi frame ini untuk perbandingan di frame selanjutnya
        previousBassEnergy = bassEnergy;
    }

    void SpawnRandomNote()
    {
        int randomLane = Random.Range(1, 7);
        Transform targetHitZone = hitZones[randomLane - 1];

        Vector3 safeSpawnPos = targetHitZone.position + (targetHitZone.up * 50f);
        GameObject newNoteObj = Instantiate(notePrefab, safeSpawnPos, targetHitZone.rotation, targetHitZone.parent);

        newNoteObj.transform.localScale = new Vector3(noteScale, noteScale, 1f);

        SpriteRenderer noteRenderer = newNoteObj.GetComponent<SpriteRenderer>();
        if (noteRenderer != null && noteSprites.Length >= 6)
        {
            noteRenderer.sprite = noteSprites[randomLane - 1];
        }

        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.targetHitZone = targetHitZone;

        // DITAMBAHKAN KALIBRASI OFFSET UNTUK PENYESUAIAN MANUAL
        controller.noteHitTime = SongManager.instance.visualSongPosition + spawnWarningTime + audioOffset;

        controller.ForcePositionUpdate();
        inputManager.lanes[randomLane - 1].activeNotes.Add(controller);
    }
}