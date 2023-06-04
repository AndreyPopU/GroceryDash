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
        // Protect out of bounds - Cycle instead
        if (desire > 0 && index == options.Count - 1) index = -1;
        if (desire < 0 && index == 0) index = options.Count;

        // Update value and segments
        index += desire;
        valueText.text = options[index];
    }
}
