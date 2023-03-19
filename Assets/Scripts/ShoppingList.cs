using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShoppingList : MonoBehaviour
{
    public Dictionary<string, int> shoppingItems = new Dictionary<string, int>();
    public List<ShoppingItem> items = new List<ShoppingItem>();
    public Transform contents;
    public GameObject itemPrefab;
    int offset = 100;

    void Start()
    {
        shoppingItems.Add("Apple", 3);
        shoppingItems.Add("Milk", 2);
        shoppingItems.Add("Water", 1);

        foreach (KeyValuePair<string, int> pair in shoppingItems)
        {
            ShoppingItem item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, contents).GetComponent<ShoppingItem>();
            item.transform.localPosition = new Vector3(0, offset, 0);
            item.SetText(pair.Value, pair.Key);
            items.Add(item);
            offset -= 50;
        }
    }

    public void Add(string productName)
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
    }
}
