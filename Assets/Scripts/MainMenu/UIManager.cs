using UnityEngine;
using TMPro;
using System.Collections; // Wajib ditambahkan untuk Coroutine

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("In-Game HUD")]
    public TextMeshProUGUI hitFeedbackText;
    public TextMeshProUGUI hitScoreText;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMissText;

    private int score = 0;
    private int combo = 0;
    private int missCount = 0;
    private int maxMissAllowed = 10;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (hitFeedbackText != null) hitFeedbackText.text = "";
        if (hitScoreText != null) hitScoreText.text = "0000000000";
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void RegisterHit(string type)
    {
        if (type == "Perfect")
        {
            if (combo == 0) combo = 2;
            else combo += 2;

            score += 200 * combo;
            hitFeedbackText.text = $"PERFECT!\nx{combo}";
            hitFeedbackText.color = Color.cyan;
            AnimateTextPop(); // Panggil Animasi Pop
        }
        else if (type == "Good")
        {
            combo = 0;
            score += 50;
            hitFeedbackText.text = "GOOD!";
            hitFeedbackText.color = Color.green;
            AnimateTextPop(); // Panggil Animasi Pop
        }
        else if (type == "Miss")
        {
            combo = 0;
            missCount++;
            hitFeedbackText.text = "MISS!";
            hitFeedbackText.color = Color.red;
            AnimateTextPop(); // Panggil Animasi Pop

            // PICU EFEK CAMERA SHAKE! (Durasi 0.15 detik, Kekuatan 0.2)
            if (CameraShake.instance != null)
            {
                CameraShake.instance.Shake(0.15f, 0.2f);
            }

            if (missCount >= maxMissAllowed)
            {
                TriggerGameOver();
            }
        }

        hitScoreText.text = score.ToString("D10");
    }

    // --- BARU: LOGIKA ANIMASI POP TEXT ---
    private void AnimateTextPop()
    {
        // Pastikan teksnya tidak null sebelum mencoba dianimasikan
        if (hitFeedbackText == null) return;

        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        // 1. Teks langsung membesar 1.5x lipat seketika
        hitFeedbackText.transform.localScale = Vector3.one * 1.5f;

        Vector3 targetScale = Vector3.one;

        // 2. Teks menyusut perlahan ke ukuran normal (1x)
        while (hitFeedbackText.transform.localScale.x > 1.01f)
        {
            // Lerp digunakan agar pergerakannya mulus (smooth)
            hitFeedbackText.transform.localScale = Vector3.Lerp(hitFeedbackText.transform.localScale, targetScale, Time.unscaledDeltaTime * 15f);
            yield return null; // Tunggu frame selanjutnya
        }

        // 3. Pastikan angkanya benar-benar bulat kembali ke 1 di akhir
        hitFeedbackText.transform.localScale = targetScale;
    }
    // ------------------------------------

    private void TriggerGameOver()
    {
        ShowResult(false);
        Time.timeScale = 0f;

        if (SongManager.instance != null && SongManager.instance.musicSource != null)
        {
            SongManager.instance.musicSource.Stop();
        }
    }

    public void ShowResult(bool isWin)
    {
        resultPanel.SetActive(true);
        resultTitleText.text = isWin ? "YOU WIN" : "GAME OVER";
        resultTitleText.color = isWin ? Color.green : Color.red;
        resultScoreText.text = "Score: " + score.ToString("D6");
        resultMissText.text = "Miss: " + missCount + " / " + maxMissAllowed;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // WAJIB ADA agar game tidak beku saat di-restart
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}