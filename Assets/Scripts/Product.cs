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
    public Basket basket;

    void Awake() => rb = GetComponent<Rigidbody>();    

    public IEnumerator Enlarge()
    {
        YieldInstruction instruction = new WaitForFixedUpdate();

        while(transform.localScale.x < 1)
        {
            transform.localScale += Vector3.one * .1f;
            yield return instruction;
        }
    }

    public void Destroy() => StartCoroutine(DestroyCO());

    private IEnumerator DestroyCO()
    {
        while (transform.localScale.x > 0)
        {
            transform.localScale -= Vector3.one * .1f;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            // If player holds a basket and that basket contains the product, make it impossible to interact with
            if (_player.holdBasket && _player.holdBasket.products.Contains(this)) return;

            if (!_player.productsInRange.Contains(this)) _player.productsInRange.Add(this);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.holdBasket && pick.player.holdBasket.products.Contains(this)) return;

            if (!pick.player.productsInRange.Contains(this)) pick.player.productsInRange.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            if (_player.productsInRange.Contains(this)) _player.productsInRange.Remove(this);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.productsInRange.Contains(this)) pick.player.productsInRange.Remove(this);
        }
    }
}
