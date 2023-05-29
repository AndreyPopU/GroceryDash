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
    public Transform contents;
    public GameObject itemPrefab;
    public int offset = 100;
    public bool shared;

    public void Buy(string productName)
    {
        if (shared)
        {

        }
        else
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

            for (int i = 0; i < GameManager.instance.players.Count; i++)
            {
                foreach (KeyValuePair<string, int> pair in GameManager.instance.players[i].shoppingList.shoppingItems)
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

            GameManager.instance.StartRound(false);
        }
    }
}
