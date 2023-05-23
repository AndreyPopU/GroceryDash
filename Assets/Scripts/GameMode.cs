using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameMode : MonoBehaviour
{
    public static GameMode instance;

    public TextMeshProUGUI modeText;
    public TextMeshProUGUI descriptionText;

    private string[] modeDescriptions;
    private int index;

    private void Awake() => instance = this;

    private void Start()
    {
        modeDescriptions = new string[]
        {
            "The first team to complete their shopping list wins. \n Best of 3.",
            "Ramping difficulty game mode where players that fail to complete their shopping list get eliminated.",
            "Players share one shopping list to complete but not the purchases. First to complete the shopping list wins. \n 3 Rounds.",
            "Players shop individually and share a shopping list. Get points by buying an item that is still on the shopping list. \n 3 Shopping Lists to complete."
        };

        descriptionText.text = modeDescriptions[index];
    }

    public void ChangeMode()
    {
        index++;

        // Protect from out of bounds
        if (index > 3) index = 0;

        // Change Game Mode
        switch (index)
        {
            case 0: GameManager.instance.gameMode = GameManager.GameMode.Round; break;
            case 1: GameManager.instance.gameMode = GameManager.GameMode.Elimination; break;
            case 2: GameManager.instance.gameMode = GameManager.GameMode.Race; break;
            case 3: GameManager.instance.gameMode = GameManager.GameMode.Time; break;
        }

        // Visual Change
        modeText.text = "Gamemode \n" + GameManager.instance.gameMode.ToString();
        descriptionText.text = modeDescriptions[index];
    }
}
