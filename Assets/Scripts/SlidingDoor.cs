using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class SlidingDoor : MonoBehaviour
{
    public List<Transform> inRange;
    public bool open;

    private Animator animator;
    private AudioSource source;

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Animate
        if (inRange.Count > 0 && !open)
        {
            open = true;
            source.Play();
        }

        if (inRange.Count <= 0 && open)
        {
            open = false;
            source.Play();
        }

        animator.SetBool("open", open);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!inRange.Contains(other.transform) && (other.GetComponent<Player>() || other.GetComponent<NavMeshAgent>()))
        {
            if (Tutorial.instance.tutorialCompleted == 0) Tutorial.instance.StartTutorial();
            inRange.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (inRange.Contains(other.transform)) inRange.Remove(other.transform);
    }
}
