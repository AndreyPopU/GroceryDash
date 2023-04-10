using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float currentTime;

    [SerializeField] TextMeshProUGUI roundText;
    [SerializeField] TextMeshProUGUI countdownText;

    void Update()
    {
        if (!GameManager.instance.roundStarted) return;

        if (currentTime <= 0)
        {
            if (!roundText.gameObject.activeInHierarchy)
            {
                roundText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(false);
                currentTime = 180;
                GameManager.instance.StartRound(true);
            }
            else
            {
                currentTime = 0;
                GameManager.instance.StartRound(false);
            }
        }
        else currentTime -= 1 * Time.deltaTime;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        if (roundText.gameObject.activeInHierarchy) roundText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        else countdownText.text = (seconds + 1).ToString();
    }
}