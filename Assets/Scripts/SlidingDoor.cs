using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public List<Transform> inRange;
    public bool open;

    private Animator animator;

    private IEnumerator Start()
    {
        animator = GetComponentInParent<Animator>();
        yield return new WaitForSeconds(1);
        inRange.Clear();
    }

    void Update()
    {
        // Animate
        if (inRange.Count > 0 && !open) open = true;
        
        if (inRange.Count  <= 0 && open) open = false;

        animator.SetBool("open", open);

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
