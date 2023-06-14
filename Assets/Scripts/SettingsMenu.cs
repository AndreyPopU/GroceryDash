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
    public Toggle fullscreenToggle;

    Resolution[] resolutions;
    List<Resolution> relevantResolutions = new List<Resolution>();

    private void Start() => LoadSettings();

    public void FindResolution(bool saved)
    {
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentResolution = 0;
        if (saved) currentResolution = resolutionDropdown.index = PlayerPrefs.GetInt(SaveLoadManager.resolutionString);
        int i = 0;

        foreach (Resolution resolution in resolutions)
        {
            // Add only resolutions that match the current screen hertz
            if (resolution.refreshRate == Screen.currentResolution.refreshRate)
            {
                options.Add(resolution.width + " x " + resolution.height);
                relevantResolutions.Add(resolution);
                if (!saved) { if (resolution.width == Screen.width && resolution.height == Screen.height) currentResolution = i; }
                i++;
            }
        }

        // Update UI
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.UpdateCurrentOption(currentResolution);
        resolutionDropdown.RefreshShownValue();

        SetResolution();
    }

    public void LoadSettings()
    {
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
        if (PlayerPrefs.HasKey(SaveLoadManager.fullscreenString))
        {
            fullscreenToggle.isOn = SaveLoadManager.IntToBool(PlayerPrefs.GetInt(SaveLoadManager.fullscreenString));
            Screen.fullScreen = fullscreenToggle.isOn;
        }
        else fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void SaveSettings()
    {
        SetResolution();
        Screen.fullScreen = fullscreenToggle.isOn;
        SaveLoadManager.SaveSettings(soundSlider.index, musicSlider.index, SaveLoadManager.BoolToInt(fullscreenToggle.isOn), resolutionDropdown.index);
    }

    // Resolution
    public void SetResolution()
    {
        Resolution resolution = relevantResolutions[resolutionDropdown.index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
