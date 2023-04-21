using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Product : MonoBehaviour
{
    public bool canPickUp = true;
    public string productName;
    public Player owner;
    public Player lastOwner;
    [HideInInspector]
    public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _player)) _player.closestProduct = this;

        // Old shopping code

        //if (other.GetComponent<Checkout>())
        //{
        //    if (lastOwner.shoppingList.shoppingItems.ContainsKey(productName) && lastOwner.shoppingList.shoppingItems[productName] <= 0) return;

        //    if (lastOwner.holdProduct == null) // ???
        //    {
        //        lastOwner.shoppingList.Add(productName);
        //        Destroy(gameObject);
        //    }
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            if (_player.closestProduct == this) _player.closestProduct = null;
        }
    }
}
