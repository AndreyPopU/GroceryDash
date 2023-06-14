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
using static Unity.VisualScripting.Member;

public class GameManager : MonoBehaviour
{
    public InputAction joinAction;

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
    public GameObject resultPanel;
    public TextMeshProUGUI scoreResultText;
    public GameObject returnToMain;
    public GameObject logo;
    public Color inkColor;
    public Player keyboardPlayer;

    private PlayerInputManager inputManager;

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

        // Deal with input
        inputManager = FindObjectOfType<PlayerInputManager>();
        joinAction.Enable();
        joinAction.performed += context => JoinAction(context);
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

        // Assign random color and hat, and add to list of players
        int randomHat = UnityEngine.Random.Range(0, CustomizationManager.instance.hats.Count);
        int randomColor = UnityEngine.Random.Range(0, CustomizationManager.instance.colors.Count);
        Player player = input.GetComponent<Player>();
        player.index = input.playerIndex;

        // Hat
        GameObject hat = Instantiate(CustomizationManager.instance.hats[randomHat], player.hatPosition.position, Quaternion.identity);
        hat.transform.SetParent(player.hatPosition);
        hat.transform.localRotation = Quaternion.identity;
        player.hat = hat;

        // Color
        player.color = CustomizationManager.instance.colors[randomColor];
        player.colorName = CustomizationManager.instance.colorNames[randomColor];
        CustomizationManager.instance.colors.RemoveAt(randomColor);

        // Add to camera follow targets
        players.Add(player);
        CameraManager.instance.targets.Add(player.transform);

        // Set Controller Color to player color
        player.EnableController(true);

        // Keep track of keyboard player
        var device = input.devices[0];
        if (device.name.ToString() == "Keyboard") keyboardPlayer = player;
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
        shoppingList1.owners.Clear();
        shoppingList2.owners.Clear();

        if (gameMode == GameMode.Round) // Teammode
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (i % 2 == 0) // Even - go to team 1
                {
                    players[i].shoppingList = shoppingList1;
                    players[i].teamText.text = "Team 1";
                    shoppingList1.owners.Add(players[i]);
                    shoppingList1.ownerIcons[shoppingList1.owners.Count - 1].color = shoppingList1.owners[shoppingList1.owners.Count - 1].color;
                    shoppingList1.gameObject.SetActive(true);
                }
                else // Odd - go to team 2
                {
                    players[i].shoppingList = shoppingList2;
                    players[i].teamText.text = "Team 2";
                    shoppingList2.owners.Add(players[i]);
                    shoppingList2.ownerIcons[shoppingList2.owners.Count - 1].color = shoppingList2.owners[shoppingList2.owners.Count - 1].color;
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
            players[i].milkEffect.Stop();
            players[i].inMilk = false;
        }

        if (start) StartGame();
        else StartCoroutine(EndGame(false));
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
        if (CanvasManager.instance.paused) CanvasManager.instance.PauseGame();
        CanvasManager.instance.canPause = false;
        Timer timer = GetComponent<Timer>();
        timer.enabled = false;
        timer.roundText.gameObject.SetActive(false);

        // Display winners
        if (!playerInvoked)
        {
            LevelManager.instance.EndRound();
            resultText.gameObject.SetActive(true);
            resultText.transform.localScale = Vector3.zero;

            if (winners.Count > 0)
            {
                Player winner = winners[0];

                if (gameMode == GameMode.Round)
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
            else
            {
                scoreText.color = Color.white;
                scoreText.text = "Tie!";
            }
            
        }

        yield return null;

        if (!playerInvoked)
        {
            resultText.text = "Round " + (4 - rounds) + " Ended!";
            StartCoroutine(ScaleText(resultText.transform, 1));
        }
        else FadePanel.instance.Fade(1);

        // Players drop items
        foreach (Player player in players)
        {
            if (player.holdBasket != null)
            {
                player.holdBasket.canPickUp = false;
                player.LaunchBasket(player.gfx.forward);
            }
            if (player.holdProduct != null)
            {
                player.holdProduct.canPickUp = false;
                player.LaunchProduct(player.gfx.forward);
            }
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

            yield return new WaitForSeconds(1.5f);
        }

        // If there are still rounds to be played reload the level
        if (rounds > 0)
        {
            FadePanel.instance.Fade(1);
            yield return new WaitForSeconds(1.25f);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            CanvasManager.instance.canPause = true;
        }
        else
        {
            shoppingList1.gameObject.SetActive(false);
            shoppingList2.gameObject.SetActive(false);
            rounds = 3;
            CanvasManager.instance.ChangeFocus(returnToMain);

            // Display Results
            resultPanel.SetActive(true);

            // Give UI priority to the player with Keyboard&Mouse
            foreach (Player player in players)
            {
                var device = player.input.devices[0];
                if (device.name == "Keyboard")
                {
                    FindObjectOfType<InputSystemUIInputModule>().actionsAsset = player.input.actions;
                    break;
                }
            }
        
            // Winner is the team with most points
            int team1Total = 0; 
            int team2Total = 0;

            foreach (Player player in players)
            {
                if (player.shoppingList.team.Contains("1")) team1Total += player.score; 
                else team2Total += player.score;
            }

            scoreResultText.color = inkColor;
            if (team1Total == team2Total) scoreResultText.text = "Tie!";
            else if (team1Total > team2Total) scoreResultText.text = "Team 1 Wins!";
            else scoreResultText.text = "Team 2 Wins!";
        }
    }

