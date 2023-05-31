using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager instance;

    public MyButton selectedButton;
    public GameObject[] players;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        print("quit");
        Application.Quit();
    }

    public void DisplayPlayer(PlayerInput input)
    {
        players[input.playerIndex].SetActive(true);
        input.GetComponent<Player>().index = input.playerIndex;
    }
}
