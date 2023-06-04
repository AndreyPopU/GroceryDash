using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;

    public GameObject[] panelsInOrder;
    public GameObject[] buttonsInOrder;
    public bool paused;
    public bool canPause;
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

        if (paused) ChangeFocus(buttonsInOrder[0]);
        else panelsInOrder[1].SetActive(false);
        //if (paused) Time.timeScale = 0;
        //else Time.timeScale = 1;
    }

    public void ChangeFocus(GameObject focus)
    {
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
    }

    public void Quit()
    {
        print("quit");
        Application.Quit();
    }

    public void GoBack()
    {
        print("Called");
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
