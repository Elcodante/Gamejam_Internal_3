using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Wajib ditambahkan untuk menjalankan animasi (Coroutine)

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject tutorialPanel;

    [Header("Animation Settings")]
    [Tooltip("Waktu yang dibutuhkan untuk panel membesar/mengecil (detik)")]
    public float animDuration = 0.25f;
    private Coroutine tutorialAnimCoroutine;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Raka");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ==========================================
    // LOGIKA TUTORIAL PANEL (DENGAN ANIMASI)
    // ==========================================
    public void OpenTutorial()
    {
        if (tutorialPanel == null) return;

        // Hentikan animasi sebelumnya jika tombol ditekan berulang-ulang dengan cepat
        if (tutorialAnimCoroutine != null) StopCoroutine(tutorialAnimCoroutine);

        // Mulai animasi membesar (dari skala 0 ke 1)
        tutorialAnimCoroutine = StartCoroutine(AnimateScale(tutorialPanel, Vector3.zero, Vector3.one, true));
    }

    public void CloseTutorial()
    {
        if (tutorialPanel == null) return;

        // Mulai animasi mengecil (dari skala 1 ke 0)
        if (tutorialAnimCoroutine != null) StopCoroutine(tutorialAnimCoroutine);
        tutorialAnimCoroutine = StartCoroutine(AnimateScale(tutorialPanel, Vector3.one, Vector3.zero, false));
    }

    // Fungsi inti untuk menggerakkan ukuran panel secara mulus
    private IEnumerator AnimateScale(GameObject panel, Vector3 startScale, Vector3 targetScale, bool openPanel)
    {
        if (openPanel) panel.SetActive(true);

        panel.transform.localScale = startScale;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            // Rumus SmoothStep: Membuat awalan dan akhiran animasi lebih halus (tidak kaku)
            float smoothT = t * t * (3f - 2f * t);

            panel.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            yield return null; // Tunggu ke frame berikutnya
        }

        panel.transform.localScale = targetScale;

        // Jika perintahnya untuk menutup, matikan panel setelah ukurannya 0
        if (!openPanel) panel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Terpanggil!");
        Application.Quit();
    }
}