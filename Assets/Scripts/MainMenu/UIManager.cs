using UnityEngine;
using TMPro;

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
        if (hitScoreText != null) hitScoreText.text = "000000";
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
        }
        else if (type == "Good")
        {
            combo = 0;
            score += 50;
            hitFeedbackText.text = "GOOD!";
            hitFeedbackText.color = Color.green;
        }
        else if (type == "Miss")
        {
            combo = 0;
            missCount++;
            hitFeedbackText.text = "MISS!";
            hitFeedbackText.color = Color.red;

            if (missCount >= maxMissAllowed)
            {
                TriggerGameOver();
            }
        }

        hitScoreText.text = score.ToString("D6");
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
        resultPanel.SetActive(true);
        resultTitleText.text = isWin ? "YOU WIN" : "GAME OVER";
        resultTitleText.color = isWin ? Color.green : Color.red;
        resultScoreText.text = "Score: " + score.ToString("D6");
        resultMissText.text = "Miss: " + missCount + " / " + maxMissAllowed;
    }
}