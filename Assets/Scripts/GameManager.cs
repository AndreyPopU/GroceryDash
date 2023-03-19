using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }
}
