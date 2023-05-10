using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    // Tag class

    [HideInInspector] public Player player;

    void Start() => player = GetComponentInParent<Player>();    

}
