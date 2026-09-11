using UnityEngine;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance { get; private set; }

    public AudioSource musicSource;

    private double dspSongTime;

    public float songPosition;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        dspSongTime = AudioSettings.dspTime;

        musicSource.Play();
    }

    private void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
    }
}

