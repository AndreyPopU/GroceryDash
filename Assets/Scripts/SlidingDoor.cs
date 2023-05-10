using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public List<Transform> inRange;
    public bool open;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        inRange.Clear();
    }

    void Update()
    {
        if (inRange.Count > 0 && !open)
        {
            // Play Animation
            open = true;
        }
        
        if (inRange.Count  <= 0 && open)
        {
            // Play Animation
            open = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!inRange.Contains(other.transform)) inRange.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (inRange.Contains(other.transform)) inRange.Remove(other.transform);
    }
}
