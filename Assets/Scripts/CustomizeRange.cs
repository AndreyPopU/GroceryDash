using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeRange : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>()) other.GetComponent<Player>().customize = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>()) other.GetComponent<Player>().customize = false;
    }
}
