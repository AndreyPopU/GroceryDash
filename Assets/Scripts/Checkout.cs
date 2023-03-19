using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkout : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Product product))
        {
            if (product.player.holdProduct == null)
            {
                product.player.shoppingList.Add(product.productName);
                product.gameObject.SetActive(false);
            }
        }
    }
}
