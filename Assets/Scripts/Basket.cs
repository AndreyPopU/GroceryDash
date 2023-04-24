using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Basket : MonoBehaviour
{
    public List<Product> products;

    public int capacity = 5;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector]public float mass;
    public BoxCollider containCollider;
    public BoxCollider coreCollider;
    public BoxCollider triggerCollider;
    public Player player;
    public Player lastOwner;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mass = rb.mass;
    }

    public void AddProduct(Product product)
    {
        // Check Capacity
        if (products.Count >= capacity)
        {
            // User feedback that basket is full

            return;
        }

        // Place in cart and anchor
        float randomX = Random.Range(-.85f, .85f);
        float randomZ = Random.Range(-.5f, .5f);
        products.Add(product);
        product.owner = player;
        product.lastOwner = player;
        product.transform.position = transform.position + new Vector3(randomX, 1, randomZ);
        product.transform.SetParent(transform);
        product.rb.isKinematic = false;
        product.anchor = true;
        mass += .25f;
    }

    public void AddRigidbody()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Product product))
        {
            if (!products.Contains(product)) products.Add(product);
        }

        if (other.TryGetComponent(out Player _player))
        {
            _player.closestBasket = this;
        }

        if (other.GetComponent<Checkout>())
        {
            // Start buying

            //if (player.shoppingList.shoppingItems[productName] <= 0) return;

            //if (player.holdProduct == null)
            //{
            //    player.shoppingList.Add(productName);
            //    Destroy(gameObject);
            //}
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.TryGetComponent(out Player player))
            if (player.dashing)
            {
                transform.SetParent(null);
                coreCollider.enabled = true;
                containCollider.enabled = false;
                AddRigidbody();
                rb.isKinematic = false;
                rb.AddForce(player.gfx.forward * player.throwForce * 1.5f, ForceMode.Impulse);
                this.player.SlowDown(false);
                this.player.holdBasket = null;
                lastOwner = player;
                this.player = null;
            }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Checkout checkout))
        {
            if (products.Count == 0) return;

            // Start scanning items
            if (!checkout.scanning && checkout.scanningProduct == null)
            {
                checkout.scanningProduct = products[0];
                checkout.Scan();
                products.RemoveAt(0);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            if (_player.closestBasket == this) _player.closestBasket = null;
        }

        if (other.TryGetComponent(out Product product))
        {
            if (products.Contains(product)) products.Remove(product);
        }
    }
}
