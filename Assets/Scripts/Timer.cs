using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float currentTime;

    public TextMeshProUGUI roundText;
    public TextMeshProUGUI countdownText;

    void Update()
    {
        if (CanvasManager.instance.paused) return;

        if (currentTime <= 0.04f)
        {
            // When round hasn't started and countdown hits 0 - start
            if (!roundText.gameObject.activeInHierarchy)
            {
                GameManager.instance.StartRound(true);
                CanvasManager.instance.canPause = true;
                roundText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(false);
                currentTime = 90;
            }
            else GameManager.instance.StartRound(false);
        }
        else currentTime -= 1 * Time.deltaTime;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        if (roundText.gameObject.activeInHierarchy) roundText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        else countdownText.text = (seconds + 1).ToString();
    }
}