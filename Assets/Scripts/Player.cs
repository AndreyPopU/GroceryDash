using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    [Header("Player stats")]
    [SerializeField] private float speed = 300;
    [SerializeField] private float slowSpeed = 200;
    [SerializeField] private float rotateSpeed = .4f;
    [SerializeField] private float throwForce = 5;

    [Header("Product Management")]
    public ShoppingList shoppingList;
    public Product closestProduct;
    public Product holdProduct;
    public Transform holdParent;

    [Header("Hold onto items")]
    public Player closestPlayer;

    [Header("Basket")]
    public Basket holdBasket;
    public Basket closestBasket;

    [Header("Dash")]
    [SerializeField] private bool canDash = true;
    public float dashRange;
    public float dashDuration = .25f;
    public float dashCD = 1;
    public ParticleSystem dashEffect;
    private float baseDashCD;
    public bool dashing;

    private Vector2 movement;
    private Rigidbody rb;
    private Transform gfx;
    private float baseSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gfx = transform.GetChild(0);
        baseSpeed = speed;
        baseDashCD = dashCD;
    }

    void FixedUpdate()
    {
        if (dashCD > 0) dashCD -= Time.deltaTime;

        if (dashing) return;

        rb.velocity = new Vector3(movement.x, 0, movement.y) * speed * Time.fixedDeltaTime;
        gfx.forward = Vector3.Lerp(gfx.forward, new Vector3(movement.x, 0, movement.y), rotateSpeed);
    }

    #region Input

    public void OnMove(CallbackContext context) => movement = context.ReadValue<Vector2>();

    public void OnDash(CallbackContext context) => context.action.started += _ => Dash();

    public void OnThrow(CallbackContext context) => context.action.started += _ => ThrowProduct();

    public void OnGrab(CallbackContext context) => context.action.started += _ => Grab();
    public void OnHold(CallbackContext context) => context.action.performed += _ => Hold();

    #endregion

    private void Dash()
    {
        // If input is neutral return
        if (movement.x == 0 && movement.y == 0 || dashing || dashCD > 0 || !canDash) return;
        dashing = true;
        dashCD = baseDashCD;
        dashEffect.Play();
        StartCoroutine(DashCO());
    }

    private IEnumerator DashCO()
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        float startTime = Time.time;
        rb.velocity = Vector3.zero;

        while (Time.time < startTime + dashDuration)
        {
            rb.velocity += new Vector3(movement.x, 0, movement.y) * dashRange;
            yield return waitForFixedUpdate;
        }
        dashing = false;
    }

    private void ThrowProduct()
    {
        if (holdProduct != null)
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.rb.AddForce(gfx.forward * throwForce, ForceMode.Impulse);
            holdProduct = null;
        }
    }

    private void Hold()
    {
        if (closestPlayer != null)
        {
            // Combine movement vectors

            print("holding onto");
        }
    }

    private void Grab()
    {
        if (holdBasket)
        {
            // Sell basket products

            // Drop basket
        }
        else if (holdProduct != null) // Pick up basket
        {

        }

        if (holdProduct != null) // Drop product
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct = null;
        }
        else if (closestProduct != null) // Pick up product
        {
            if (!closestProduct.gameObject.activeInHierarchy) // If product is from shelf - instantiate 
            {
                Product product = Instantiate(closestProduct.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Product>();
                holdProduct = product;
            }
            else holdProduct = closestProduct.GetComponent<Product>();

            holdProduct.transform.SetParent(holdParent);
            holdProduct.transform.localPosition = Vector3.zero;
            holdProduct.transform.localEulerAngles = Vector3.zero;
            holdProduct.rb.isKinematic = true;
            holdProduct.player = this;
        }
    }

    public void SlowDown(bool slow)
    {
        if (slow) speed = slowSpeed;
        else speed = baseSpeed;

        canDash = !slow;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>()) closestPlayer = other.GetComponent<Player>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && closestPlayer == other.GetComponent<Player>()) closestPlayer = null;
    }
}
