using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

public class ShoppingList : MonoBehaviour
{
    public Dictionary<string, int> shoppingItems = new Dictionary<string, int>();
    public List<ShoppingItem> items = new List<ShoppingItem>();
    public List<Player> owners;
    public string team;
    public Transform contents;
    public GameObject itemPrefab;
    public int offset = 100;
    public bool shared;

    public void Buy(string productName)
    {
        // Reduce shopping item amount
        shoppingItems[productName]--;

        // Check for every item in the shopping list
        for (int i = 0; i < items.Count; i++)
        {
            // When item name matches with product bought
            if (items[i].text.text.Contains(productName))
            {
                // Update item's text on the shopping list
                items[i].SetText(shoppingItems[productName], productName);
                break;
            }
        }

        for (int i = 0; i < GameManager.instance.players.Count; i++)
        {
            // If shopping list still contains items with an amount more than 0 (shopping list is not completed) return;
            foreach (KeyValuePair<string, int> pair in shoppingItems)
            {
                if (pair.Value > 0) return;
            }
        }

#if ENABLE_CLOUD_SERVICES_ANALYTICS
        Analytics.CustomEvent("ListCompleted", new Dictionary<string, object>
                {
                    { "listCompleted", true },
                });

        AnalyticsService.Instance.CustomData("ListCompleted", new Dictionary<string, object>
                {
                    { "listCompleted", true },
                });
#endif

        // Award players with a point 
        foreach (Player owner in owners)
        {
            print("Awarding one point to player " + owner.gameObject.name);
            owner.score++;
        }

        // Assign Winners
        GameManager.instance.winners.AddRange(owners);

        // End Round
        GameManager.instance.StartRound(false);
    }
}
