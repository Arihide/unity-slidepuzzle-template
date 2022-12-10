using UnityEngine;

public static class SlidePuzzleData
{
    public const string TotalClearCountKey = "TotalClearCount";

    public static void AddClearCount(int size)
    {
        string key = $"ClearCount{size}";
        int clearCount = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, clearCount);
        PlayerPrefs.SetInt(TotalClearCountKey, PlayerPrefs.GetInt(TotalClearCountKey, 0) + 1);
        PlayerPrefs.Save();
    }

    public static int GetClearCount()
    {
        return PlayerPrefs.GetInt(TotalClearCountKey, 0);
    }

    public static int GetClearCount(int size)
    {
        string key = $"ClearCount{size}";
        return PlayerPrefs.GetInt(key, 0);
    }
}