using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool roundStarted = true;

    public ShoppingList shoppingList1;
    public ShoppingList shoppingList2;

    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    public void BindShoppingList(PlayerInput input)
    {
        if (input.playerIndex == 0)
        {
            input.GetComponent<Player>().shoppingList = shoppingList1;
            shoppingList1.gameObject.SetActive(true);
        }
        else
        {
            input.GetComponent<Player>().shoppingList = shoppingList2;
            shoppingList2.gameObject.SetActive(true);
        }

        CameraManager.instance.targets.Add(input.transform);
        input.transform.position = transform.GetChild(input.playerIndex).position;
        MeshRenderer renderer = input.GetComponentInChildren<MeshRenderer>();
        renderer.material.color = input.playerIndex == 0 ? Color.green : Color.yellow;

    }

    public void StartRound(bool start)
    {
        roundStarted = start;
        Player[] players = FindObjectsOfType<Player>();

        for (int i = 0; i < players.Length; i++) players[i].canMove = start;

        if (start)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].index == 0)
                {
                    players[i].shoppingList.shoppingItems.Add("Apple", 2);
                    players[i].shoppingList.shoppingItems.Add("Milk", 1);
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
            Timer timer = GetComponent<Timer>();
            timer.countdownText.text = "Time's Up!";
            timer.countdownText.gameObject.SetActive(true);
            timer.roundText.gameObject.SetActive(false);
        }
    }
}
