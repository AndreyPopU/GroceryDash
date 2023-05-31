using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Windows;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager instance;

    public Color[] colors;
    public int colorIndex;

    public Player player;
    private Animator animator;

    private void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
    }

    public void ChangeColor()
    {
        colorIndex++;

        // Protect from out of bounds
        if (colorIndex < 0) colorIndex = colors.Length;
        else if (colorIndex >= colors.Length) colorIndex = 0;

        // Change player color
        player.color = colors[colorIndex];
        player.gfx.GetComponent<MeshRenderer>().material.color = player.color;

        // Set Controller Color to player color
        player.EnableController(true);
    }
}
