using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player)) player.SlowDown(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player)) player.SlowDown(false); 
    }
}
