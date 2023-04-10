using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public List<Product> products;

    [HideInInspector] public Rigidbody rb;
    public MeshCollider meshCollider;
    public BoxCollider boxCollider;
    public BoxCollider triggerCollider;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddProduct(Product product)
    {
        products.Add(product);
        product.rb.isKinematic = false;
        product.transform.position = transform.position + Vector3.up;
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
