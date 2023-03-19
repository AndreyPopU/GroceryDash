using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public ShoppingList shoppingList1;
    public ShoppingList shoppingList2;

    private PlayerInputManager playerInputManager;

    void Update()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();    
    }

    public void BindShoppingList(PlayerInput input)
    {
        // Cycle through players and assign shopping lists
        Player[] players = FindObjectsOfType<Player>();

        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].shoppingList)
            {
                if (playerInputManager.playerCount == 1)
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
    }
}
