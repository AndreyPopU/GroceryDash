using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ShoppingList : MonoBehaviour
{
    public Dictionary<string, int> shoppingItems = new Dictionary<string, int>();
    public List<ShoppingItem> items = new List<ShoppingItem>();
    public Transform contents;
    public GameObject itemPrefab;
    public int offset = 100;

    public void Buy(string productName)
    {
        shoppingItems[productName]--;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].text.text.Contains(productName))
            {
                items[i].SetText(shoppingItems[productName], productName);
                break;
            }
        }

        Player[] players = FindObjectsOfType<Player>();
        for (int i = 0; i < players.Length; i++)
        {
            foreach (KeyValuePair<string, int> pair in players[i].shoppingList.shoppingItems)
            {
                if (pair.Value > 0) return;
            }
        }

        GameManager.instance.StartRound(false);
    }
}
