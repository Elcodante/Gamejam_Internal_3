using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// BARU: Class kustom untuk menyimpan ID dan Audio Clip berpasangan di Inspector
[System.Serializable]
public class Sound
{
    public string soundID; // Contoh: "MenuBGM", "Click", "Perfect"
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Library (Database)")]
    // BARU: Menggunakan Array dari class Sound
    public Sound[] bgmList;
    public Sound[] sfxList;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Tetap matikan BGM Menu jika masuk ke scene game (agar SongManager mengambil alih)
        if (scene.buildIndex != 0)
        {
            StopBGM();
        }
    }

    // --- SISTEM PEMANGGILAN BARU MENGGUNAKAN ID (STRING) ---

    public void PlayBGM(string id)
    {
        // Cari audio clip di dalam array berdasarkan ID-nya
        Sound s = Array.Find(bgmList, sound => sound.soundID == id);

        if (s == null)
        {
            Debug.LogWarning($"[AudioManager] BGM dengan ID '{id}' tidak ditemukan!");
            return;
        }

        if (bgmSource.clip == s.clip) return;

        bgmSource.clip = s.clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string id)
    {
        // Cari audio clip di dalam array berdasarkan ID-nya
        Sound s = Array.Find(sfxList, sound => sound.soundID == id);

        if (s == null)
        {
            Debug.LogWarning($"[AudioManager] SFX dengan ID '{id}' tidak ditemukan!");
            return;
        }

        sfxSource.PlayOneShot(s.clip);
    }
}