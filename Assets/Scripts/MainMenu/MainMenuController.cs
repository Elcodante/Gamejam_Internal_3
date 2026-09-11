using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk pindah scene

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel; // Tempat memasukkan SettingsPanel dari Inspector

    private void Start()
    {
        settingsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Ucup");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game Terpanggil!");
        Application.Quit();
    }
}