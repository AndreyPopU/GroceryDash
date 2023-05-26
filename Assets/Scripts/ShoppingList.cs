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

            GameManager.instance.listTime = (int)FindObjectOfType<Timer>().currentTime;

            #if ENABLE_CLOUD_SERVICES_ANALYTICS
                Analytics.CustomEvent("ListTime", new Dictionary<string, object>
                {
                    { "listTime", GameManager.instance.listTime },
                });

            AnalyticsService.Instance.CustomData("ListTime", new Dictionary<string, object>
                {
                    { "listTime", GameManager.instance.listTime },
                });
#endif

            GameManager.instance.StartRound(false);
        }
    }
}
