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

    public int playerCount;
    public List<Player> players;
    public bool roundStarted = true;
    public bool gameStarted = false;

    public GameObject disconnectedTextPrefab;

    public ShoppingList shoppingList1;
    public ShoppingList shoppingList2;

    public GameObject[] canvasJoin;
    
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
        {
            if (!LevelManager.instance.listsBound) BindShoppingList();

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].index == 0)
                {
                    players[i].shoppingList.shoppingItems.Add("Milk", 1);
                    players[i].shoppingList.shoppingItems.Add("Apple", 2);
                    players[i].shoppingList.shoppingItems.Add("Bread", 1);
                    players[i].shoppingList.shoppingItems.Add("Water", 2);
                    players[i].shoppingList.shoppingItems.Add("Croissant", 1);

                    foreach (KeyValuePair<string, int> pair in players[i].shoppingList.shoppingItems)
                    {
                        ShoppingItem item = Instantiate(players[i].shoppingList.itemPrefab, Vector3.zero, Quaternion.identity, players[i].shoppingList.contents).GetComponent<ShoppingItem>();
                        item.transform.localPosition = new Vector3(0, players[i].shoppingList.offset, 0);
                        item.SetText(pair.Value, pair.Key);
                        players[i].shoppingList.items.Add(item);
                        players[i].shoppingList.offset -= 50;
                    }
                }
                else
                {
                    players[i].shoppingList.shoppingItems.Add("Apple", 1);
                    players[i].shoppingList.shoppingItems.Add("Milk", 2);
                    players[i].shoppingList.shoppingItems.Add("Cheese", 1);
                    players[i].shoppingList.shoppingItems.Add("Crab", 1);
                    players[i].shoppingList.shoppingItems.Add("Fish", 2);

                    foreach (KeyValuePair<string, int> pair in players[i].shoppingList.shoppingItems)
                    {
                        ShoppingItem item = Instantiate(players[i].shoppingList.itemPrefab, Vector3.zero, Quaternion.identity, players[i].shoppingList.contents).GetComponent<ShoppingItem>();
                        item.transform.localPosition = new Vector3(0, players[i].shoppingList.offset, 0);
                        item.SetText(pair.Value, pair.Key);
                        players[i].shoppingList.items.Add(item);
                        players[i].shoppingList.offset -= 50;
                    }
                }
            }
        }
        else
        {
            StartCoroutine(Time());
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

    private IEnumerator Time() // Invoked
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
}
