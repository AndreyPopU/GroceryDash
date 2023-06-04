using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SmartSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public int index;
    public int value;
    public int valuePerSegment;
    public GameObject[] segments;

    public void ChangeValue(int desire)
    {
        // Protect out of bounds
        if (desire > 0 && index == 9) return;
        if (desire < 0 && index == -1) return;

        // Update value and segments
        if (desire < 0) segments[index].SetActive(false);
        index += desire;
        value = index * valuePerSegment + valuePerSegment - 80;
        mixer.SetFloat("volume", value);
        if (desire > 0) segments[index].SetActive(true);

        // Update the slider segments
        if (desire == 0)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (i <= index) segments[i].SetActive(true);
                else segments[i].SetActive(false);
            }
        }
    }
}
