using System;
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
        products.Add(product);

        // Keep the product
        product.owner = player;
        product.basket = this;
        if (!product.lastOwner) product.lastOwner = player;
        product.transform.position = Vector3.up * -100;
        mass += .25f;
        rb.mass = mass;

        StartCoroutine(ActivateLevel(products.Count - 1, true));
    }

    public IEnumerator ActivateLevel(int index, bool active)
    {
        YieldInstruction instruction = new WaitForFixedUpdate();

        Transform[] levelItems = transform.GetChild(0).GetChild(index).GetComponentsInChildren<Transform>();

        if (active)
        {
            float scale = 0;
            while(scale < 1)
            {
                scale += .1f;

                foreach (Transform item in levelItems)
                    item.transform.localScale = Vector3.one * scale;

                yield return instruction;
            }
        }
        else
        {
            float scale = 1;
            while (scale > 0)
            {
                scale -= .1f;

                foreach (Transform item in levelItems)
                    item.transform.localScale = Vector3.one * scale;

                yield return instruction;
            }
        }
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
                products.RemoveAt(0);
                checkout.Scan();
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
