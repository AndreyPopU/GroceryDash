using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameMode { Round, Elimination, Time, Race }

    public GameMode gameMode;
    [Header("Players")]
    public int playerCount;
    public List<Player> players;
    public bool roundStarted = true;
    public bool gameStarted = false;
    public bool paused;

    [Header("UI")]
    public ShoppingList shoppingList1;
    public ShoppingList shoppingList2;
    public GameObject[] canvasJoin;
    public GameObject disconnectedTextPrefab;
    
    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    public void SpawnPlayer(PlayerInput input)
    {
        input.transform.position = transform.GetChild(input.playerIndex).position;
        playerCount++;
        StartZone zone = FindObjectOfType<StartZone>();
        zone.playerCountText.text = zone.playerCount + "/" + playerCount;
        canvasJoin[input.playerIndex].SetActive(false);

        Player player = input.GetComponent<Player>();
        player.color = CustomizationManager.instance.colors[UnityEngine.Random.Range(0, 8)];
        players.Add(player);
    }

    public void BindShoppingList()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i == 0)
            {
                players[i].shoppingList = shoppingList1;
                shoppingList1.gameObject.SetActive(true);
            }
            else
            {
                players[i].shoppingList = shoppingList2;
                shoppingList2.gameObject.SetActive(true);
            }
        }
        LevelManager.instance.listsBound = true;
    }

    public void StartRound(bool start)
    {
        roundStarted = start;
        Player[] players = FindObjectsOfType<Player>();

        for (int i = 0; i < players.Length; i++) players[i].canMove = start;

        if (start)
            foreach (Player player in players)
                GenerateShoppingList(player);
        else StartCoroutine(TimeEnd());
    }

    public void PauseGame()
    {
        paused = !paused;

        if (paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public IEnumerator ScaleText(Transform text, int desire)
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        while (text.localScale.x < desire)
        {
            text.localScale += Vector3.one * .1f;
            yield return waitForFixedUpdate;
        }
    }

    private IEnumerator TimeEnd() // Invoked
    {
        Timer timer = GetComponent<Timer>();
        timer.enabled = false;
        timer.countdownText.transform.localScale = Vector3.zero;
        timer.countdownText.gameObject.SetActive(true);
        timer.roundText.gameObject.SetActive(false);
        yield return null;
        timer.countdownText.text = "Round Ended!";
        StartCoroutine(ScaleText(timer.countdownText.transform, 1));
    }

    public void GenerateShoppingList(Player player) // Individual
    {
        // Bind player to shopping list
        if (!LevelManager.instance.listsBound) BindShoppingList();

        // Assign products to buy based on GameMode
        if (gameMode == GameMode.Round)
        {
            // Randomize products
            player.shoppingList.shoppingItems.Add("Apple", 1);
            player.shoppingList.shoppingItems.Add("Milk", 2);
            player.shoppingList.shoppingItems.Add("Cheese", 1);
            player.shoppingList.shoppingItems.Add("Crab", 1);
            player.shoppingList.shoppingItems.Add("Fish", 2);
        }
        else if (gameMode == GameMode.Time)
        {

        }
        else if (gameMode == GameMode.Race)
        {

        }

        // Add items to shopping list UI
        foreach (KeyValuePair<string, int> pair in player.shoppingList.shoppingItems)
        {
            ShoppingItem item = Instantiate(player.shoppingList.itemPrefab, Vector3.zero, Quaternion.identity, player.shoppingList.contents).GetComponent<ShoppingItem>();
            item.transform.localPosition = new Vector3(0, player.shoppingList.offset, 0);
            item.SetText(pair.Value, pair.Key);
            player.shoppingList.items.Add(item);
            player.shoppingList.offset -= 50;
        }
    }

    public void GenerateShoppingList(Player[] players) // Teams
    {
        // What do we do in case of 3 players

        // Assign player.teammate
    }
}
