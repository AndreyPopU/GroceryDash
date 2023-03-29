using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkout : MonoBehaviour
{
    public bool open = true;
    public Material openMat, closedMat;

    private MeshRenderer lights;

    private void Start()
    {
        lights = GetComponentInChildren<MeshRenderer>();
        lights.material = openMat;

        InvokeRepeating("ChangeColor", 3, 3);
    }

    private void ChangeColor()
    {
        open = !open;

        if (open) lights.material = openMat;
        else lights.material = closedMat;
    }

}
