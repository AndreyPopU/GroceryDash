using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShoppingItem : MonoBehaviour
{
    [HideInInspector]
    public TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(int value, string key)
    {
        text.text = value + " " + key;
    }
}
