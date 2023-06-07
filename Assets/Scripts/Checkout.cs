using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Checkout : MonoBehaviour
{
    public Product scanningProduct;
    public Transform scanPosition;
    public bool scanning;
    public bool scanCD;
    public bool open = true;
    public bool self;
    public Material openMat, closedMat;
    public Transform checkoutPoint;
    public ParticleSystem confetti;

    [Header("Basket")]
    public Basket basket;
    public BasketStack stack;

    [Header("UI")]
    public Canvas canvas;
    public Slider circularSlider;
    public Image feedbackImage;
    public Sprite correctIcon, incorrectIcon;
    public Image checkoutImage;

    private Coroutine runningCoroutine;
    private MeshRenderer lights;
    private AudioSource audioSource;

    private void Start()
    {
        checkoutImage.sprite = self ? Resources.Load<Sprite>("SelfCheckoutIcon") : Resources.Load<Sprite>("CheckoutIcon");
        lights = GetComponentInChildren<MeshRenderer>();
        lights.material = openMat;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (scanningProduct != null && !scanning) Scan();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Product product))
        {
            if (product != null && product.owner == null && scanningProduct == null) scanningProduct = product;
        }
    }

    public void Scan()
    {
        if (scanCD) return;

        scanning = true;
        scanCD = true;
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(ScanCO());
    }

    private IEnumerator ScanCO()
    {
        if (scanningProduct.basket != null)
        {
            Basket basket = scanningProduct.basket;
            basket.StartCoroutine(basket.ActivateLevel(basket.products.Count, false));
            basket.productIcons[basket.products.Count].StartCoroutine(basket.FadeIcon(basket.productIcons[basket.products.Count].GetComponent<CanvasGroup>(), 0));
            if (basket.lastOwner.holdBasket == null) basket.lastOwner.SlowDown(false);
        }

        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        // Setup Canvas
        circularSlider.value = 0;
        feedbackImage.gameObject.SetActive(false);
        canvas.gameObject.SetActive(true);

        // Anchor product
        scanningProduct.transform.localScale = Vector3.zero;
        scanningProduct.StartCoroutine(scanningProduct.Enlarge());
        scanningProduct.transform.parent = null;
        scanningProduct.canPickUp = false;
        scanningProduct.rb.isKinematic = true;
        scanningProduct.transform.position = scanPosition.position;
        scanningProduct.transform.rotation = Quaternion.identity;

        // Fill up Slider
        while(circularSlider.value < circularSlider.maxValue)
        {
            circularSlider.value += .1f;

            yield return waitForFixedUpdate;
        }

        circularSlider.value = circularSlider.maxValue;

        // Decide on outcome

        // If main menu just give positive outcome
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (!Tutorial.instance.tutorialCompleted && Tutorial.instance.index == 3) Tutorial.instance.NextTask();
            confetti.Play();
            feedbackImage.sprite = correctIcon;
        }
        else // Actually decide
        {
            // If the owner of the product needs to buy said product and it's not yet completed, then do that
            if (scanningProduct.lastOwner.shoppingList.shoppingItems.ContainsKey(scanningProduct.productName) &&
                scanningProduct.lastOwner.shoppingList.shoppingItems[scanningProduct.productName] > 0)
            {
                confetti.Play();
                feedbackImage.sprite = correctIcon;
                scanningProduct.lastOwner.shoppingList.Buy(scanningProduct.productName);
            }
            else
            // If the owner of the product needs has the item in the shopping list but has already bought enough
            // or doesn't have the item at all, display incorrectly
            if (!scanningProduct.lastOwner.shoppingList.shoppingItems.ContainsKey(scanningProduct.productName) ||
                scanningProduct.lastOwner.shoppingList.shoppingItems[scanningProduct.productName] <= 0) feedbackImage.sprite = incorrectIcon;
        }

        audioSource.Play();
        feedbackImage.gameObject.SetActive(true);

        // Finalize
        Destroy(scanningProduct.gameObject);
        scanningProduct = null;
        scanning = false;

        GameManager.instance.scans++;

        #if ENABLE_CLOUD_SERVICES_ANALYTICS
            Analytics.CustomEvent("Scans", new Dictionary<string, object>
            {
                { "scans", GameManager.instance.scans },
            });

        AnalyticsService.Instance.CustomData("Scans", new Dictionary<string, object>
            {
                { "scans", GameManager.instance.scans },
            });
#endif

        // Remove basket from checkout and add it to a stack of baskets
        if (basket != null && basket.products.Count == 0)
        {
            stack.AddBasket(basket);
            basket.canPickUp = true;
            basket = null;
        }

        // Wait some time for players to recognize, can be interrupted by another purchase
        yield return new WaitForSeconds(.5f);
        scanCD = false;

        // Then wait some more
        yield return new WaitForSeconds(1f);

        canvas.gameObject.SetActive(false);
        runningCoroutine = null;
    }
}
