using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Display : MonoBehaviour
{
    private Image image;
    public CanvasGroup group;
    private Coroutine runningCoroutine;

    private void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        if (image != null) group = image.GetComponent<CanvasGroup>();
    }

    public void ShowIcon()
    {
        if (Tutorial.instance.tutorialCompleted == 0 && Tutorial.instance.index == 4) Tutorial.instance.NextTask();

        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(ShowIconCO());
    }

    private IEnumerator ShowIconCO()
    {
        YieldInstruction instruction = new WaitForFixedUpdate();

        if (image!= null) image.gameObject.SetActive(true);

        while (group.alpha < 1)
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

        if (image != null) image.gameObject.SetActive(false);
        runningCoroutine = null;
    }
}
