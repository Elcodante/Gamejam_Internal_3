using UnityEngine;

public class AutoBeatmapGenerator : MonoBehaviour
{
    [Header("Audio Sources")]
    // Kita hapus mainAudio dari sini, biarkan SongManager yang mengurus lagu utama!
    public AudioSource ghostAudio;

    [Header("Settings")]
    public float spawnWarningTime = 2f;

    [Header("Detection Tuning")]
    // KITA TURUNKAN SENSITIVITASNYA SEMENTARA UNTUK TES
    public float beatThreshold = 0.01f;
    public float cooldown = 0.2f;

    [Header("References")]
    public GameObject notePrefab;
    public InputTester inputManager;
    public Transform[] hitZones;

    private float[] spectrumData = new float[256];
    private float lastSpawnTime = 0f;

    void Start()
    {
        // 1. Ambil waktu delay dari SongManager agar sinkron!
        double mainStartTime = AudioSettings.dspTime + SongManager.instance.songDelayInSeconds;

        // 2. Putar Ghost Audio LEBIH AWAL sebesar waktu jatuh (spawnWarningTime)
        ghostAudio.PlayScheduled(mainStartTime - spawnWarningTime);
    }

    void Update()
    {
        if (!ghostAudio.isPlaying) return;

        // 3. FFT: Mengubah suara menjadi data frekuensi
        ghostAudio.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // 4. Analisis Bass: Rata-ratakan 10 pita frekuensi terendah
        float bassEnergy = 0f;
        for (int i = 0; i < 10; i++)
        {
            bassEnergy += spectrumData[i];
        }
        bassEnergy /= 10;

        // BUKA LOG INI JIKA INGIN MELIHAT ANGKA BASS SAAT GAME BERJALAN
        //Debug.Log($"Energi Bass Saat Ini: {bassEnergy}");

        // 5. Cek ambang batas
        if (bassEnergy > beatThreshold && Time.time > lastSpawnTime + cooldown)
        {
            lastSpawnTime = Time.time;
            SpawnRandomNote();
        }
    }

    void SpawnRandomNote()
    {
        int randomLane = Random.Range(1, 7);
        Transform targetHitZone = hitZones[randomLane - 1];

        // 1. SOLUSI BUG 1-FRAME: Tentukan posisi spawn sementara yang jauh di atas layar 
        // (misal 50 unit ke arah 'atas' dari target). Ini menjauhkan nada dari pandangan di frame pertama.
        Vector3 safeSpawnPos = targetHitZone.position + (targetHitZone.up * 50f);

        // 2. Spawn note di posisi aman tersebut, tetap sebagai child dari parent HitZone
        GameObject newNoteObj = Instantiate(notePrefab, safeSpawnPos, targetHitZone.rotation, targetHitZone.parent);

        // 3. Hubungkan data target
        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.targetHitZone = targetHitZone;
        controller.noteHitTime = SongManager.instance.visualSongPosition + spawnWarningTime;

        // 4. PENTING: Paksa nada menghitung posisi aslinya saat ini juga agar tidak ada delay visual!
        controller.ForcePositionUpdate();

        inputManager.lanes[randomLane - 1].activeNotes.Add(controller);
    }
}