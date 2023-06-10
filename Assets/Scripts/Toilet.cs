using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toilet : MonoBehaviour
{
    public List<Player> playersRange;

    private Animator animator;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.customize = false;
            if (!playersRange.Contains(player)) playersRange.Add(player);
            animator.SetBool("open", true);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.customize = false;
            if (playersRange.Contains(player)) playersRange.Remove(player);

            // If no players in range, close door
            if (playersRange.Count <= 0) animator.SetBool("open", false);
        }
    }
}
