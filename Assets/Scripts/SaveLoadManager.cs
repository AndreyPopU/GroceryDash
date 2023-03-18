using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveLoadManager
{
    // Settings
    public static int qualityValue;
    public static string qualityString = "qualityValue";
    public static int resolutionValue;
    public static string resolutionString = "resolutionValue";
    public static float sound;
    public static string soundString = "sound";
    public static int fullscreen;
    public static string fullscreenString = "fullscreen";

    // Resolution, quality, sound, fullscreen
    public static void SaveSettings(float _sound, int _fullscreen, int _resolution, int _quality)
    {
        PlayerPrefs.SetInt(qualityString, _quality);
        PlayerPrefs.SetInt(resolutionString, _resolution);
        PlayerPrefs.SetFloat(soundString, _sound);
        PlayerPrefs.SetInt(fullscreenString, _fullscreen);
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
