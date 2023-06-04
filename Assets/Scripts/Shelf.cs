using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shelf : MonoBehaviour
{
    public Product product;
    public Image productImage;

    // Find icon based on product
    private void Start() => productImage.sprite = Resources.Load<Sprite>(product.productName + "Icon");

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (!player.productsInRange.Contains(product)) player.productsInRange.Add(product);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (!pick.player.productsInRange.Contains(product)) pick.player.productsInRange.Add(product);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (player.productsInRange.Contains(product)) player.productsInRange.Remove(product);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.productsInRange.Contains(product)) pick.player.productsInRange.Remove(product);
        }
    }
}
