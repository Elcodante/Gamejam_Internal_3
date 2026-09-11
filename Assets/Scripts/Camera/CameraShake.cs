using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private Vector3 originalPos;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Simpan posisi asli kamera saat game dimulai
        originalPos = transform.localPosition;
    }

    // Fungsi ini bisa dipanggil dari script manapun
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // Hentikan getaran sebelumnya jika pemain Miss berturut-turut
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Menghasilkan posisi acak di sekitar posisi asli
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            // Gunakan unscaledDeltaTime agar getaran tetap jalan walau game sedang di-pause
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Kembalikan posisi kamera persis ke asalnya setelah getaran selesai
        transform.localPosition = originalPos;
    }
}