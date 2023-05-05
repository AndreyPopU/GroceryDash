using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketStack : MonoBehaviour
{
    public List<Basket> baskets;

    private void OnTriggerEnter(Collider other)
    {
        if (baskets.Count <= 0) return;

        if (other.TryGetComponent(out Player player)) player.closestBasket = baskets[0];
    }

    private void OnTriggerExit(Collider other)
    {
        if (baskets.Count <= 0) return;

        if (other.TryGetComponent(out Player player) && player.closestBasket == baskets[0]) player.closestBasket = null;
    }
}
