using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("In-Game HUD")]
    public TextMeshProUGUI hitFeedbackText;
    public TextMeshProUGUI hitScoreText;

    [Header("Countdown System")]
    public TextMeshProUGUI countdownText;
    public PathSystem.Runtime.PathController carController;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMissText;

    [Header("Pause Panel")]
    public GameObject pausePanel;

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
        if (pausePanel != null) pausePanel.SetActive(false);

        // Mulai hitung mundur saat scene dimuat
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        // 1. Tahan mobil agar tidak bergerak dulu
        if (carController != null) carController.Stop();

        if (countdownText != null) countdownText.gameObject.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
                countdownText.color = Color.white;

                // Efek pop sederhana untuk angka hitung mundur
                countdownText.transform.localScale = Vector3.one * 1.5f;
            }

            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.unscaledDeltaTime;
                if (countdownText != null && countdownText.transform.localScale.x > 1.01f)
                {
                    countdownText.transform.localScale = Vector3.Lerp(countdownText.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 10f);
                }
                yield return null;
            }
            count--;
        }

        // 2. Waktunya GO!
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = Color.yellow;
            countdownText.transform.localScale = Vector3.one * 1.5f;
        }

        // Lepas rem mobil!
        if (carController != null) carController.Play();

        // 3. Sembunyikan teks GO setelah 1 detik
        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
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
            AnimateTextPop();
        }
        else if (type == "Good")
        {
            combo = 0;
            score += 50;
            hitFeedbackText.text = "GOOD!";
            hitFeedbackText.color = Color.green;
            AnimateTextPop();
        }
        else if (type == "Miss")
        {
            combo = 0;
            missCount++;
            hitFeedbackText.text = "MISS!";
            hitFeedbackText.color = Color.red;
            AnimateTextPop();

            // PICU EFEK CAMERA SHAKE! (Pastikan script CameraShake sudah ada di scene)
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

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void AnimateTextPop()
    {
        if (hitFeedbackText == null) return;
        StopAllCoroutines();
        // Restart coroutine hitung mundur jika berbenturan (opsional, tapi aman)
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        hitFeedbackText.transform.localScale = Vector3.one * 1.5f;
        Vector3 targetScale = Vector3.one;

        while (hitFeedbackText.transform.localScale.x > 1.01f)
        {
            hitFeedbackText.transform.localScale = Vector3.Lerp(hitFeedbackText.transform.localScale, targetScale, Time.unscaledDeltaTime * 15f);
            yield return null;
        }
        hitFeedbackText.transform.localScale = targetScale;
    }

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
        if (resultPanel != null) resultPanel.SetActive(true);
        resultTitleText.text = isWin ? "YOU WIN" : "GAME OVER";
        resultTitleText.color = isWin ? Color.green : Color.red;
        resultScoreText.text = "Score: " + score.ToString("D10");
        resultMissText.text = "Miss: " + missCount + " / " + maxMissAllowed;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}