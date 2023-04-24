using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Checkout : MonoBehaviour
{
    public Product scanningProduct;
    public Transform scanPosition;
    public bool scanning;
    public bool open = true;
    public Material openMat, closedMat;

    [Header("UI")]
    public Canvas canvas;
    public Slider circularSlider;
    public Image feedbackImage;
    public Sprite correctIcon, incorrectIcon;

    private Coroutine runningCoroutine;
    private MeshRenderer lights;

    private void Start()
    {
        lights = GetComponentInChildren<MeshRenderer>();
        lights.material = openMat;
    }

    private void Update()
    {
        if (scanningProduct != null && !scanning) Scan();
    }

    private void ChangeColor()
    {
        open = !open;

        if (open) lights.material = openMat;
        else lights.material = closedMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Product product))
        {
            if (product != null && product.owner == null) scanningProduct = product;
        }
    }

    public void Scan()
    {
        scanning = true;
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(ScanCO());
    }

    private IEnumerator ScanCO()
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();

        // Setup Canvas
        circularSlider.value = 0;
        feedbackImage.gameObject.SetActive(false);
        canvas.gameObject.SetActive(true);

        // Anchor product
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

        // If the owner of the product needs to buy said product and it's not yet completed, then do that
        if (scanningProduct.lastOwner.shoppingList.shoppingItems.ContainsKey(scanningProduct.productName) &&
            scanningProduct.lastOwner.shoppingList.shoppingItems[scanningProduct.productName] > 0)
        {
            feedbackImage.sprite = correctIcon;
            scanningProduct.lastOwner.shoppingList.Buy(scanningProduct.productName);
        }
        else
        // If the owner of the product needs has the item in the shopping list but has already bought enough
        // or doesn't have the item at all, display incorrectly
        if (!scanningProduct.lastOwner.shoppingList.shoppingItems.ContainsKey(scanningProduct.productName) ||
            scanningProduct.lastOwner.shoppingList.shoppingItems[scanningProduct.productName] <= 0) feedbackImage.sprite = incorrectIcon;

        feedbackImage.gameObject.SetActive(true);

        // Finalize
        Destroy(scanningProduct.gameObject);
        scanningProduct = null;
        scanning = false;

        // Wait some time for players to recognize, can be interrupted by another purchase
        yield return new WaitForSeconds(1.5f);

        canvas.gameObject.SetActive(false);
        runningCoroutine = null;
    }

}
