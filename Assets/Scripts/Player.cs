using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    [Header("Player stats")]
    [SerializeField] private float speed = 300;
    [SerializeField] private float basketSpeed = 250;
    [SerializeField] private float slowSpeed = 200;
    [SerializeField] private float rotateSpeed = .4f;
    [SerializeField] private float slowRotateSpeed = .3f;
    [SerializeField] private float throwForce = 5;
    public bool canMove = true;

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

    [Header("Bumped")]
    public bool bumped;
    public float bumpForce;
    public float bumpDuration;

    private Vector2 movement;
    private Rigidbody rb;
    private Transform gfx;
    private float baseSpeed;
    private float baseRotateSpeed;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody>();
        gfx = transform.GetChild(0);
        baseSpeed = speed;
        baseRotateSpeed = rotateSpeed;
        baseDashCD = dashCD;
    }

    void FixedUpdate()
    {
        if (dashCD > 0) dashCD -= Time.deltaTime;

        if (dashing || bumped || !canMove) return;

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
            holdProduct.owner = null;
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
            if (closestProduct == null) // Drop basket
            {
                holdBasket.transform.SetParent(null);
                holdBasket.boxCollider.enabled = true;
                holdBasket.meshCollider.enabled = false;
                holdBasket.rb.isKinematic = false;
                closestBasket = holdBasket;
                holdBasket = null;
                speed = baseSpeed;
                rotateSpeed = baseRotateSpeed;
                return;
            }
        }
        else if (closestBasket != null && holdProduct == null) // Pick up basket if not holding product already
        {
            if (closestBasket.player != null) return;

            closestBasket.transform.SetParent(holdParent);
            closestBasket.transform.localPosition = new Vector3(0, -1.4f, 1.25f);
            closestBasket.transform.localEulerAngles = Vector3.up * -90;
            closestBasket.rb.isKinematic = true;
            closestBasket.boxCollider.enabled = false;
            closestBasket.meshCollider.enabled = true;
            closestBasket.player = this;
            holdBasket = closestBasket;
            closestBasket = null;
            speed = basketSpeed;
            rotateSpeed = slowRotateSpeed;
            return;
        }

        if (holdProduct != null) // Drop product
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.owner = null;
            holdProduct = null;
            return;
        }
        else if (closestProduct != null) // Pick up product
        {
            if (closestProduct.owner != null) return;

            if (holdBasket != null) // If player carries a basket drop products there
            {
                if (!closestProduct.gameObject.activeInHierarchy) // If product is from shelf - instantiate 
                {
                    Product product = Instantiate(closestProduct.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Product>();
                    holdBasket.AddProduct(product);
                }
                else holdBasket.AddProduct(closestProduct);

                return;
            }

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
            holdProduct.owner = this;
            holdProduct.lastOwner = this;
        }
    }

    public void Bump(Vector3 direction)
    {
        bumped = true;
        if (holdProduct != null)
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.owner = null;
            holdProduct = null;
        }
        StartCoroutine(BumpCO(direction));
    }

    private IEnumerator BumpCO(Vector3 direction)
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        float startTime = Time.time;
        rb.velocity = Vector3.zero;

        while (Time.time < startTime + bumpDuration)
        {
            rb.velocity += direction * bumpForce;
            yield return waitForFixedUpdate;
        }
        bumped = false;
    }

    public void SlowDown(bool slow)
    {
        if (slow)
        {
            speed = slowSpeed;
            rotateSpeed = slowRotateSpeed;
        }
        else
        {
            speed = holdBasket != null ? basketSpeed : baseSpeed;
            rotateSpeed = holdBasket != null ? slowRotateSpeed : baseRotateSpeed;
        }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent(out Player player))
        {
            if (dashing)
            {
                player.Bump(new Vector3(movement.x, 0, movement.y));
            }
        }
        
    }
}
