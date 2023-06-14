using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Windows;
using static Unity.VisualScripting.Member;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager instance;

    public List<Color> colors;
    public List<GameObject> hats;
    public string[] colorNames;
    public int colorIndex;
    public int hatIndex;

    public Player player;
    private Animator animator;
    private AudioSource source;

    private void Awake()
    {
        // Singleton
        if (instance == null) instance = this;
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        source = GetComponent<AudioSource>();

        // If player are already connected remove their colors from the possible colors
        if (GameManager.instance.players.Count > 0)
        {
            foreach (Player player in GameManager.instance.players)
            {
                colors.Remove(player.color);
                player.teamText.text = "";
            }
        }

        CanvasManager.instance.canPause = true;
    }

    public void ChangeHat()
    {
        hatIndex++;

        // Protect from out of bounds
        if (hatIndex >= hats.Count) hatIndex = 0;

        // Remove previous hat
        Destroy(player.hat);

        // Change player hat
        GameObject hat = Instantiate(hats[hatIndex], player.hatPosition.position, Quaternion.identity);
        hat.transform.SetParent(player.hatPosition);
        hat.transform.localRotation = Quaternion.identity;
        player.hat = hat;
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
        player.SetColor();
        colors.RemoveAt(colorIndex + 1);

        // Set Controller Color to player color
        player.EnableController(true);
    }

    public void PlaySound(AudioClip sound)
    {
        source.clip = sound;
        source.Play();
    }
}
