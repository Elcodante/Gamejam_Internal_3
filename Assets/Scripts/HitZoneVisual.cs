using UnityEngine;
using System.Collections;

public class HitZoneVisual : MonoBehaviour
{
    private SpriteRenderer sr;

    private Color originalColor;
    private Vector3 originalScale; // BARU: Menyimpan ukuran asli HitZone

    [Header("Visual Settings")]
    public Color pressedColor = Color.white;

    // BARU: Ukuran saat tombol ditekan. (0.8 = mengecil 20%. Jika ingin membesar, isi 1.2)
    public Vector3 pressedScale = new Vector3(0.8f, 0.8f, 1f);

    // Dipercepat agar kembalinya tombol terasa seperti pegas (membal)
    public float fadeSpeed = 15f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        // Simpan ukuran awal yang Anda atur di Unity Editor
        originalScale = transform.localScale;
    }

    public void Flash()
    {
        StopAllCoroutines();

        // 1. Ubah instan ke warna terang dan ukuran tertekan (Squish!)
        sr.color = pressedColor;
        transform.localScale = pressedScale;

        // 2. Mulai proses pegas untuk kembali ke asal
        StartCoroutine(FadeBack());
    }

    private IEnumerator FadeBack()
    {
        // Terus berjalan selama warna belum sama ATAU ukuran belum sama
        // (Menggunakan jarak Vector3.Distance untuk toleransi kemulusan)
        while (sr.color != originalColor || Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            // Kembalikan warna perlahan
            sr.color = Color.Lerp(sr.color, originalColor, fadeSpeed * Time.deltaTime);

            // Kembalikan ukuran perlahan (memberikan efek pegas)
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, fadeSpeed * Time.deltaTime);

            yield return null;
        }

        // Pastikan kembali 100% presisi di akhir frame
        sr.color = originalColor;
        transform.localScale = originalScale;
    }
}