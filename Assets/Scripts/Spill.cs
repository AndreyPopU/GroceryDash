using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.GetComponent<Player>()) other.GetComponent<Player>().SlowDown(true);

        if (other.transform.parent != null && other.transform.parent.GetComponent<Player>()) other.transform.parent.GetComponent<Player>().SlowDown(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.GetComponent<Player>()) other.GetComponent<Player>().SlowDown(false);

        if (other.transform.parent != null && other.transform.parent.GetComponent<Player>()) other.transform.parent.GetComponent<Player>().SlowDown(false);
    }
}
