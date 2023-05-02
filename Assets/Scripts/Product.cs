using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool anchor;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();    
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (anchor)
        {
            rb.isKinematic = true;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            // If player holds a basket and that basket contains the product, make it impossible to interact with
            if (_player.holdBasket && _player.holdBasket.products.Contains(this)) return;

            _player.closestProduct = this;
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.holdBasket && pick.player.holdBasket.products.Contains(this)) return;
            pick.player.closestProduct = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            if (_player.closestProduct == this) _player.closestProduct = null;
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.closestProduct == this) pick.player.closestProduct = null;
        }

    }
}
