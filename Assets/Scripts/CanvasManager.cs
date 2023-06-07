using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;

    public GameObject[] panelsInOrder;
    public GameObject[] buttonsInOrder;
    public bool paused;
    public bool canPause;
    public bool keyboard;
    public MyButton selectedButton;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void PauseGame()
    {
        if (!canPause) return;

        paused = !paused;
        panelsInOrder[0].SetActive(paused);

        if (paused)
        {
            ChangeFocus(buttonsInOrder[0]);
            LockPauseButtons(false);
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            panelsInOrder[1].SetActive(false);
        }
    }

    public void PauseGame(PlayerInput input)
    {
        if (!canPause) return;

        // Assign player action scheme to the UI - (grants acces to the UI only to that player)
        FindObjectOfType<InputSystemUIInputModule>().actionsAsset = input.actions;
        var device = input.devices[0];
        if (device.name == "Keyboard") keyboard = true;
        else keyboard = false;

        PauseGame();
    }

    public void ChangeFocus(GameObject focus)
    {
        if (keyboard) return;
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(focus);
    }

    public void LockPauseButtons(bool doLock)
    {
        for (int i = 0; i < buttonsInOrder.Length; i++)
            buttonsInOrder[i].GetComponent<Button>().interactable = !doLock;
    }

    public void BackToMain()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        // Exit menues
        while (paused) GoBack();
        GameManager.instance.rounds = 0;
        GameManager.instance.StartCoroutine(GameManager.instance.EndGame(true));
        GameManager.instance.ReturnToMain();
    }

    public void Quit()
    {
        print("quit");
        Application.Quit();
    }

    public void GoBack()
    {
        // Unlock buttons
        LockPauseButtons(false);

        // Cycle through possible open menus and find the one last opened
        for (int i = panelsInOrder.Length - 1; i > 0; i--)
        {
            if (panelsInOrder[i].activeInHierarchy)
            {
                panelsInOrder[i].SetActive(false);
                ChangeFocus(buttonsInOrder[i]);
                return;
            }
        }

        instance.PauseGame();
    }
}
