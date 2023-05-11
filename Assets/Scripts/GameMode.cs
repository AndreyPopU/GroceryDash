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
    public string[] modeDescriptions;

    private int index;

    private void Awake() => instance = this;

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
