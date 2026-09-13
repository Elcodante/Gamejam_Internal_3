using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public string bgmID = "MenuBGM"; // Pastikan nama ini SAMA PERSIS dengan ID BGM di AudioManager Anda

    void Start()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(bgmID);
        }
    }
}
