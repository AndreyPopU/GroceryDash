using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public SmartDropdown resolutionDropdown;
    public SmartSlider soundSlider;
    public SmartSlider musicSlider;
    public GameObject settingsMenu;

    Resolution[] resolutions;

    private bool isFullscreen;

    private void Start()
    {
        Screen.fullScreen = isFullscreen;
        LoadSettings();
    }

    public void FindResolution(bool saved)
    {
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentResolution = 0;
        if (saved) currentResolution = resolutionDropdown.index = PlayerPrefs.GetInt(SaveLoadManager.resolutionString);

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (!saved) { if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) currentResolution = i; }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.UpdateCurrentOption(currentResolution);
        resolutionDropdown.RefreshShownValue();

        SetResolution();
    }

    public void LoadSettings()
    {
        // Quality
        //if (PlayerPrefs.HasKey(SaveLoadManager.qualityString)) qualityDropdown.UpdateCurrentOption(PlayerPrefs.GetInt(SaveLoadManager.qualityString));
        //else qualityDropdown.UpdateCurrentOption(0);
        //qualityDropdown.RefreshShownValue();
        //SetQuality();

        // Resolution
        FindResolution(PlayerPrefs.HasKey(SaveLoadManager.resolutionString));

        // Load Sound Value
        if (PlayerPrefs.HasKey(SaveLoadManager.soundString))
        {
            soundSlider.index = PlayerPrefs.GetInt(SaveLoadManager.soundString);
            soundSlider.ChangeValue(0);
            
        }

        // Load Music Value
        if (PlayerPrefs.HasKey(SaveLoadManager.musicString))
        {
            musicSlider.index = PlayerPrefs.GetInt(SaveLoadManager.musicString);
            musicSlider.ChangeValue(0);
        }

        // Fullscreen
        if (PlayerPrefs.HasKey(SaveLoadManager.fullscreenString)) isFullscreen = SaveLoadManager.IntToBool(PlayerPrefs.GetInt(SaveLoadManager.fullscreenString));
        else isFullscreen = Screen.fullScreen;
        Screen.fullScreen = isFullscreen;
    }

    public void SaveSettings()
    {
        SetResolution();
        //SetQuality();
        Screen.fullScreen = isFullscreen;
        SaveLoadManager.SaveSettings(soundSlider.index, musicSlider.index, SaveLoadManager.BoolToInt(isFullscreen), resolutionDropdown.index);
    }

    // Quality
    //public void SetQuality() => QualitySettings.SetQualityLevel(qualityDropdown.index);

    // Resolution
    public void SetResolution()
    {
        Resolution resolution = resolutions[resolutionDropdown.index];
        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
    }

    // Fullscreen
    public void Fullscreen() => isFullscreen = !isFullscreen;
}
