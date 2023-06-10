using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartZone : MonoBehaviour
{
    public int playerCount;
    public TextMeshProUGUI startText;
    public TextMeshProUGUI playerCountText;
    public Slider progressSlider;
    public BoxCollider startCollider;
    public bool entered;
    public bool canStart;
    public GameObject obvious;

    void Start()
    {
        startCollider = GetComponent<BoxCollider>();

        playerCountText.text = playerCount + "/" + GameManager.instance.playerCount;
    }

    void Update()
    {
        obvious.transform.localScale = new Vector3(1 + Mathf.Abs(Mathf.Sin(Time.time * 2) / 10), 1 + Mathf.Abs(Mathf.Sin(Time.time * 2) / 10), 1) * .1f;

        if (GameManager.instance.playerCount == 0 || !canStart) return;

        if (playerCount == GameManager.instance.playerCount)
        {
            if (progressSlider.value < progressSlider.maxValue) progressSlider.value += 6 * Time.deltaTime;
            else
            {
                if (!entered)
                {
                    if (CanvasManager.instance.paused) CanvasManager.instance.PauseGame();
                    CanvasManager.instance.canPause = false;

                    if (Tutorial.instance.tutorialCompleted == 0) Tutorial.instance.EndTutorial();

                    // Disable join spots
                    for (int i = 0; i < GameManager.instance.joinCanvas.Count; i++)
                        GameManager.instance.joinCanvas[i].SetActive(false);

                    // Players drop items
                    foreach (Player player in GameManager.instance.players)
                    {
                        if (player.holdBasket != null) player.PickUpBasket(false);
                        if (player.holdProduct != null) player.PickUpProduct(false);
                    }

                    // Start Game
                    FadePanel.instance.Fade(1);
                    foreach (Player player in FindObjectsOfType<Player>())
                    {
                        player.canMove = false;
                        player.canDash = false;
                        player.rb.velocity = Vector3.zero;
                    }

                    FindObjectOfType<SlidingDoor>().inRange.Clear();

                    Invoke("ChangeScene", 1.5f);
                    entered = true;
                }
            }
        }
        else
        {
            if (progressSlider.value > 0) progressSlider.value -= 3 * Time.deltaTime;
        }
    }

    private void ChangeScene() // Invoked
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.GetComponent<Player>())
        {
            playerCount++;
            playerCountText.text = playerCount + "/" + GameManager.instance.playerCount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.GetComponent<Player>())
        {
            playerCount--;
            playerCountText.text = playerCount + "/" + GameManager.instance.playerCount;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, startCollider.size);
    }
}
