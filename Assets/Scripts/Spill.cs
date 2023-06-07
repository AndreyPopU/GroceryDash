using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.GetComponent<Player>())
        {
            other.GetComponent<Player>().SlowDown(true);
            other.GetComponent<Player>().inMilk = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.GetComponent<Player>())
        {
            other.GetComponent<Player>().SlowDown(false);
            other.GetComponent<Player>().inMilk = false;
        }
    }
}
