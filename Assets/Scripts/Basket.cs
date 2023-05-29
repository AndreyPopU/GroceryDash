using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Basket : MonoBehaviour
{
    public List<Product> products;

    public Image[] productIcons;
    public Vector3 holdOffset;
    public Vector3 center;
    public int capacity = 5;
     public BasketStack stackParent;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public float mass;
    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Quaternion startRotation;
    public BoxCollider coreCollider;
    public Player player;
    public Player lastOwner;
    public bool canPickUp = true;

    private Transform canvas;

    void Start()
    {
        if (transform.parent != null) stackParent = transform.parent.GetComponent<BasketStack>();
        rb = GetComponent<Rigidbody>();
        mass = rb.mass;
        canvas = productIcons[0].transform.parent;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Update()
    {
        // Canvas always face camera
        canvas.eulerAngles = new Vector3(canvas.eulerAngles.x, 0, canvas.eulerAngles.z);
    }

    public void AddProduct(Product product)
    {
        products.Add(product);

        // Asign canvas image product icon
        Image icon = productIcons[products.Count - 1];
        icon.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(product.productName + "Icon");
        icon.StartCoroutine(FadeIcon(icon.GetComponent<CanvasGroup>(), 1));

        // Keep the product
        product.owner = player;
        product.basket = this;
        if (!product.lastOwner) product.lastOwner = player;
        product.transform.position = Vector3.up * -100;
        mass += .25f;
        rb.mass = mass;

        StartCoroutine(ActivateLevel(products.Count - 1, true));
    }

    public IEnumerator FadeIcon(CanvasGroup group, int desire)
    {
        if (group.alpha < desire)
        {
            while (group.alpha < desire)
            {
                group.alpha += 2 * Time.deltaTime;
                yield return null;
            }
        }
        else if (group.alpha > desire)
        {
            while (group.alpha > desire)
            {
                group.alpha -= 2 * Time.deltaTime;
                yield return null;
            }
        }

        group.alpha = desire;
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
            if (player != null || products.Count == 0) return;

            // Anchor
            if (checkout.basket == null)
            {
                rb.velocity = Vector3.zero;
                transform.SetParent(checkout.transform);
                if (!checkout.self) transform.position = checkout.scanPosition.position + checkout.transform.right * 1.5f;
                else transform.position = checkout.scanPosition.position + checkout.transform.right * -1.3f;
                transform.rotation = Quaternion.identity;
                canPickUp = false;
                checkout.basket = this;
            }

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

            if (product.rb.velocity.magnitude > 2.5f || product.transform.position.y > transform.position.y)
            {
                product.rb.velocity = Vector3.zero;
                AddProduct(product);
            }
        }
    }
}
