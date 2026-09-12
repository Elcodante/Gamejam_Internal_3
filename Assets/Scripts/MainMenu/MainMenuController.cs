using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject tutorialPanel;

    [Header("Animation Settings")]
    [Tooltip("Durasi animasi pop-up (dalam detik)")]
    public float animDuration = 0.25f;

    // Wadah untuk mencegah animasi bertumpuk jika tombol di-spam
    private Coroutine settingsAnimCoroutine;
    private Coroutine tutorialAnimCoroutine;

    private void Start()
    {
        // Pastikan semua panel tertutup saat awal
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Raka");
    }

    // ==========================================
    // LOGIKA ANIMASI SETTINGS PANEL
    // ==========================================
    public void OpenSettings()
    {
        if (settingsPanel == null) return;
        if (settingsAnimCoroutine != null) StopCoroutine(settingsAnimCoroutine);
        settingsAnimCoroutine = StartCoroutine(AnimateScale(settingsPanel, Vector3.zero, Vector3.one, true));
    }

    public void CloseSettings()
    {
        if (settingsPanel == null) return;
        if (settingsAnimCoroutine != null) StopCoroutine(settingsAnimCoroutine);
        settingsAnimCoroutine = StartCoroutine(AnimateScale(settingsPanel, Vector3.one, Vector3.zero, false));
    }

    // ==========================================
    // LOGIKA ANIMASI TUTORIAL PANEL
    // ==========================================
    public void OpenTutorial()
    {
        if (tutorialPanel == null) return;
        if (tutorialAnimCoroutine != null) StopCoroutine(tutorialAnimCoroutine);
        tutorialAnimCoroutine = StartCoroutine(AnimateScale(tutorialPanel, Vector3.zero, Vector3.one, true));
    }

    public void CloseTutorial()
    {
        if (tutorialPanel == null) return;
        if (tutorialAnimCoroutine != null) StopCoroutine(tutorialAnimCoroutine);
        tutorialAnimCoroutine = StartCoroutine(AnimateScale(tutorialPanel, Vector3.one, Vector3.zero, false));
    }

    // ==========================================
    // FUNGSI INTI ANIMASI POP-UP
    // ==========================================
    private IEnumerator AnimateScale(GameObject panel, Vector3 startScale, Vector3 targetScale, bool openPanel)
    {
        // Jika membuka, aktifkan objeknya dulu sebelum dibesarkan
        if (openPanel) panel.SetActive(true);

        panel.transform.localScale = startScale;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            // Rumus SmoothStep agar pergerakannya mulus (tidak kaku di awal/akhir)
            float smoothT = t * t * (3f - 2f * t);

            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            yield return null;
        }

        // Pastikan ukurannya pas di akhir animasi
        panel.transform.localScale = targetScale;

        // Jika menutup, nonaktifkan objeknya setelah mengecil ke 0
        if (!openPanel) panel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Terpanggil!");
        Application.Quit();
    }
}