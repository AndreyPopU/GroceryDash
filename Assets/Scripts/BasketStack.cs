using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasketStack : MonoBehaviour
{
    public Stack<Basket> baskets;

    private void Start()
    {
        baskets = new Stack<Basket>();
        foreach (Basket basket in GetComponentsInChildren<Basket>()) baskets.Push(basket);
    }

    public void ResetStack()
    {
        // Clear List
        baskets.Clear();

        // Return baskets to their parent stack
        Stack<Basket> temp = new Stack<Basket>();


        foreach (Basket basket in FindObjectsOfType<Basket>())
        {
            if (basket.stackParent == null) continue;

            // Position them correctly
            basket.transform.parent = transform;
            basket.coreCollider.enabled = false;
            basket.transform.localPosition = Vector3.zero + Vector3.up * 0.4f * temp.Count;
            basket.transform.localRotation = Quaternion.identity;
            basket.rb.isKinematic = true;

            // Add them to new list
            temp.Push(basket);
        }

        while(temp.Count > 0)
            baskets.Push(temp.Pop());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (baskets.Count <= 0) return;

        if (other.TryGetComponent(out Player player)) player.closestBasket = baskets.Peek();
    }

    private void OnTriggerExit(Collider other)
    {
        if (baskets.Count <= 0) return;

        if (other.TryGetComponent(out Player player) && player.closestBasket == baskets.Peek()) player.closestBasket = null;
    }
}
