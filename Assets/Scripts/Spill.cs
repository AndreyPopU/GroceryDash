using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.TryGetComponent(out Player player)) player.SlowDown(true);

        if (other.TryGetComponent(out Basket basket))
        {
            if (basket.player != null) basket.player.SlowDown(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.TryGetComponent(out Player player)) player.SlowDown(false);

        if (other.TryGetComponent(out Basket basket))
        {
            if (basket.player != null) basket.player.SlowDown(true);
        }
    }
}
