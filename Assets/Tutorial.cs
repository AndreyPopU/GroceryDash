using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public static Tutorial instance;

    public GameObject[] tasks;
    public int index;
    public bool tutorialCompleted;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void StartTutorial()
    {
        StartCoroutine(ScaleText(1));
        tasks[index].SetActive(true);
    }

    public void NextTask()
    {
        tasks[index].SetActive(false);
        index++;
        tasks[index].SetActive(true);

        if (index == 7)
        {
            FindObjectOfType<StartZone>().canStart = true;
            FindObjectOfType<StartZone>().startText.text = "Stand here to start!\r\nPlayers:";
        }
    }

    public void EndTutorial()
    {
        StartCoroutine(ScaleText(0));
    }

    public IEnumerator ScaleText(int desire)
    {
        YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();
        CanvasGroup group = GetComponent<CanvasGroup>();

        if (transform.localScale.x < desire)
        {
            while (transform.localScale.x < desire)
            {
                group.alpha += .1f;
                transform.localScale += Vector3.one * .1f;
                yield return waitForFixedUpdate;
            }
        }
        else
        {
            while (transform.localScale.x > desire)
            {
                group.alpha += .15f;
                transform.localScale -= Vector3.one * .15f;
                yield return waitForFixedUpdate;
            }
        }

        transform.localScale = Vector3.one * desire;
    }
}
