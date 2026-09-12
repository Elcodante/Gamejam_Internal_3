using UnityEngine;
using UnityEngine.EventSystems; // Wajib untuk mendeteksi kursor mouse
using System.Collections;

// Tambahkan IPointerEnterHandler dan IPointerExitHandler agar bisa merespons mouse
public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [Tooltip("Pengali ukuran saat kursor masuk (Contoh: 1.1 berarti membesar 10%)")]
    public float hoverMultiplier = 1.1f;

    [Tooltip("Kecepatan transisi animasi membesar/mengecil")]
    public float animationSpeed = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        // Simpan ukuran asli tombol saat game dimulai (misal: 3, 3, 3)
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    // Fungsi ini otomatis terpanggil saat kursor MOUSE MASUK ke area tombol
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverMultiplier;

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine());
    }

    // Fungsi ini otomatis terpanggil saat kursor MOUSE KELUAR dari area tombol
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine());
    }

    // Coroutine untuk memperhalus pergerakan ukuran (efek pegas/smooth)
    private IEnumerator ScaleRoutine()
    {
        // Terus jalankan sampai ukurannya nyaris sama dengan target
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            // Lerp memberikan efek deselerasi (melambat saat hampir sampai target)
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
            yield return null; // Tunggu frame berikutnya
        }

        // Kunci di ukuran target agar sangat presisi
        transform.localScale = targetScale;
    }

    // Tambahan: Pastikan ukuran kembali normal jika tombol dinonaktifkan secara tiba-tiba (misal panel ditutup)
    private void OnDisable()
    {
        transform.localScale = originalScale;
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
    }
}