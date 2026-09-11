using UnityEngine;

public class SongManager : MonoBehaviour
{
    public static SongManager instance;
    public AudioSource musicSource;
    public float songDelayInSeconds = 3f;

    private double dspSongTime;

    // 1. Waktu untuk PENILAIAN / LOGIKA (Presisi tinggi, tapi pembaruannya kasar)
    public float songPosition;

    // 2. Waktu untuk PERGERAKAN VISUAL (Sangat mulus mengikuti monitor Anda)
    public float visualSongPosition;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60; // Atau ubah ke 120/144 jika monitor Anda mendukung
        QualitySettings.vSyncCount = 1;

        dspSongTime = AudioSettings.dspTime + songDelayInSeconds;
        musicSource.PlayScheduled(dspSongTime);

        // Set awal waktu visual sama dengan minus delay
        visualSongPosition = -songDelayInSeconds;
    }

    void Update()
    {
        // 1. Ambil Waktu Audio Asli
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);

        // 2. Tambahkan Waktu Visual secara frame-by-frame (sangat halus)
        visualSongPosition += Time.unscaledDeltaTime;

        // 3. SISTEM ANTI-DRIFT (Koreksi)
        // Mencegah waktu visual tertinggal dari audio jika dimainkan di lagu berdurasi panjang
        float drift = songPosition - visualSongPosition;
        if (Mathf.Abs(drift) > 0.02f)
        {
            // Jika mulai tidak sinkron, tarik perlahan waktu visual agar menyamai audio
            visualSongPosition += drift * Time.unscaledDeltaTime * 5f;
        }
    }
}