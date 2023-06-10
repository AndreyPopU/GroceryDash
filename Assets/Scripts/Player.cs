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
using UnityEngine.UI;
using UnityEngine.InputSystem.Layouts;
using System.Runtime.ExceptionServices;

public class Player : MonoBehaviour
{
    [Header("Essential Stats")]
    public int index;
    public string nickname;
    public Transform hatPosition;
    public GameObject hat;
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
    public Transform basketHoldParent;

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

    [Header("Audio")]
    public AudioClip[] dashClips;
    public AudioClip pickUpClip;
    public AudioClip throwClip;
    public AudioClip bumpClip;

    [Header("Bumped")]
    public float bumpForce;
    private float bumpDuration;

    [Header("Customization")]
    public bool customize;
    public bool gamemode;

    [Header("Other")]
    public ParticleSystem milkEffect;
    public ParticleSystem bumpEffect;
    public ParticleSystem walkEffect;
    public MeshRenderer body, handL, handR;
    public Animator handsAnimator;
    public GameObject trailPrefab;
    public TextMeshProUGUI teamText;
    public bool inMilk;
    public int team;
    public LayerMask dropMask;

    private Animator animator;
    private Vector2 movement;
    private Vector3 dashDirection;
    private float baseRotateSpeed;
    private PlayerControls controls;
    [HideInInspector] public Rigidbody rb;
    public Transform gfx;
    [HideInInspector] public PlayerInput input;
    private AudioSource audioSource;

    private void Awake()
    {
        controls = new PlayerControls();
        gfx = transform.GetChild(0);
        SetColor();

        input = GetComponent<PlayerInput>();
        input.uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
        input.camera = CameraManager.instance.GetComponent<Camera>();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        baseRotateSpeed = rotateSpeed;
        baseDashCD = dashCD;
        dashCD = 0;
    }

    void FixedUpdate()
    {
        animator.SetFloat("speed", movement.normalized.magnitude, .1f, Time.deltaTime);

        // Walk Effect
        if (movement.magnitude > .1f && !walkEffect.isPlaying) walkEffect.Play();
        else if (movement.magnitude < .1f && walkEffect.isPlaying) walkEffect.Stop();

        // Navigate UI
        if (CanvasManager.instance.paused)
        {
            var device = GetComponent<PlayerInput>().devices[0];

            if (device.GetType().ToString() == "UnityEngine.InputSystem.DualShock.DualShock4GamepadHID" && movement.magnitude > 0.1f)
            {
                if (CanvasManager.instance.focusedButton != null) CanvasManager.instance.focusedButton.mouseOver = false;
                CanvasManager.instance.systemUI.actionsAsset = input.actions;
            }
        }

        if (dashCD > 0) dashCD -= Time.fixedDeltaTime;

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
        if (movement.magnitude > .1f && inMilk && !milkEffect.isPlaying) milkEffect.Play();
        else if ((movement.magnitude < .1f || !inMilk) && milkEffect.isPlaying) milkEffect.Stop();

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

    public void OnHat(CallbackContext context) => context.action.performed += _ => ChangeHat();

    public void OnThrow(CallbackContext context) => context.action.performed += _ => ThrowItem();

    public void OnDisplay(CallbackContext context) => context.action.performed += _ => Display();

    public void OnGrab(CallbackContext context) => context.action.performed += _ => Grab();

    public void OnHold(CallbackContext context) => context.action.performed += _ => holding = !holding;

    public void OnPause(CallbackContext context) => context.action.performed += _ => CanvasManager.instance.PauseGame(input);

    #endregion

    public void ChangeHat()
    {
        if (customize)
        {
            // Change hat
            CustomizationManager.instance.player = this;
            CustomizationManager.instance.ChangeHat();
        }
    }

    private void Dash(Vector3 direction)
    {
        if (CanvasManager.instance.paused)
        {
            var device = GetComponent<PlayerInput>().devices[0];

            if (device.name.ToString() == "Keyboard") return;

            CanvasManager.instance.GoBack();
            return;
        }

        // If input is neutral return
        if (direction.x == 0 && direction.z == 0 || dashing || dashCD > 0 || !canDash ||
            (GameManager.instance.gameStarted && !GameManager.instance.roundStarted && SceneManager.GetActiveScene().buildIndex > 0)) return;

        if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 5) Tutorial.instance.NextTask();

        int random = UnityEngine.Random.Range(0, dashClips.Length);
        audioSource.clip = dashClips[random];
        audioSource.Play();
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
        if (GameManager.instance.resultPanel.activeInHierarchy)
        {
            var device = GetComponent<PlayerInput>().devices[0];

            if (device.name.ToString() == "Keyboard") return;

            GameManager.instance.returnToMain.GetComponent<Button>().interactable = false;
            GameManager.instance.ReturnToMain();
        }

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
            PickUpProduct(false);
        else if (productsInRange.Count > 0) // Pick up product
            PickUpProduct(true);

    }
    
    #region Grab Functions

