using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Sound
{
    public string soundID;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Library")]
    public Sound[] bgmList;
    public Sound[] sfxList;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // --- BARU: Muat pengaturan volume yang tersimpan saat game baru dibuka ---
            // Angka 1f berarti nilai standarnya adalah 100% jika belum pernah disetting
            SetBGMVolume(PlayerPrefs.GetFloat("BGMVolume", 1f));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 0) StopBGM();
    }

    public void PlayBGM(string id)
    {
        Sound s = Array.Find(bgmList, sound => sound.soundID == id);

        if (s == null)
        {
            Debug.LogWarning($"[AudioManager] BGM dengan ID '{id}' tidak ditemukan!");
            return;
        }

        // --- PERBAIKAN BUG ---
        // Jika lagunya sama DAN sedang berputar, baru hentikan perintah.
        // Tapi jika lagunya sama TAPI sedang mati (habis dari scene game), abaikan return dan paksa Play!
        if (bgmSource.clip == s.clip && bgmSource.isPlaying) return;
        // ---------------------

        bgmSource.clip = s.clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM() { bgmSource.Stop(); }

    public void PlaySFX(string id)
    {
        Sound s = Array.Find(sfxList, sound => sound.soundID == id);
        if (s == null) return;
        sfxSource.PlayOneShot(s.clip);
    }

    // --- BARU: FUNGSI PENGATUR VOLUME ---

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume); // Simpan ke memori HP/PC

        // SANGAT PENTING: Jika pemain mengatur BGM saat game balapan sedang dipause, 
        // kita juga harus mengecilkan volume di SongManager!
        if (SongManager.instance != null && SongManager.instance.musicSource != null)
        {
            SongManager.instance.musicSource.volume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume); // Simpan ke memori HP/PC
    }
}