using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shelf : MonoBehaviour
{
    public Product product;
    public Image productImage;

    private Coroutine runningCoroutine;
    private CanvasGroup group;

    private void Start()
    {
        productImage.sprite = Resources.Load<Sprite>(product.productName + "Icon");
        group = productImage.transform.parent.GetComponent<CanvasGroup>();
    }

    public void ShowProduct()
    {
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(ShowProductCO());
    }

    public IEnumerator ShowProductCO()
    {
        YieldInstruction instruction = new WaitForFixedUpdate();

        productImage.transform.parent.gameObject.SetActive(true);

        while(group.alpha < 1)
        {
            group.alpha += .1f;
            yield return instruction;
        }

        yield return new WaitForSeconds(3);

        while (group.alpha > 0)
        {
            group.alpha -= .1f;
            yield return instruction;
        }

        productImage.transform.parent.gameObject.SetActive(false);
        runningCoroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (!player.productsInRange.Contains(product)) player.productsInRange.Add(product);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (!pick.player.productsInRange.Contains(product)) pick.player.productsInRange.Add(product);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            if (player.productsInRange.Contains(product)) player.productsInRange.Remove(product);
        }
        else if (other.TryGetComponent(out PickUp pick))
        {
            if (pick.player.productsInRange.Contains(product)) pick.player.productsInRange.Remove(product);
        }
    }
}
