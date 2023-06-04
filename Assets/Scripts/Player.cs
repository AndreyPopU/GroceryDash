using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.Analytics;
using System.Security.Cryptography;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    [Header("Essential Stats")]
    public int index;
    public string nickname;
    public Color color;
    public string colorName;
    public Player teammate;
    public int score;
    public bool connected;
    public float timeOut = 15;

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
    public List<Product> productsInRange;
    public ShoppingList shoppingList;
    public Product holdProduct;
    public Transform holdParent;

    [Header("Hold onto items")]
    public Player closestPlayer;

    [Header("Basket")]
    public Basket holdBasket;
    public Basket closestBasket;

    [Header("Dash")]
    public bool canDash = true;
    public float dashRange;
    public float dashDuration = .25f;
    public float dashCD = 1;
    public ParticleSystem dashEffect;
    private float baseDashCD;
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
    private float baseRotateSpeed;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        gfx = transform.GetChild(0);
        gfx.GetComponent<MeshRenderer>().material.color = color;

        PlayerInput input = GetComponent<PlayerInput>();
        input.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        input.camera = CameraManager.instance.GetComponent<Camera>();
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody>();
        baseRotateSpeed = rotateSpeed;
        baseDashCD = dashCD;
        dashCD = 0;
    }

    void FixedUpdate()
    {
        if (dashCD > 0) dashCD -= Time.deltaTime;

        //Counteract sliding and sloppy movement
        CounterMovement();

        if (dashing || bumpDuration > 0 || !canMove) return;

        Movement();
        Hold();
        gfx.forward = Vector3.Lerp(gfx.forward, new Vector3(movement.x, 0, movement.y), rotateSpeed);
    }

    private void Update()
    {
        if (bumpDuration > 0) bumpDuration -= Time.deltaTime;

        if (!connected)
        {
            if (timeOut > 0) timeOut -= Time.deltaTime;
            else GameManager.instance.DisconnectPlayer(this);
        }
    }

    public void EnableController(bool enabled)
    {
        var device = GetComponent<PlayerInput>().devices[0];

        if (device.GetType().ToString() == "UnityEngine.InputSystem.DualShock.DualShock4GamepadHID")
        {
            DualShockGamepad ds4 = (DualShockGamepad)device;
            
            if (enabled) ds4.SetLightBarColor(color);
            else ds4.ResetHaptics();
        }
    }

    public void DisableController()
    {
        var device = GetComponent<PlayerInput>().devices[0];

        if (device.GetType().ToString() == "UnityEngine.InputSystem.DualShock.DualShock4GamepadHID")
        {
            DualShockGamepad ds4 = (DualShockGamepad)device;
            ds4.ResetHaptics();
        }
    }

    public void Rumble() => StartCoroutine(RumbleCo(.6f, 1, .15f));

    public IEnumerator RumbleCo(float low, float high, float duration)
    {
        // Set Controller Color to player color
        var device = GetComponent<PlayerInput>().devices[0];

        if (device.GetType().ToString() == "UnityEngine.InputSystem.DualShock.DualShock4GamepadHID")
        {
            DualShockGamepad ds4 = (DualShockGamepad)device;
            ds4.SetMotorSpeeds(low, high);

            yield return new WaitForSeconds(duration);

            ds4.SetMotorSpeeds(0, 0);
        }
    }

    private void Movement()
    {
        // Cap speed
        if (rb.velocity.magnitude > maxSpeed) rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

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

    public void OnPause(CallbackContext context) => context.action.performed += _ => CanvasManager.instance.PauseGame();

    #endregion

    private void Dash(Vector3 direction)
    {
        if (CanvasManager.instance.paused)
        {
            CanvasManager.instance.GoBack();
            return;
        }

        // If input is neutral return
        if (direction.x == 0 && direction.z == 0 || dashing || dashCD > 0 || !canDash ||
            (GameManager.instance.gameStarted && !GameManager.instance.roundStarted && SceneManager.GetActiveScene().buildIndex > 0)) return;
        
        dashEffect.Play();
        dashing = true;
        dashCD = baseDashCD;

        GameManager.instance.dashes++;

        #if ENABLE_CLOUD_SERVICES_ANALYTICS

            Analytics.CustomEvent("Dash", new Dictionary<string, object>
            {
                { "dash", GameManager.instance.dashes },
            });

            AnalyticsService.Instance.CustomData("Dash", new Dictionary<string, object>
            {
                { "dash", GameManager.instance.dashes },
            });

        #endif

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
        foreach (Display display in FindObjectsOfType<Display>())
            display.ShowIcon();
    }

    private void Grab()
    {
        if (productsInRange.Count > 0)
            for (int i = 0; i < productsInRange.Count; i++)
                if (productsInRange[i] == null)
                    productsInRange.RemoveAt(i);

        // Change Mode
        if (gamemode) GameMode.instance.ChangeMode();

        if (customize)
        {
            // Change color
            CustomizationManager.instance.player = this;
            CustomizationManager.instance.ChangeColor();
        }

        if (holdBasket != null)
        {
            if (productsInRange.Count <= 0) // Drop basket
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

        if (GameManager.instance.gameStarted && !GameManager.instance.roundStarted) return;

        if (holdProduct != null) // Drop product
        {
            PickUpProduct(false);
            return;
        }
        else if (productsInRange.Count > 0) // Pick up product
        {
            PickUpProduct(true);
            return;
        }
    }
    
    #region Grab Functions

    public void PickUpBasket(bool pickUp)
    {
        if (closestBasket != null && !closestBasket.canPickUp) return;

        if (pickUp)
        {
            // If basket was part of stack - remove it 
            if (closestBasket.stackParent != null)
            {
                closestBasket.stackParent.baskets.Pop();
                if (closestBasket.stackParent.baskets.Count == 0)
                    closestBasket.stackParent.GetComponent<BoxCollider>().enabled = false;
                closestBasket.stackParent = null;
            }

            // Anchor basket in player's hands
            closestBasket.transform.SetParent(holdParent);
            closestBasket.transform.localPosition = Vector3.zero;
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

            GameManager.instance.basketsUsed++;

            #if ENABLE_CLOUD_SERVICES_ANALYTICS
            Analytics.CustomEvent("BasketUsed", new Dictionary<string, object>
            {
                { "basketUsed", GameManager.instance.basketsUsed },
            });

            AnalyticsService.Instance.CustomData("BasketUsed", new Dictionary<string, object>
            {
                { "basketUsed", GameManager.instance.basketsUsed },
            });

            #endif
        }
        else
        {
            LaunchBasket(gfx.forward);
            
            // Release on head

            //holdBasket.transform.SetParent(null);
            //holdBasket.lastOwner = holdBasket.player;
            //holdBasket.player = null;
            //holdBasket.rb.isKinematic = false;
            //holdBasket.coreCollider.enabled = true;
            //closestBasket = holdBasket;
            //SceneManager.MoveGameObjectToScene(holdBasket.gameObject, SceneManager.GetActiveScene());
            //holdBasket = null;
        }

        // Slow Down player
        SlowDown(pickUp);
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
        SceneManager.MoveGameObjectToScene(holdBasket.gameObject, SceneManager.GetActiveScene());
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

            GameManager.instance.productsUsed++;

            #if ENABLE_CLOUD_SERVICES_ANALYTICS

                Analytics.CustomEvent("ProductUsed", new Dictionary<string, object>
                {
                    { "productUsed", GameManager.instance.productsUsed },
                });

                AnalyticsService.Instance.CustomData("ProductUsed", new Dictionary<string, object>
                {
                    { "productUsed", GameManager.instance.productsUsed },
                });

            #endif
        }
        else
        {
            // Separate from player and enable physics
            holdProduct.transform.SetParent(null);
            holdProduct.rb.isKinematic = false;
            holdProduct.owner = null;
            SceneManager.MoveGameObjectToScene(holdProduct.gameObject, SceneManager.GetActiveScene());
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
        SceneManager.MoveGameObjectToScene(holdProduct.gameObject, SceneManager.GetActiveScene());
        holdProduct = null;
    }

    public Product GetProduct()
    {
        // Loop through the list of products in range
        int closestIndex = 0;
        float closestDistance = 999;

        for (int i = 1; i < productsInRange.Count; i++)
        {
            // If closest product is held by another player or can't be picked up - return
            if (productsInRange[0].owner != null || !productsInRange[0].canPickUp) continue;

            float distanceBetween = Vector3.Distance(transform.position, productsInRange[i].transform.position);
            if (distanceBetween < closestDistance)
            {
                closestDistance = distanceBetween;
                closestIndex = i;
            }
        }

        Product closestProduct = productsInRange[closestIndex];

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

        GameManager.instance.bumps++;

#if ENABLE_CLOUD_SERVICES_ANALYTICS
        Analytics.CustomEvent("Bumps", new Dictionary<string, object>
        {
            { "bumps", GameManager.instance.bumps },
        });

        AnalyticsService.Instance.CustomData("Bumps", new Dictionary<string, object>
        {
            { "bumps", GameManager.instance.bumps },
        });
#endif

        rb.velocity = Vector3.zero;
        rb.AddForce(direction * force * Time.deltaTime, ForceMode.Impulse);

        if (holdProduct != null) PickUpProduct(false);
        else if (holdBasket != null) PickUpBasket(false);

        bumpDuration = .2f;

        if (connected) Rumble();
    }

    #region Events

    public void DisconnectPlayer(PlayerInput input)
    {
        GameObject disconnected = Instantiate(GameManager.instance.disconnectedTextPrefab, GameManager.instance.GetComponentInChildren<Canvas>().transform);

        // If reconnected text is still active - destroy it
        GameObject reconnected = GameObject.Find("Reconnected" + index.ToString());
        if (reconnected != null) Destroy(reconnected);

        timeOut = 15;
        connected = false;

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

        connected = true;
        // Set Color and Name to match the player
        reconnected.gameObject.name = "Reconnected" + index.ToString();
        reconnected.GetComponent<TextMeshProUGUI>().color = color;
        reconnected.GetComponent<TextMeshProUGUI>().text = nickname + index + " Reconnected!";
        reconnected.transform.position -= Vector3.up * (70 * index);
        GameManager.instance.StartCoroutine(GameManager.instance.ScaleText(reconnected.transform, 1));
        Invoke("ReconnectController", .2f);
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

    void ReconnectController() // Invoked
    {
        EnableController(true);
    }

    public void OnApplicationQuit()
    {
        EnableController(false);
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
