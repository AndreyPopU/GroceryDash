using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameMode { Round, Elimination, Time, Race }

    [Header("Analytics")]
    public int basketsUsed;
    public int productsUsed;
    public bool listCompleted;
    public int scans;
    public int dashes;
    public int bumps;

    [Header("GameMode")]
    public GameMode gameMode;
    public int rounds = 3;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;

    [Header("Players")]
    public int playerCount;
    public List<Player> players;
    public List<Player> winners;
    public bool roundStarted = true;
    public bool gameStarted = false;

    [Header("UI")]
    public ShoppingList shoppingList1;
    public ShoppingList shoppingList2;
    public GameObject disconnectedTextPrefab;
    public List<GameObject> joinCanvas;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (!Application.isEditor) // Initialize environment "release"
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName("release");
            UnityServices.InitializeAsync(options);
        }
        else UnityServices.InitializeAsync(); // Initialize default environment "production"

        DontDestroyOnLoad(gameObject);
    }

    public void SpawnPlayer(PlayerInput input)
    {
        // Return if not in Main Menu
        if (SceneManager.GetActiveScene().buildIndex != 0) return;

        // Spawn player at spawnpoint, update start zone player number and disable join spot
        input.transform.position = transform.GetChild(input.playerIndex).position;
        playerCount++;
        StartZone zone = FindObjectOfType<StartZone>();
        zone.playerCountText.text = zone.playerCount + "/" + playerCount;
        joinCanvas[input.playerIndex].SetActive(false);

        // Assign random color and add to list of players
        int randomColor = UnityEngine.Random.Range(0, 8);
        Player player = input.GetComponent<Player>();
        player.color = CustomizationManager.instance.colors[randomColor];
        player.colorName = CustomizationManager.instance.colorNames[randomColor];
        players.Add(player);

        // Add to camera follow targets
        CameraManager.instance.targets.Add(player.transform);

        // Set Controller Color to player color
        player.EnableController(true);
    }

    public void DisconnectPlayer(Player player)
    {
        // Enable join canvas if in Main Menu
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            joinCanvas[player.index].gameObject.SetActive(true);
        }

        CameraManager.instance.targets.Remove(player.transform);
        players.Remove(player);
        Destroy(player.gameObject);
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
                    shoppingList1.owners.Add(players[i]);
                    shoppingList1.gameObject.SetActive(true);
                }
                else
                {
                    players[i].shoppingList = shoppingList2;
                    shoppingList2.owners.Add(players[i]);
                    shoppingList2.gameObject.SetActive(true);
                }
            }
        }
        else if (gameMode == GameMode.Time || gameMode == GameMode.Elimination) // Completely shared list
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

        for (int i = 0; i < players.Count; i++)
        {
            players[i].canMove = start;
            players[i].canDash = start;
        }

        if (start) StartGame();
        else StartCoroutine(EndGame(false));
    }

    private void Update()
    {
        Camera.main.ViewportToScreenPoint(UnityEngine.Input.mousePosition);
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

    public IEnumerator EndGame(bool playerInvoked)
    {
        if (!playerInvoked)
        {
            resultText.gameObject.SetActive(true);
            resultText.transform.localScale = Vector3.zero;
        }
        else FadePanel.instance.Fade(1);

        CanvasManager.instance.canPause = false;

        Timer timer = GetComponent<Timer>();
        timer.enabled = false;
        timer.roundText.gameObject.SetActive(false);

        // Display winners
        if (winners.Count > 0 && !playerInvoked)
        {
            Player winner = winners[0];

            if (winners.Count > 1)
            {
                scoreText.color = Color.white;
                scoreText.text = winner.shoppingList.team + " Wins!";
            }
            else
            {
                scoreText.color = winners[0].color;
                scoreText.text = winner.colorName + " Wins!";
            }
        }

        yield return null;

        if (!playerInvoked)
        {
            resultText.text = "Round " + (4 - rounds) + " Ended!";
            StartCoroutine(ScaleText(resultText.transform, 1));
        }

        // Players drop items
        foreach (Player player in players)
        {
            if (player.holdBasket != null) player.PickUpBasket(false);
            if (player.holdProduct != null) player.PickUpProduct(false);
            player.SlowDown(false);
        }

        rounds--;

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
        roundStarted = false;

        yield return new WaitForSeconds(1);

        if (!playerInvoked)
        {
            yield return new WaitForSeconds(1);

            StartCoroutine(ScaleText(resultText.transform, 0));
            FadePanel.instance.Fade(1);

            yield return new WaitForSeconds(1);
        }
            
        // If there are still rounds to be played reload the level
        if (rounds > 0) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
        {
            // Return to main menu
            shoppingList1.gameObject.SetActive(false);
            shoppingList2.gameObject.SetActive(false);
            rounds = 3;

            SceneManager.LoadScene(0);

            foreach (Player player in players)
            {
                player.transform.position = transform.GetChild(player.index).position;
                player.transform.rotation = Quaternion.identity;
                player.gfx.transform.rotation = Quaternion.identity;
                player.canMove = true;
                player.canDash = true;
                player.rb.velocity = Vector3.zero;
                player.score = 0;
            }

            UpdateJoinSpots();
            FadePanel.instance.Fade(0);
        }
        CanvasManager.instance.canPause = true;
    }

    public void StartGame() // Individual
    {
        // Bind player to shopping list
        if (!LevelManager.instance.listsBound) BindShoppingList();

        // Reset winners
        winners.Clear();

        // Clear Items
        shoppingList1.shoppingItems.Clear();
        shoppingList2.shoppingItems.Clear();

        // Assign products to buy based on GameMode
        if (gameMode == GameMode.Round) // Teammode
        {
            // Randomize products in both teams' shopping lists

            // Team 1
            shoppingList1.shoppingItems.Add("Apple", 1);
            //shoppingList1.shoppingItems.Add("Cheese", 1);
            //shoppingList1.shoppingItems.Add("Crab", 1);
            //shoppingList1.shoppingItems.Add("Fish", 2);

            // Team 2
            shoppingList2.shoppingItems.Add("Water", 1);
            shoppingList2.shoppingItems.Add("Cheese", 1);
            shoppingList2.shoppingItems.Add("Crab", 1);
            shoppingList2.shoppingItems.Add("Fish", 2);
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

    public void UpdateJoinSpots()
    {
        for (int i = 0; i < joinCanvas.Count; i++)
            joinCanvas[i].SetActive(true);

        for (int i = 0; i < players.Count; i++)
            joinCanvas[i].SetActive(false);
    }
}
