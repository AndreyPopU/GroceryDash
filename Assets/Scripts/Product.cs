using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Product : MonoBehaviour
{
    public bool canPickUp = true;
    public string productName;
    public Player owner;
    public Player lastOwner;
    [HideInInspector]
    public Rigidbody rb;
    public Basket basket;
    public float pickUpCD;
    public bool thrown;

    private bool called;

    void Awake() => rb = GetComponent<Rigidbody>();

    private void Update()
    {
        if (pickUpCD > 0)
        {
            called = false;
            pickUpCD -= Time.deltaTime;
        }
        else
        {
            if (!called)
            {
                canPickUp = true;
                called = true;
            }
        }
    }

    public IEnumerator Enlarge()
    {
        YieldInstruction instruction = new WaitForFixedUpdate();

        while (transform.localScale.x < 1)
        {
            transform.localScale += Vector3.one * .1f;
            yield return instruction;
        }
    }

    public void Destroy() => StartCoroutine(DestroyCO());

    private IEnumerator DestroyCO()
    {
        while (transform.localScale.x > 0)
        {
            transform.localScale -= Vector3.one * .1f;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            // If player holds a basket and that basket contains the product, make it impossible to interact with
            if (_player.holdBasket && _player.holdBasket.products.Contains(this)) return;

            if (!_player.productsInRange.Contains(this)) _player.productsInRange.Add(this);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.holdBasket && pick.player.holdBasket.products.Contains(this)) return;

            if (!pick.player.productsInRange.Contains(this)) pick.player.productsInRange.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player _player))
        {
            if (_player.productsInRange.Contains(this)) _player.productsInRange.Remove(this);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.productsInRange.Contains(this)) pick.player.productsInRange.Remove(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        DisableTrail();

        if (thrown)
        {
            if (collision.collider.TryGetComponent(out Player player))
            {
                if (player.holdBasket != null) return;

                if (GameManager.instance.roundStarted || SceneManager.GetActiveScene().buildIndex == 0)
                {
                    rb.velocity = Vector3.zero;
                    // Swap products
                    if (player.holdProduct != null) player.PickUpProduct(false);
                    player.PickUpProduct(true);
                }
            }
            thrown = false;
        }
    }

    public void DisableTrail()
    {
        // If trail active - disable
        if (GetComponentInChildren<ParticleSystem>())
        {
            GetComponentInChildren<ParticleSystem>().Stop();
            Destroy(GetComponentInChildren<ParticleSystem>().gameObject, 2);
        }

        
    }

    private void OnCollisionStay(Collision collision)
    {
        
    }
}
