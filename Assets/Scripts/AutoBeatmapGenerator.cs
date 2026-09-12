using UnityEngine;

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

    // --- BARU: Variabel untuk Ghost Audio ---
    private double pauseStartTime;
    private double scheduledStartTime;

    void Start()
    {
        if (SongManager.instance != null && SongManager.instance.musicSource.clip != null)
        {
            ghostAudio.clip = SongManager.instance.musicSource.clip;
        }

        scheduledStartTime = AudioSettings.dspTime + SongManager.instance.songDelayInSeconds - spawnWarningTime;
        ghostAudio.PlayScheduled(scheduledStartTime);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (!ghostAudio.isPlaying) return;

        ghostAudio.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        float bassEnergy = 0f;
        for (int i = 0; i < 3; i++)
        {
            bassEnergy += spectrumData[i];
        }
        bassEnergy /= 3f;

        bool isSpiking = bassEnergy > previousBassEnergy;
        bool isLoudEnough = bassEnergy > beatThreshold;
        bool isCooldownReady = Time.time > lastSpawnTime + cooldown;

        if (isSpiking && isLoudEnough && isCooldownReady)
        {
            lastSpawnTime = Time.time;
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
        {
            noteRenderer.sprite = noteSprites[randomLane - 1];
        }

        NoteController controller = newNoteObj.GetComponent<NoteController>();
        controller.targetHitZone = targetHitZone;
        controller.noteHitTime = SongManager.instance.visualSongPosition + spawnWarningTime + audioOffset;

        controller.ForcePositionUpdate();
        inputManager.lanes[randomLane - 1].activeNotes.Add(controller);
    }
}