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

        DontDestroyOnLoad(transform.parent.gameObject);
    }

    private void Start()
    {
        group = GetComponent<CanvasGroup>();
        Fade(0);
    }

    public void Fade(int desire)
    {
        StartCoroutine(FadeCO(desire));
    }

    private IEnumerator FadeCO(int desire)
    {
        if (group.alpha > desire)
        {
            while(group.alpha > desire)
            {
                group.alpha -= Time.deltaTime;
                yield return null;
            }

            group.alpha = desire;
            yield break;
        }
        else if (group.alpha < desire)
        {
            while (group.alpha < desire)
            {
                group.alpha += Time.deltaTime;
                yield return null;
            }

            group.alpha = desire;
        }
    }
}
