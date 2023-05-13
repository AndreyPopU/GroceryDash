using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    public int index;
    public string nickname;
    public Color color;
    public Player teammate;
    [Header("Player stats")]
    [SerializeField] private float speed = 1000;
    [SerializeField] private float counterMovement = 250;
    [SerializeField] private float maxSpeed = 7;
    [SerializeField] private float rotateSpeed = .4f;
    [SerializeField] private float slowRotateSpeed = .2f;
    public float throwForce = 10;

    [Header("Invididual shopping list")]
    public Dictionary<string, int> shoppingItems = new Dictionary<string, int>();

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
    public BoxCollider pickUpCollider;

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

    [Header("Customization")]
    public bool customize;
    public bool gamemode;

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
        dashCD = 0;
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

    public void OnDisplay(CallbackContext context) => context.action.performed += _ => Display();

    public void OnGrab(CallbackContext context) => context.action.performed += _ => Grab();

    public void OnHold(CallbackContext context) => context.action.performed += _ => holding = !holding;

    public void OnPause(CallbackContext context) => context.action.performed += _ => GameManager.instance.PauseGame();

    #endregion

    private void Dash(Vector3 direction)
    {
        // If input is neutral return
        if (direction.x == 0 && direction.z == 0 || (GameManager.instance.gameStarted && !GameManager.instance.roundStarted)
            || dashing || dashCD > 0 || !canDash) return;
        
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


    private void Hold()
    {
        if (!holding || (GameManager.instance.gameStarted && !GameManager.instance.roundStarted)) return;

        if (closestPlayer != null)
        {
            print("holding onto");
        }
    }

    private void ThrowItem()
    {
        if (GameManager.instance.gameStarted && !GameManager.instance.roundStarted) return;

        if (holdProduct != null) LaunchProduct(gfx.forward);
        else if (holdBasket != null) LaunchBasket(gfx.forward);
    }

    private void Display()
    {
        foreach (Shelf shelf in FindObjectsOfType<Shelf>())
            shelf.ShowProduct();
    }

    private void Grab()
    {
        if (gamemode)
        {
            // Change Mode
            GameMode.instance.ChangeMode();
        }

        if (customize)
        {
            // Enter toilet
            CustomizationManager.instance.player = this;
            CustomizationManager.instance.ChangeColor();

            // Open customization menu

        }

        if (GameManager.instance.gameStarted && !GameManager.instance.roundStarted) return;

        if (holdProduct != null) // Drop product
        {
            PickUpProduct(false);
            return;
        }
        else if (closestProduct != null) // Pick up product
        {
            // If closest product is held by another player or can't be picked up - return
            if (closestProduct.owner != null || !closestProduct.canPickUp) return; 

            PickUpProduct(true);
            return;
        }

        if (holdBasket != null)
        {
            if (closestProduct == null) // Drop basket
            {
                PickUpBasket(false);
                return;
            }
        }
        
        else if (closestBasket != null && holdProduct == null) // Pick up basket if not holding product already
        {
            if (closestBasket.player != null) return;

            PickUpBasket(true);
            return;
        }

    }
    
    #region Grab Functions

    public void PickUpBasket(bool pickUp)
    {
        // Slow Down player
        SlowDown(pickUp);

        if (pickUp)
        {
            // If basket was part of stack - remove it 
            if (closestBasket.stackParent != null)
            {
                closestBasket.stackParent.baskets.RemoveAt(0);
                if (closestBasket.stackParent.baskets.Count == 0) Destroy(closestBasket.stackParent.gameObject);
                closestBasket.stackParent = null;
            }

            // Anchor basket in player's hands
            closestBasket.transform.SetParent(holdParent);
            closestBasket.transform.localPosition = closestBasket.holdOffset;
            closestBasket.transform.localEulerAngles = Vector3.up * -90;
            closestBasket.player = this;
            closestBasket.rb.isKinematic = true;
            closestBasket.coreCollider.enabled = false;
            holdBasket = closestBasket;

            // Give ownership of every product in the basket to the player who holds it
            foreach (Product product in holdBasket.products)
            {
                product.owner = this;
                product.lastOwner = this;
            }

            closestBasket = null;

            // Slow Down player
            maxSpeed = 5.5f - holdBasket.rb.mass;
        }
        else
        {
            holdBasket.transform.SetParent(null);
            holdBasket.player = null;
            holdBasket.rb.isKinematic = false;
            holdBasket.coreCollider.enabled = true;
            closestBasket = holdBasket;
            holdBasket = null;
        }

        // Enable & adjust colliders
        if (holdBasket != null)
        {
            basketCollider.center = holdBasket.center;
            basketCollider.size = new Vector3(holdBasket.coreCollider.size.z, holdBasket.coreCollider.size.y, holdBasket.coreCollider.size.x);
            pickUpCollider.center = holdBasket.center;
            pickUpCollider.size = basketCollider.size + Vector3.one * 1.5f;
        }
        basketCollider.enabled = pickUp;
        pickUpCollider.enabled = pickUp;
    }

    public void LaunchBasket(Vector3 direction)
    {
        // Separate from player physics and enable it's own
        holdBasket.transform.SetParent(null);
        holdBasket.coreCollider.enabled = true;
        holdBasket.rb.isKinematic = false;

        // Launch
        holdBasket.rb.AddForce(direction * throwForce * 1.5f, ForceMode.Impulse);
        
        // Deal with ownership
        holdBasket.lastOwner = holdBasket.player;
        holdBasket.player = null;
        basketCollider.enabled = false;
        holdBasket = null;
        SlowDown(false);
    }

    public void PickUpProduct(bool pickUp)
    {
        if (pickUp)
        {
            if (holdBasket != null) // If player carries a basket drop products there
            {
                // Check capacity
                if (holdBasket.products.Count >= holdBasket.capacity) return;

                holdBasket.AddProduct(GetProduct());
                maxSpeed = 5.5f - holdBasket.rb.mass;
                return;
            }
            
            holdProduct = GetProduct();

            // Anchor to player's hands
            holdProduct.transform.SetParent(holdParent);
            holdProduct.transform.localPosition = Vector3.zero;
            holdProduct.transform.localEulerAngles = Vector3.zero;
            holdProduct.rb.isKinematic = true;

            // Assign ownership
            holdProduct.owner = this;
            holdProduct.lastOwner = this;
        }
        else
        {
            // Separate from player and enable physics
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.owner = null;
            holdProduct = null;
        }
    }

    public void LaunchProduct(Vector3 direction)
    {
        // Separate from player and enable own physics
        holdProduct.transform.SetParent(null);
        holdProduct.rb.isKinematic = false;

        // Launch
        holdProduct.rb.AddForce(direction * throwForce, ForceMode.Impulse);

        // Deal with ownership
        holdProduct.lastOwner = holdProduct.owner;
        holdProduct.owner = null;
        holdProduct = null;
    }

    public Product GetProduct()
    {
        if (!closestProduct.gameObject.activeInHierarchy) // If product is from shelf - Instantiate 
        {
            return Instantiate(closestProduct.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Product>();
        }
        else return closestProduct; // Else pick up off the floor
    }

    #endregion

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

        // If reconnected text is still active - destroy it
        GameObject reconnected = GameObject.Find("Reconnected" + index.ToString());
        if (reconnected != null) Destroy(reconnected);

        // Set Color and Name to match the player
        disconnected.gameObject.name = "Disconnected" + index.ToString();
        disconnected.GetComponent<TextMeshProUGUI>().color = color;
        disconnected.GetComponent<TextMeshProUGUI>().text = nickname + index + " Disconnected!";
        disconnected.transform.position -= Vector3.up * (70 * index);
        GameManager.instance.StartCoroutine(GameManager.instance.ScaleText(disconnected.transform, 1));
        Destroy(disconnected, 3);
    }

    public void ReconnectPlayer(PlayerInput input)
    {
        GameObject reconnected = Instantiate(GameManager.instance.disconnectedTextPrefab, GameManager.instance.GetComponentInChildren<Canvas>().transform);
        
        // If disconnected text is still active - destroy it
        GameObject disconnected = GameObject.Find("Disconnected" + index.ToString());
        if (disconnected != null) Destroy(disconnected);

        // Set Color and Name to match the player
        reconnected.gameObject.name = "Reconnected" + index.ToString();
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
            canDash = false;
        }
        else
        {
            maxSpeed = holdBasket != null ? 5.5f - holdBasket.rb.mass: 7;
            rotateSpeed = holdBasket != null ? slowRotateSpeed : baseRotateSpeed;
            canDash = holdBasket != null ? false : true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>()) closestPlayer = other.GetComponent<Player>();
        if (other.GetComponent<GameMode>()) gamemode = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() && closestPlayer == other.GetComponent<Player>()) closestPlayer = null;
        if (other.GetComponent<GameMode>()) gamemode = false;
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
                LaunchBasket(player.gfx.forward);
        }

        if (holdProduct != null || holdBasket != null) return;

        if (collision.collider.TryGetComponent(out Product product))
        {
            if (product.rb.velocity.magnitude > 2.5f)
            {
                product.rb.velocity = Vector3.zero;
                PickUpProduct(product);
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