    public void PickUpBasket(bool pickUp)
    {
        // If there is a basket in range and you can't pick it up
        if (closestBasket != null && !closestBasket.canPickUp) return;

        handsAnimator.SetFloat("product", 0);
        StartCoroutine(SmoothTransition("basket", SaveLoadManager.BoolToInt(pickUp)));

        if (pickUp)
        {
            if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 1) Tutorial.instance.NextTask();

            audioSource.clip = pickUpClip;
            audioSource.Play();

            // If basket was part of stack - remove it 
            if (closestBasket.stackParent != null)
            {
                closestBasket.stackParent.baskets.Pop();
                if (closestBasket.stackParent.baskets.Count == 0)
                    closestBasket.stackParent.GetComponent<BoxCollider>().enabled = false;
                closestBasket.stackParent = null;
            }

            // Anchor basket in player's hands
            closestBasket.transform.SetParent(basketHoldParent);
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
            maxSpeed = 4.5f; //  - holdBasket.rb.mass;
            rotateSpeed = slowRotateSpeed;
            canDash = false;

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
            // Release on head

            // Prevent from dropping out of bounds
            RaycastHit hit;
            if (Physics.Raycast(transform.position, gfx.transform.forward, out hit, 2, dropMask))
                return;

            holdBasket.transform.SetParent(null);
            holdBasket.transform.position += gfx.forward * 2;
            holdBasket.lastOwner = holdBasket.player;
            holdBasket.player = null;
            holdBasket.rb.isKinematic = false;
            holdBasket.coreCollider.enabled = true;
            closestBasket = holdBasket;
            SceneManager.MoveGameObjectToScene(holdBasket.gameObject, SceneManager.GetActiveScene());
            holdBasket = null;
            SlowDown(false);
        }
    }

    public void LaunchBasket(Vector3 direction)
    {
        if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 2) Tutorial.instance.NextTask();

        audioSource.clip = throwClip;
        audioSource.Play();

        // Separate from player physics and enable it's own
        holdBasket.transform.SetParent(null);
        holdBasket.coreCollider.enabled = true;
        holdBasket.rb.isKinematic = false;

        // Add trail
        GameObject trail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity, holdBasket.transform);
        trail.transform.SetParent(holdBasket.transform);
        trail.transform.localPosition = Vector3.zero;

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
        handsAnimator.SetFloat("basket", 0);
        StartCoroutine(SmoothTransition("product", SaveLoadManager.BoolToInt(pickUp)));

        if (pickUp)
        {
            if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 0) Tutorial.instance.NextTask();

            audioSource.clip = pickUpClip;
            audioSource.Play();

            if (holdBasket != null) // If player carries a basket drop products there
            {
                // Check capacity
                if (holdBasket.products.Count >= holdBasket.capacity) return;

                holdBasket.AddProduct(GetProduct());
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
            audioSource.clip = throwClip;
            audioSource.Play();

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
        if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 2) Tutorial.instance.NextTask();

        // Separate from player and enable own physics
        holdProduct.transform.SetParent(null);
        holdProduct.rb.isKinematic = false;

        // Add trail
        GameObject trail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity, holdProduct.transform);
        trail.transform.SetParent(holdProduct.transform);
        trail.transform.localPosition = Vector3.zero;

        // Launch
        holdProduct.rb.AddForce(direction * throwForce, ForceMode.Impulse);
        audioSource.clip = throwClip;
        audioSource.Play();

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

    //public void PriorityPickUp()
    //{
    //    if (holdBasket != null || holdProduct != null) return;

    //    // Shoot out raycast
    //    RaycastHit hit;

    //    if (Physics.Raycast(transform.position, gfx.transform.forward, out hit, 5))
    //    {
    //        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

    //        print(hit.transform.name);

    //        if (hit.transform.GetComponent<Product>())
    //        {
    //            holdProduct = hit.transform.GetComponent<Product>();
    //            PickUpProduct(true, true);
    //        }
    //        else if (hit.transform.GetComponent<Shelf>())
    //        {
    //            holdProduct = Instantiate(hit.transform.GetComponent<Shelf>().product.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Product>();
    //            PickUpProduct(true, true);
    //        }
    //        else if (hit.transform.GetComponent<Basket>()) closestBasket = hit.transform.GetComponent<Basket>();
    //    }
    //}

    #endregion

    public IEnumerator SmoothTransition(string animation, int desire)
    {
        float current = 0;

        if (desire > 0)
        {
            while(current < desire)
            {
                current += .1f;
                handsAnimator.SetFloat(animation, current);
                yield return null;
            }
        }
        else if (desire < 1)
        {
            current = 1;
            while (current > desire)
            {
                current -= .1f;
                handsAnimator.SetFloat(animation, current);
                yield return null;
            }
        }
    }

    public void Bump(Vector3 direction, float force)
    {
        if (bumpDuration > 0) return;

        if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 6) Tutorial.instance.NextTask();

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

        audioSource.clip = bumpClip;
        audioSource.Play();
        bumpEffect.Play();
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

    public void SetColor()
    {
        body.material.color = color;
        handL.material.color = color;
        handR.material.color = color;
    }

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
            if (product.rb.velocity.magnitude > 2.5f && GameManager.instance.roundStarted)
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
