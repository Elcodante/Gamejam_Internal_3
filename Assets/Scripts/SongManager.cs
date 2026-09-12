using UnityEngine;

public class SongManager : MonoBehaviour
{
    public static SongManager instance;
    public AudioSource musicSource;
    public float songDelayInSeconds = 5f;

    private double dspSongTime;
    public float songPosition;
    public float visualSongPosition;

    // BARU: Penanda agar fungsi menang tidak dipanggil berkali-kali
    private bool isSongFinished = false;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        dspSongTime = AudioSettings.dspTime + songDelayInSeconds;
        musicSource.PlayScheduled(dspSongTime);

        visualSongPosition = -songDelayInSeconds;
    }

    void Update()
    {
        // Hentikan penghitungan jika lagu sudah selesai
        if (isSongFinished) return;

        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        visualSongPosition += Time.unscaledDeltaTime;

        float drift = songPosition - visualSongPosition;
        if (Mathf.Abs(drift) > 0.02f)
        {
            visualSongPosition += drift * Time.unscaledDeltaTime * 5f;
        }

        // --- BARU: DETEKSI LAGU SELESAI (KONDISI MENANG) ---
        // musicSource.clip.length berisi total durasi asli lagu tersebut (dalam detik)
        if (musicSource.clip != null && songPosition >= musicSource.clip.length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        isSongFinished = true;

        Debug.Log("<color=green>Lagu Selesai! Pemain Menang!</color>");

        // Panggil UI Manager untuk memunculkan panel result
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowResult(true);
        }

        // Hentikan pergerakan semua sisa partikel atau nada di latar belakang
        Time.timeScale = 0f;
    }
}