using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Windows;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager instance;

    public List<Color> colors;
    public string[] colorNames;
    public int colorIndex;

    public Player player;
    private Animator animator;

    private void Awake()
    {
        if (instance == null) instance = this;
        animator = GetComponent<Animator>();

        foreach (Player player in GameManager.instance.players)
        {
            colors.Remove(player.color);
            player.teamText.text = "";
        }

        CanvasManager.instance.canPause = true;
    }

    public void ChangeColor()
    {
        colorIndex++;

        // Protect from out of bounds
        if (colorIndex < 0) colorIndex = colors.Count;
        else if (colorIndex >= colors.Count) colorIndex = 0;

        // Add previous color of player to available colors
        colors.Insert(colorIndex, player.color);

        // Change player color
        player.color = colors[colorIndex + 1];
        player.colorName = colorNames[colorIndex + 1];
        player.gfx.GetComponent<MeshRenderer>().material.color = player.color;
        colors.RemoveAt(colorIndex + 1);

        // Set Controller Color to player color
        player.EnableController(true);
    }
}
