using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    public Product product;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.closestProduct = product;
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            pick.player.closestProduct = product;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (player.closestProduct == product) player.closestProduct = null;
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.closestProduct == product) pick.player.closestProduct = null;
        }
    }
}
