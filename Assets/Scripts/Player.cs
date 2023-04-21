using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    public int index;
    [Header("Player stats")]
    [SerializeField] private float speed = 1000;
    [SerializeField] private float counterMovement = 250;
    [SerializeField] private float maxSpeed = 7;
    [SerializeField] private float rotateSpeed = .4f;
    [SerializeField] private float slowRotateSpeed = .2f;
    [SerializeField] private float throwForce = 10;

    public bool canMove = true;
    public bool holding;

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
    private float threshold = .01f;
    public bool dashing;

    [Header("Bumped")]
    public float bumpForce;
    private float bumpDuration;

    private Vector2 movement;
    private Vector3 dashDirection;
    [HideInInspector] public Rigidbody rb;
    private Transform gfx;
    private float baseSpeed;
    private float baseRotateSpeed;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.PlayerMovement.Hold.started += ctx => holding = !holding;
        controls.PlayerMovement.Hold.canceled += ctx => holding = false;
        controls.PlayerMovement.Dash.started += ctx => Dash(new Vector3(movement.x, 0, movement.y).normalized);
        controls.PlayerMovement.Throw.started += ctx => ThrowProduct();
        controls.PlayerMovement.Grab.started += ctx => Grab();
    }

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

        if (dashing || bumpDuration > 0 || !canMove) return;

        Movement();
        Hold();
        gfx.forward = Vector3.Lerp(gfx.forward, new Vector3(movement.x, 0, movement.y), rotateSpeed);
    }

    private void Update()
    {
        if (bumpDuration > 0) bumpDuration -= Time.deltaTime;
    }

    private void Movement()
    {
        // Cap speed
        if (rb.velocity.magnitude > maxSpeed) rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        //Counteract sliding and sloppy movement
        CounterMovement();

        //Apply forces to move player
        rb.AddForce(new Vector3(movement.x, 0, movement.y) * speed * Time.deltaTime);
    }

    private void CounterMovement()
    {
        // Counter movement
        if (Mathf.Abs(movement.x) <= .01f) rb.AddForce(new Vector3(-rb.velocity.x, 0, 0) * counterMovement * Time.deltaTime);
        if (Mathf.Abs(movement.y) <= .01f) rb.AddForce(new Vector3(0, 0, -rb.velocity.z) * counterMovement * Time.deltaTime);
    }

    public void OnMove(CallbackContext context) => movement = context.ReadValue<Vector2>();

    private void Dash(Vector3 direction)
    {
        // If input is neutral return
        if (direction.x == 0 && direction.z == 0 || !GameManager.instance.roundStarted || dashing || dashCD > 0 || !canDash) return;
        
        dashEffect.Play();
        dashing = true;
        dashCD = baseDashCD;
        StartCoroutine(DashCO(direction));
    }

    private IEnumerator DashCO(Vector3 direction)
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();
        yield return null;

        dashDirection = direction;

        float startTime = Time.time;
        rb.velocity = Vector3.zero;

        while (Time.time < startTime + dashDuration)
        {
            rb.velocity += direction * dashRange;
            yield return waitForFixedUpdate;
        }
        dashing = false;
    }

    private void ThrowProduct()
    {
        if (holdProduct != null && GameManager.instance.roundStarted)
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
        if (!holding) return;

        if (closestPlayer != null && GameManager.instance.roundStarted)
        {
            // Combine movement vectors

            print("holding onto");
        }
    }

    private void Grab()
    {
        if (!GameManager.instance.roundStarted) return;

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
            SlowDown(true);
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
            if (closestProduct.owner != null) return; // If closest product is held by another player return

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

            if (!closestProduct.canPickUp) return;

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

    public void Bump(Vector3 direction, float force)
    {
        if (bumpDuration > 0) return;

        rb.velocity = Vector3.zero;
        rb.AddForce(direction * force * Time.deltaTime, ForceMode.Impulse);

        if (holdProduct != null) // Drop product
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.owner = null;
            holdProduct = null;
        }

        bumpDuration = .2f;
    }

    public void SlowDown(bool slow)
    {
        if (slow)
        {
            maxSpeed = holdBasket != null ? 5.5f : 2.5f;
            rotateSpeed = slowRotateSpeed;
        }
        else
        {
            maxSpeed = holdBasket != null ? 4.5f : 7;
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

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.TryGetComponent(out Player player))
            if (dashing)
            {
                player.Bump(dashDirection, bumpForce);
                StopCoroutine("DashCO");
                rb.velocity = Vector3.zero;
            }
    }

    public void OnEnable()
    {
        controls.PlayerMovement.Enable();
    }

    public void OnDisable()
    {
        controls.PlayerMovement.Disable();
    }
}
