using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{

    public int index;
    public string nickname;
    public Color color;
    [Header("Player stats")]
    [SerializeField] private float speed = 1000;
    [SerializeField] private float counterMovement = 250;
    [SerializeField] private float maxSpeed = 7;
    [SerializeField] private float rotateSpeed = .4f;
    [SerializeField] private float slowRotateSpeed = .2f;
    public float throwForce = 10;

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
    public BoxCollider basketCollider;

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
    [HideInInspector] public Transform gfx;
    private float baseSpeed;
    private float baseRotateSpeed;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        gfx = transform.GetChild(0);
        gfx.GetComponent<MeshRenderer>().material.color = color;
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody>();
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

    #region Input

    public void OnMove(CallbackContext context) => movement = context.ReadValue<Vector2>();

    public void OnDash(CallbackContext context) => context.action.performed += _ => Dash(new Vector3(movement.x, 0, movement.y).normalized);

    public void OnThrow(CallbackContext context) => context.action.performed += _ => ThrowItem();

    public void OnGrab(CallbackContext context) => context.action.performed += _ => Grab();

    public void OnHold(CallbackContext context) => context.action.performed += _ => holding = !holding;

    #endregion

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

    private void ThrowItem()
    {
        if (!GameManager.instance.roundStarted) return;

        if (holdProduct != null)
        {
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.rb.AddForce(gfx.forward * throwForce, ForceMode.Impulse);
            holdProduct.owner = null;
            holdProduct = null;
        }
        else if (holdBasket != null)
        {
            holdBasket.transform.SetParent(null);
            holdBasket.coreCollider.enabled = true;
            holdBasket.rb.isKinematic = false;
            holdBasket.rb.AddForce(gfx.forward * throwForce * 1.5f, ForceMode.Impulse);
            holdBasket.lastOwner = holdBasket.player;
            holdBasket.player = null;
            basketCollider.enabled = false;
            holdBasket = null;
            SlowDown(false);
            return;
        }
    }

    private void Hold()
    {
        if (!holding) return;

        if (closestPlayer != null && GameManager.instance.roundStarted)
        {
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
                holdBasket.coreCollider.enabled = true;
                holdBasket.rb.isKinematic = false;
                closestBasket = holdBasket;
                basketCollider.enabled = false;
                holdBasket.player = null;
                holdBasket = null;
                SlowDown(false);
                return;
            }
        }
        else if (closestBasket != null && holdProduct == null) // Pick up basket if not holding product already
        {
            if (closestBasket.player != null) return;

            // Anchor basket in player's hands
            closestBasket.transform.SetParent(holdParent);
            closestBasket.transform.localPosition = new Vector3(0, -1.4f, 1.25f);
            closestBasket.transform.localEulerAngles = Vector3.up * -90;
            closestBasket.rb.isKinematic = true;
            closestBasket.coreCollider.enabled = false;
            basketCollider.enabled = true;
            closestBasket.player = this;
            holdBasket = closestBasket;

            // Give ownership of every product in the basket to the player who holds it
            foreach (Product product in holdBasket.products)
            {
                product.owner = this;
                product.lastOwner = this;
            }

            closestBasket = null;

            // Slow Down player
            SlowDown(true);
            maxSpeed = 5.5f;
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
                // Check capacity
                if (holdBasket.products.Count >= holdBasket.capacity)
                {
                    // User feedback that basket is full

                    return;
                }

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
        else if (holdBasket != null)
        {
            holdBasket.transform.SetParent(null);
            holdBasket.rb.isKinematic = false;
            holdBasket.player = null;
            basketCollider.enabled = false;
            holdBasket = null;
        }

        bumpDuration = .2f;
    }

    #region Events

    public void DisconnectPlayer(PlayerInput input)
    {
        GameObject disconnected = Instantiate(GameManager.instance.disconnectedTextPrefab, GameManager.instance.GetComponentInChildren<Canvas>().transform);
        // Set Color and Name to match the player
        disconnected.GetComponent<TextMeshProUGUI>().color = color;
        disconnected.GetComponent<TextMeshProUGUI>().text = nickname + index + " Disconnected!";
        disconnected.transform.position -= Vector3.up * (70 * index);
        GameManager.instance.StartCoroutine(GameManager.instance.ScaleText(disconnected.transform, 1));
        Destroy(disconnected, 3);
    }

    public void ReconnectPlayer(PlayerInput input)
    {
        GameObject reconnected = Instantiate(GameManager.instance.disconnectedTextPrefab, GameManager.instance.GetComponentInChildren<Canvas>().transform);
        // Set Color and Name to match the player
        reconnected.GetComponent<TextMeshProUGUI>().color = color;
        reconnected.GetComponent<TextMeshProUGUI>().text = nickname + index + " Reconnected!";
        reconnected.transform.position -= Vector3.up * (70 * index);
        GameManager.instance.StartCoroutine(GameManager.instance.ScaleText(reconnected.transform, 1));
        Destroy(reconnected, 3);
    }

    #endregion

    public void SlowDown(bool slow)
    {
        if (slow)
        {
            maxSpeed = 2.5f;
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
        {
            if (dashing)
            {
                player.Bump(dashDirection, bumpForce);
                StopCoroutine("DashCO");
                rb.velocity = Vector3.zero;
            }

            if (player.dashing && holdBasket != null)
            {
                holdBasket.transform.SetParent(null);
                holdBasket.coreCollider.enabled = true;
                holdBasket.rb.isKinematic = false;
                holdBasket.rb.AddForce(player.gfx.forward * player.throwForce * 1.5f, ForceMode.Impulse);
                SlowDown(false);
                holdBasket.lastOwner = this;
                holdBasket.player = null;
                holdBasket.rb.mass = holdBasket.mass;
                basketCollider.enabled = false;
                holdBasket = null;
            }
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
