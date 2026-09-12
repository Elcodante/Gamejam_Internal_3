using UnityEngine;

public class SongManager : MonoBehaviour
{
    public static SongManager instance;
    public AudioSource musicSource;
    public float songDelayInSeconds = 5f;

    [Header("Song Playlist")]
    public AudioClip[] songPlaylist;

    private double dspSongTime;
    public float songPosition;
    public float visualSongPosition;

    public bool isSongFinished = false;

    // --- BARU: Variabel untuk mencatat waktu pause ---
    private double pauseStartTime;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        musicSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);

        if (songPlaylist != null && songPlaylist.Length > 0)
        {
            int randomSongIndex = Random.Range(0, songPlaylist.Length);
            musicSource.clip = songPlaylist[randomSongIndex];
            Debug.Log($"Lagu yang terpilih: {musicSource.clip.name}");
        }

        dspSongTime = AudioSettings.dspTime + songDelayInSeconds;

        if (musicSource.clip != null)
        {
            musicSource.PlayScheduled(dspSongTime);
        }

        visualSongPosition = -songDelayInSeconds;
    }

    void Update()
    {
        if (isSongFinished || Time.timeScale == 0f) return;

        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        visualSongPosition += Time.deltaTime;

        float drift = songPosition - visualSongPosition;
        if (Mathf.Abs(drift) > 0.02f)
        {
            visualSongPosition += drift * Time.deltaTime * 5f;
        }

        if (musicSource.clip != null && songPosition >= musicSource.clip.length)
        {
            TriggerWin();
        }
    }

    // ==========================================
    // FUNGSI BARU UNTUK PAUSE AUDIO DENGAN AMAN
    // ==========================================
    public void PauseSong()
    {
        pauseStartTime = AudioSettings.dspTime; // Catat kapan audio mulai di-pause
        if (musicSource != null) musicSource.Pause();
    }

    public void ResumeSong()
    {
        // Hitung berapa detik game tertahan, lalu majukan jadwal lagunya
        double pauseDuration = AudioSettings.dspTime - pauseStartTime;
        dspSongTime += pauseDuration;

        if (musicSource != null)
        {
            // Jika pause dilakukan saat hitung mundur 3, 2, 1 (lagu belum bunyi), jadwalkan ulang!
            if (songPosition < 0)
            {
                musicSource.Stop();
                musicSource.PlayScheduled(dspSongTime);
            }
            else
            {
                musicSource.UnPause();
            }
        }
    }

    private void TriggerWin()
    {
        if (isSongFinished) return;
        isSongFinished = true;
        Debug.Log("<color=green>Lagu Selesai! Pemain Menang!</color>");

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowResult(true);
        }

        Time.timeScale = 0f;
    }
}