    public void ReturnToMain()
    {
        // Return to main menu
        FadePanel.instance.Fade(1);

        Invoke("ReturnForReal", 1.5f);
    }

    void ReturnForReal() // Invoked
    {
        SceneManager.LoadScene(0);
        resultPanel.SetActive(false);

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

    public void StartGame() // Individual
    {
        // Bind player to shopping list
        if (!LevelManager.instance.listsBound) BindShoppingList();

        // Reset winners
        winners.Clear();

        // Clear Items
        shoppingList1.shoppingItems.Clear();
        shoppingList2.shoppingItems.Clear();

        Dictionary<string, int> shoppingItems = new Dictionary<string, int>();

        // Assign products to buy based on GameMode
        if (gameMode == GameMode.Round) // Teammode
        {
            // Randomize products in both teams' shopping lists
            int random = UnityEngine.Random.Range(0, 5);

            switch (random)
            {
                case 0:
                    shoppingItems.Add("Water", 1);
                    shoppingItems.Add("Cheese", 1);
                    shoppingItems.Add("Fish", 2);
                    break;
                case 1:
                    shoppingItems.Add("Mussel", 1);
                    shoppingItems.Add("Cupcake", 1);
                    shoppingItems.Add("Waffle", 2);
                    break;
                case 2:
                    shoppingItems.Add("Bread", 2);
                    shoppingItems.Add("Lollipop", 1);
                    shoppingItems.Add("Croissant", 2);
                    break;
                case 3:
                    shoppingItems.Add("Apple", 2);
                    shoppingItems.Add("Eggs", 1);
                    shoppingItems.Add("Ice Cream", 2);
                    break;
                case 4:
                    shoppingItems.Add("Milk", 2);
                    shoppingItems.Add("Yoghurt", 1);
                    shoppingItems.Add("Shrimp", 2);
                    break;
            }
        }

        // Shuffle
        shoppingList1.shoppingItems = shoppingItems.Shuffle();
        shoppingList2.shoppingItems = shoppingItems.Shuffle();

        // Add items to shopping list UI
        foreach (KeyValuePair<string, int> pair in shoppingList1.shoppingItems)
        {
            ShoppingItem item = Instantiate(shoppingList1.itemPrefab, Vector3.zero, Quaternion.identity, shoppingList1.contents).GetComponent<ShoppingItem>();
            item.transform.localPosition = new Vector3(0, shoppingList1.offset, 0);
            item.SetText(pair.Value, pair.Key);
            shoppingList1.items.Add(item);
            shoppingList1.offset -= 55;
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
                shoppingList2.offset -= 55;
            }
        }
    }

    public void UpdateJoinSpots()
    {
        for (int i = 0; i < joinCanvas.Count; i++)
            joinCanvas[i].SetActive(true);

        for (int i = 0; i < players.Count; i++)
            joinCanvas[i].SetActive(false);
    }

    public void JoinAction(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().buildIndex > 0) return;

        if (logo != null && logo.activeInHierarchy) logo.SetActive(false);

        inputManager.JoinPlayerFromActionIfNotAlreadyJoined(context);
    }
}