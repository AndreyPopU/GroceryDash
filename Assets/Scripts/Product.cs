using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Product : MonoBehaviour
{
    public string productName;
    public Player player;
    [HideInInspector]
    public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            _player.closestProduct = this;
        }

        if (other.GetComponent<Checkout>())
        {
            if (player.shoppingList.shoppingItems[productName] <= 0) return;

            if (player.holdProduct == null)
            {
                player.shoppingList.Add(productName);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            if (_player.closestProduct == this) _player.closestProduct = null;
        }
    }
}
