using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool disableOnClick;
    private bool mouseOver;
    Vector3 clickScale = new Vector2(1.2f, 1.2f);
    Vector3 enterScale = new Vector2(1.1f, 1.1f);
    Vector3 exitScale = new Vector2(1f, 1f);

    IEnumerator Click()
    {
        if (disableOnClick) yield break;

        while (transform.localScale.x < clickScale.x - .03f && mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, clickScale, .5f);
            yield return new WaitForSeconds(.02f);
        }
        transform.localScale = clickScale;

        while (transform.localScale.x > exitScale.x + .02f && mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, exitScale, .5f);
            yield return new WaitForSeconds(.02f);
        }
        transform.localScale = exitScale;
    }

    IEnumerator Enter()
    {
        while (transform.localScale.x < enterScale.x - .02f && mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, enterScale, .5f);
            yield return new WaitForSeconds(.02f);
        }
        transform.localScale = enterScale;
    }

    IEnumerator Exit()
    {
        while (transform.localScale.x > exitScale.x + .02f && !mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, exitScale, .5f);
            yield return new WaitForSeconds(.02f);
        }
        transform.localScale = exitScale;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(Click());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        transform.localScale = exitScale;
        StartCoroutine(Enter());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        transform.localScale = enterScale;
        StartCoroutine(Exit());
    }
}
