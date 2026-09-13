using UnityEngine;
using System.Collections;

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
    private double pauseStartTime;

    // --- BARU: Kunci untuk menahan Update ---
    private bool isGameStarted = false;

    void Awake()
    {
        if (instance == null) instance = this;

        if (songPlaylist != null && songPlaylist.Length > 0)
        {
            int randomSongIndex = Random.Range(0, songPlaylist.Length);
            musicSource.clip = songPlaylist[randomSongIndex];
            Debug.Log($"Lagu yang terpilih: {musicSource.clip.name}");
        }
    }

    IEnumerator Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        musicSource.volume = PlayerPrefs.GetFloat("BGMVolume", 1f);

        // --- PERBAIKAN FATAL: Bangunkan audio utama dan paksa muat! ---
        if (musicSource.clip != null)
        {
            if (musicSource.clip.loadState == AudioDataLoadState.Unloaded)
            {
                musicSource.clip.LoadAudioData();
            }

            while (musicSource.clip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;
            }
        }
        // --------------------------------------------------------------

        dspSongTime = AudioSettings.dspTime + songDelayInSeconds;

        if (musicSource.clip != null)
        {
            musicSource.PlayScheduled(dspSongTime);
        }

        visualSongPosition = -songDelayInSeconds;
        isGameStarted = true;
    }

    void Update()
    {
        // --- BARU: Tahan Update jika game belum benar-benar dimulai ---
        if (!isGameStarted || isSongFinished || Time.timeScale == 0f) return;

        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        visualSongPosition += Time.deltaTime;

        float drift = songPosition - visualSongPosition;
        if (Mathf.Abs(drift) > 0.02f)
        {
            visualSongPosition += drift * Time.deltaTime * 5f;
        }

        if (musicSource.clip != null && musicSource.clip.loadState == AudioDataLoadState.Loaded)
        {
            if (songPosition >= musicSource.clip.length)
            {
                TriggerWin();
            }
        }
    }

    public void PauseSong()
    {
        pauseStartTime = AudioSettings.dspTime;
        if (musicSource != null) musicSource.Pause();
    }

    public void ResumeSong()
    {
        double pauseDuration = AudioSettings.dspTime - pauseStartTime;
        dspSongTime += pauseDuration;

        if (musicSource != null)
        {
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