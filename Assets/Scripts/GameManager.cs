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
    }

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
            
        }
        else
        {

        }
    }
}
