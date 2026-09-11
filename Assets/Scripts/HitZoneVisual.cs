using UnityEngine;
using System.Collections; // Wajib untuk Coroutine

public class HitZoneVisual : MonoBehaviour
{
    private SpriteRenderer sr;

    // Warna asli HitZone Anda
    private Color originalColor;

    // Warna saat tombol ditekan (misalnya abu-abu terang atau putih)
    public Color pressedColor = Color.white;

    // Seberapa cepat warna kembali memudar (fade)
    public float fadeSpeed = 5f;

    void Awake()
    {
        // Mengambil komponen SpriteRenderer dari objek ini
        sr = GetComponent<SpriteRenderer>();
        // Menyimpan warna awal yang Anda atur di Unity Editor
        originalColor = sr.color;
    }

    // Fungsi ini yang akan dipanggil saat pemain menekan tombol
    public void Flash()
    {
        // Hentikan kedipan sebelumnya (kalau pemain menekan tombol dengan sangat cepat)
        StopAllCoroutines();

        // Ubah langsung ke warna terang
        sr.color = pressedColor;

        // Mulai proses perlahan kembali ke warna asli
        StartCoroutine(FadeBack());
    }

    private IEnumerator FadeBack()
    {
        // Terus berjalan selama warna saat ini belum persis sama dengan warna asli
        while (sr.color != originalColor)
        {
            // Lerp digunakan untuk transisi halus antar dua warna dari waktu ke waktu
            sr.color = Color.Lerp(sr.color, originalColor, fadeSpeed * Time.deltaTime);
            yield return null; // Tunggu ke frame berikutnya
        }
    }
}