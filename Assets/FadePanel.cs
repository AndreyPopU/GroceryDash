using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanel : MonoBehaviour
{
    public static FadePanel instance;

    private CanvasGroup group;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(transform.parent.gameObject);
    }

    private void Start()
    {
        group = GetComponent<CanvasGroup>();
        StartCoroutine(Fade(0));
    }

    public IEnumerator Fade(int desire)
    {
        if (group.alpha > desire)
        {
            while(group.alpha > desire)
            {
                group.alpha -= .05f;
                yield return Time.deltaTime;
            }

            group.alpha = desire;
            yield break;
        }
        else
        {
            while (group.alpha < desire)
            {
                group.alpha += .2f;
                yield return Time.deltaTime;
            }

            group.alpha = desire;
        }
    }
}
