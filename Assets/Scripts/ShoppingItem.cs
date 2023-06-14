using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingItem : MonoBehaviour
{
    [HideInInspector] public TextMeshProUGUI text;
    public Image icon;
    private Toggle toggle;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        toggle = GetComponentInChildren<Toggle>();
    }

    public void SetText(int value, string key)
    {
        icon.sprite = Resources.Load<Sprite>(key + "Icon");

        if (value > 1) text.text = value + " " + key;
        else if (value == 1) text.text = key;
        else
        {
            text.text = "<s>" + key + "</s>";
            toggle.isOn = true;
        }
    }
}
