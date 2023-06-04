using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class SaveLoadManager
{
    // Settings
    public static int qualityValue;
    public static string qualityString = "qualityValue";
    public static int resolutionValue;
    public static string resolutionString = "resolutionValue";
    public static int sound;
    public static string soundString = "sound";
    public static int music;
    public static string musicString = "music";
    public static int fullscreen;
    public static string fullscreenString = "fullscreen";

    // Resolution, quality, sound, fullscreen
    public static void SaveSettings(int _sound, int _music, int _fullscreen, int _resolution, int _quality)
    {
        PlayerPrefs.SetInt(qualityString, _quality);
        PlayerPrefs.SetInt(resolutionString, _resolution);
        PlayerPrefs.SetInt(soundString, _sound);
        PlayerPrefs.SetInt(musicString, _music);
        PlayerPrefs.SetInt(fullscreenString, _fullscreen);

        Debug.Log("Saved");
    }

    public static bool IntToBool(int convert)
    {
        if (convert == 1) return true;
        return false;
    }

    public static int BoolToInt(bool convert)
    {
        if (convert) return 1;
        return 0;
    }
}
