using UnityEngine;
using System.Collections;

// KUNCI UTAMA: ExecutionOrder 1000 memastikan script ini dijalankan
// TEPAT SETELAH CinemachineBrain selesai menghitung posisi kamera!
[DefaultExecutionOrder(1000)]
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private Vector3 currentShakeOffset = Vector3.zero;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Panggil fungsi ini dari script manapun: CameraShake.instance.Shake(0.2f, 0.5f);
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Efek Decay: Getaran perlahan mereda (fade-out) agar tidak kaku
            float damper = 1f - (elapsed / duration);
            float currentMag = magnitude * damper;

            // Acak posisi relatif terhadap pandangan layar kamera
            float x = Random.Range(-1f, 1f) * currentMag;
            float y = Random.Range(-1f, 1f) * currentMag;

            // Simpan sebagai offset (bukan posisi absolut)
            currentShakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        currentShakeOffset = Vector3.zero;
    }

    // LateUpdate berjalan setelah Cinemachine memposisikan kamera
    void LateUpdate()
    {
        if (currentShakeOffset != Vector3.zero)
        {
            // Geser kamera searah pandangan lensa (Horizontal X dan Vertikal Y di layar)
            Vector3 worldOffset = (transform.right * currentShakeOffset.x) + (transform.up * currentShakeOffset.y);
            transform.position += worldOffset;
        }
    }
}