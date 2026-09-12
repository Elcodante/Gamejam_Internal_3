using UnityEngine;

public class SongManager : MonoBehaviour
{
    public static SongManager instance;
    public AudioSource musicSource;
    public float songDelayInSeconds = 5f;

    // --- BARU: WADAH UNTUK MENYIMPAN 3 LAGU ANDA ---
    [Header("Song Playlist")]
    public AudioClip[] songPlaylist;

    private double dspSongTime;
    public float songPosition;
    public float visualSongPosition;

    private bool isSongFinished = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        // --- BARU: LOGIKA PEMILIHAN LAGU ACAK ---
        if (songPlaylist.Length > 0)
        {
            // Pilih angka acak dari 0 sampai batas jumlah lagu yang ada
            int randomSongIndex = Random.Range(0, songPlaylist.Length);

            // Masukkan lagu yang terpilih ke dalam AudioSource utama
            musicSource.clip = songPlaylist[randomSongIndex];
            Debug.Log($"Lagu yang terpilih: {musicSource.clip.name}");
        }

        dspSongTime = AudioSettings.dspTime + songDelayInSeconds;

        // Pastikan ada lagu sebelum diputar agar tidak error
        if (musicSource.clip != null)
        {
            musicSource.PlayScheduled(dspSongTime);
        }

        visualSongPosition = -songDelayInSeconds;
    }

    void Update()
    {
        if (isSongFinished) return;

        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        visualSongPosition += Time.unscaledDeltaTime;

        float drift = songPosition - visualSongPosition;
        if (Mathf.Abs(drift) > 0.02f)
        {
            visualSongPosition += drift * Time.unscaledDeltaTime * 5f;
        }

        if (musicSource.clip != null && songPosition >= musicSource.clip.length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        isSongFinished = true;
        Debug.Log("<color=green>Lagu Selesai! Pemain Menang!</color>");

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowResult(true);
        }

        Time.timeScale = 0f;
    }
}