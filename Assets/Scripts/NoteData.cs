using UnityEngine;

[CreateAssetMenu(fileName = "NewSongBeatmap", menuName = "RhythmGame/Beatmap")]
public class SongBeatmap : ScriptableObject
{
    public string songName;
    public float bpm;

    public NoteData[] notes;
}

[System.Serializable]
public class NoteData
{
    public float time; // Waktu dalam detik ketika nada harus dipukul
    public int lane;   // Jalur (misalnya 0 untuk kiri, 1 untuk tengah, 2 untuk kanan)
}
