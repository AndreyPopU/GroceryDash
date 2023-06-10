using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartDropdown : MonoBehaviour
{
    public int index;
    public int value;
    public List<string> options;
    public TextMeshProUGUI valueText;

    public void AddOptions(List<string> _options)
    {
        options.Clear();
        options.AddRange(_options);
    }

    public void UpdateCurrentOption(int current)
    {
        index = current;
    }

    public void RefreshShownValue()
    {
        valueText.text = options[index];   
    }

    public void ChangeValue(int desire)
    {
        // Update value and segments
        index += desire;

        // Protect out of bounds - Cycle instead
        if (index == options.Count) index = 0;
        if (index == -1) index = options.Count - 1;

        // Update text
        valueText.text = options[index];
    }
}
