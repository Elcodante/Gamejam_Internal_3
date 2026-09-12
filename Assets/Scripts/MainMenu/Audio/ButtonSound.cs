using UnityEngine;
using UnityEngine.UI; // Wajib untuk mengakses komponen Button

// Baris ini memastikan objek tempat script ini dipasang pasti memiliki komponen Button
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public string soundID = "Click"; // ID standar, bisa diubah di Inspector

    void Start()
    {
        // Mengambil komponen tombol pada objek ini
        Button btn = GetComponent<Button>();

        // Menambahkan perintah ke dalam sistem OnClick secara otomatis melalui kode
        btn.onClick.AddListener(PlaySound);
    }

    public void PlaySound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(soundID);
        }
    }
}