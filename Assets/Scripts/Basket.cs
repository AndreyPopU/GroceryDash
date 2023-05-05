using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Basket : MonoBehaviour
{
    public List<Product> products;

    public Vector3 holdOffset;
    public Vector3 center;
    public int capacity = 5;
    [HideInInspector] public BasketStack stackParent;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector]public float mass;
    public BoxCollider coreCollider;
    public Player player;
    public Player lastOwner;

    void Start()
    {
        if (transform.parent != null) stackParent = transform.parent.GetComponent<BasketStack>();
        rb = GetComponent<Rigidbody>();
        mass = rb.mass;
    }

    public void AddProduct(Product product)
    {
        // Place in cart and anchor
        float randomX = Random.Range(-.85f, .85f);
        float randomZ = Random.Range(-.5f, .5f);
        products.Add(product);
        product.owner = player;
        if (!product.lastOwner) product.lastOwner = player;
        product.transform.position = transform.position + new Vector3(randomX, 1, randomZ);
        product.transform.SetParent(transform);
        product.rb.isKinematic = false;
        product.anchor = true;
        mass += .25f;
        rb.mass = mass;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Product product))
            if (!products.Contains(product)) products.Add(product);

        if (other.TryGetComponent(out Player _player)) _player.closestBasket = this;
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
            if (_player.closestBasket == this) _player.closestBasket = null;

        if (other.TryGetComponent(out Product product))
            if (products.Contains(product)) products.Remove(product);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.TryGetComponent(out Product product))
        {
            if (products.Contains(product)) return;

            if (product.rb.velocity.magnitude > 2.5f)
            {
                product.rb.velocity = Vector3.zero;
                AddProduct(product);
            }
        }
    }
}
