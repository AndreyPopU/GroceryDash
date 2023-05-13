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

    [Header("GameMode")]
    public GameMode gameMode;
    public int rounds = 3;
    public TextMeshProUGUI resultText;

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
    
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SpawnPlayer(PlayerInput input)
    {
        // Spawn player at spawnpoint, update start zone player number and disable join spot
        input.transform.position = transform.GetChild(input.playerIndex).position;
        playerCount++;
        StartZone zone = FindObjectOfType<StartZone>();
        zone.playerCountText.text = zone.playerCount + "/" + playerCount;
        canvasJoin[input.playerIndex].SetActive(false);

        // Assign random color and add to list of players
        Player player = input.GetComponent<Player>();
        player.color = CustomizationManager.instance.colors[UnityEngine.Random.Range(0, 8)];
        players.Add(player);

        // Add to camera follow targets
        CameraManager.instance.targets.Add(player.transform);
    }

    public void BindShoppingList()
    {
        if (gameMode == GameMode.Round) // Teammode
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
        }
        else if (gameMode == GameMode.Race || gameMode == GameMode.Elimination) // Completely shared list
        {
            foreach (Player player in players) // Bind every player to one shopping list
            {
                player.shoppingList = shoppingList1;
                shoppingList1.gameObject.SetActive(true);
            }
            shoppingList1.shared = true;
        }
        else if (gameMode == GameMode.Race) // Shared list
        {

        }

        LevelManager.instance.listsBound = true;
    }

    public void StartRound(bool start)
    {
        roundStarted = start;

        for (int i = 0; i < players.Count; i++) players[i].canMove = start;

        if (start) GenerateShoppingList();
        else StartCoroutine(EndGame());
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

        if (text.localScale.x < desire)
        {
            while (text.localScale.x < desire)
            {
                text.localScale += Vector3.one * .1f;
                yield return waitForFixedUpdate;
            }
        }
        else
        {
            while (text.localScale.x > desire)
            {
                text.localScale -= Vector3.one * .1f;
                yield return waitForFixedUpdate;
            }
        }

        text.localScale = Vector3.one * desire;
    }

    private IEnumerator EndGame() // Invoked
    {
        Timer timer = GetComponent<Timer>();
        timer.enabled = false;
        resultText.gameObject.SetActive(true);
        resultText.transform.localScale = Vector3.zero;
        timer.roundText.gameObject.SetActive(false);
        yield return null;
        resultText.text = "Round " + (4 - rounds) + " Ended!";
        StartCoroutine(ScaleText(resultText.transform, 1));

        if (rounds > 0)
        {
            // Clear Shopping lists
            foreach (Player player in players)
            {
                foreach (ShoppingItem item in player.shoppingList.items)
                    Destroy(item.gameObject);

                player.shoppingList.shoppingItems.Clear();
                player.shoppingList.items.Clear();
                player.shoppingList.offset = 100;
                player.canMove = false;
            }

            // Stop game
            GameManager.instance.roundStarted = false;

            yield return new WaitForSeconds(2);

            StartCoroutine(ScaleText(resultText.transform, 0));

            yield return new WaitForSeconds(1);

            // Reset player Positions
            LevelManager.instance.PreparePlayers();

            // Set up timer
            timer.countdownText.transform.localScale = Vector3.zero;
            timer.countdownText.gameObject.SetActive(true);
            StartCoroutine(ScaleText(timer.countdownText.transform, 1));
            timer.currentTime = 3;
            timer.enabled = true;

            yield return new WaitForSeconds(3);

            // (Timer starts next round)
            rounds--;
        }
    }

    public void GenerateShoppingList() // Individual
    {
        // Bind player to shopping list
        if (!LevelManager.instance.listsBound) BindShoppingList();

        // Assign products to buy based on GameMode
        if (gameMode == GameMode.Round) // Teammode
        {
            // Randomize products in both teams' shopping lists

            // Team 1
            shoppingList1.shoppingItems.Add("Apple", 1);
            shoppingList1.shoppingItems.Add("Milk", 2);
            shoppingList1.shoppingItems.Add("Cheese", 1);

            // Team 2
            shoppingList2.shoppingItems.Add("Apple", 1);
            shoppingList2.shoppingItems.Add("Milk", 2);
            shoppingList2.shoppingItems.Add("Cheese", 1);
        }
        else if (gameMode == GameMode.Race || gameMode == GameMode.Elimination) // Completely shared list
        {
            // Randomize products
            shoppingList1.shoppingItems.Add("Cheese", 1);
            shoppingList1.shoppingItems.Add("Crab", 1);
            shoppingList1.shoppingItems.Add("Fish", 2);
        }
        else if (gameMode == GameMode.Race) // Shared list
        {

        }

        // Add items to shopping list UI
        foreach (KeyValuePair<string, int> pair in shoppingList1.shoppingItems)
        {
            ShoppingItem item = Instantiate(shoppingList1.itemPrefab, Vector3.zero, Quaternion.identity, shoppingList1.contents).GetComponent<ShoppingItem>();
            item.transform.localPosition = new Vector3(0, shoppingList1.offset, 0);
            item.SetText(pair.Value, pair.Key);
            shoppingList1.items.Add(item);
            shoppingList1.offset -= 50;
        }

        if (shoppingList2.gameObject.activeInHierarchy) // If second list is active, fill it too
        {
            // Add items to shopping list UI
            foreach (KeyValuePair<string, int> pair in shoppingList2.shoppingItems)
            {
                ShoppingItem item = Instantiate(shoppingList2.itemPrefab, Vector3.zero, Quaternion.identity, shoppingList2.contents).GetComponent<ShoppingItem>();
                item.transform.localPosition = new Vector3(0, shoppingList2.offset, 0);
                item.SetText(pair.Value, pair.Key);
                shoppingList2.items.Add(item);
                shoppingList2.offset -= 50;
            }
        }
        
    }

    public void GenerateShoppingList(Player[] players) // Teams
    {
        // What do we do in case of 3 players

        // Assign player.teammate
    }
}
