using UnityEngine;
using UnityEngine.UI; // Wajib untuk mengakses komponen UI

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("BGM Settings")]
    // Pastikan nama ini SAMA PERSIS dengan ID BGM di AudioManager Anda (misal: "MenuBGM")
    public string menuBGM_ID = "MenuBGM";

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(menuBGM_ID);
        }

        // 1. Sinkronkan posisi Slider dengan nilai yang tersimpan di sistem
        if (bgmSlider != null)
        {
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
            // Tambahkan "pendengar" agar saat slider digeser, ia langsung memanggil fungsi di bawah
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    // Fungsi ini otomatis terpanggil setiap kali tuas slider digeser
    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetBGMVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(value);
        }
    }
}