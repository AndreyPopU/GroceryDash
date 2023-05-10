using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager instance;

    public Color[] colors;
    public int colorIndex;

    public Player player;

    private void Awake() => instance = this;

    public void ChangeColor()
    {
        colorIndex++;

        // Protect from out of bounds
        if (colorIndex < 0) colorIndex = colors.Length;
        else if (colorIndex >= colors.Length) colorIndex = 0;

        // Change player color
        player.color = colors[colorIndex];
        player.gfx.GetComponent<MeshRenderer>().material.color = player.color;
    }
}
