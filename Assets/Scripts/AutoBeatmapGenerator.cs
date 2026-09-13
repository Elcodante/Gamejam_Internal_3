using UnityEngine;
using System.Collections;

public class AutoBeatmapGenerator : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource ghostAudio;

    [Header("Settings")]
    public float spawnWarningTime = 2f;
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
    private float previousBassEnergy = 0f;

    private double pauseStartTime;
    private double scheduledStartTime;

    // --- BARU: Variabel untuk jaring pengaman Game Jam ---
    private int notesSpawned = 0;

    void Start()
    {
        if (SongManager.instance != null && SongManager.instance.musicSource.clip != null)
        {
            ghostAudio.clip = SongManager.instance.musicSource.clip;
        }
        StartCoroutine(WaitAudioLoadAndPlay());
    }

    private IEnumerator WaitAudioLoadAndPlay()
    {
        if (ghostAudio.clip != null)
        {
            if (ghostAudio.clip.loadState == AudioDataLoadState.Unloaded)
                ghostAudio.clip.LoadAudioData();

            while (ghostAudio.clip.loadState == AudioDataLoadState.Loading)
                yield return null;

            if (ghostAudio.clip.loadState == AudioDataLoadState.Failed)
                yield break;
        }

        float delay = SongManager.instance.songDelayInSeconds;
        if (spawnWarningTime >= delay) spawnWarningTime = delay - 0.5f;

        scheduledStartTime = AudioSettings.dspTime + delay - spawnWarningTime;
        float timeToWait = (float)(scheduledStartTime - AudioSettings.dspTime);

        if (timeToWait > 0) yield return new WaitForSeconds(timeToWait);

        ghostAudio.Play();
    }

    void Update()
    {
        if (!ghostAudio.isPlaying) return;

        // ===================================================================
        // 🚨 SISTEM FAIL-SAFE GAME JAM 🚨
        // Jika browser ngambek dan FFT = 0 terus, selamatkan game ini!
        if (ghostAudio.time > 4f && notesSpawned == 0)
        {
            if (Time.frameCount % 120 == 0)
                Debug.LogWarning("[DEBUG LOG] Browser memblokir FFT! Mengaktifkan Mode Auto-Spawn!");

            if (Time.time > lastSpawnTime + cooldown)
            {
                lastSpawnTime = Time.time;
                SpawnRandomNote();
            }
            return; // Batalkan proses FFT di bawah karena browser tidak mendukungnya
        }
        // ===================================================================

        ghostAudio.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        float bassEnergy = 0f;
        for (int i = 0; i < 3; i++)
        {
            bassEnergy += spectrumData[i];
        }
        bassEnergy /= 3f;

        // Karena volume Anda 0.1 (10%), kita kalikan 10 agar kekuatannya kembali 100%
        bassEnergy *= 10f;

        if (Time.frameCount % 60 == 0)
            Debug.Log($"[DEBUG LOG] Analisa Bass (Volume 0.1): {bassEnergy.ToString("F4")}");

        bool isSpiking = bassEnergy > previousBassEnergy;
        bool isLoudEnough = bassEnergy > beatThreshold;
        bool isCooldownReady = Time.time > lastSpawnTime + cooldown;

        if (isSpiking && isLoudEnough && isCooldownReady)
        {
            lastSpawnTime = Time.time;
            notesSpawned++; // Catat bahwa FFT berhasil agar sistem Fail-Safe tidak menyala
            SpawnRandomNote();
        }

        previousBassEnergy = bassEnergy;
    }

    public void PauseGenerator()
    {
        pauseStartTime = AudioSettings.dspTime;
        if (ghostAudio != null) ghostAudio.Pause();
    }

    public void ResumeGenerator()
    {
        double pauseDuration = AudioSettings.dspTime - pauseStartTime;
        scheduledStartTime += pauseDuration;

        if (ghostAudio != null)
        {
            if (AudioSettings.dspTime < scheduledStartTime)
            {
                ghostAudio.Stop();
                ghostAudio.PlayScheduled(scheduledStartTime);
            }
            else
            {
                ghostAudio.UnPause();
            }
        }
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
            noteRenderer.sprite = noteSprites[randomLane - 1];

        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.targetHitZone = targetHitZone;
        controller.noteHitTime = SongManager.instance.visualSongPosition + spawnWarningTime + audioOffset;

        controller.ForcePositionUpdate();
        inputManager.lanes[randomLane - 1].activeNotes.Add(controller);
    }
}