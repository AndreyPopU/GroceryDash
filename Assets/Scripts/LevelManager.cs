using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public List<Player> players;
    public Transform[] spawnPositions;
    public Transform center;
    public Transform exitPoint;
    public bool listsBound;

    private Basket[] baskets;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        GameManager.instance.gameStarted = true;

        players = GameManager.instance.players;
        baskets = FindObjectsOfType<Basket>();

        PreparePlayers();

        FadePanel.instance.Fade(0);
        Invoke("StartRound", .5f);

        // Set up timer
        Timer timer = FindObjectOfType<Timer>();
        timer.countdownText.transform.localScale = Vector3.zero;
        timer.countdownText.gameObject.SetActive(true);
        StartCoroutine(GameManager.instance.ScaleText(timer.countdownText.transform, 1));
        timer.currentTime = 3;
        timer.enabled = true;

        #if ENABLE_CLOUD_SERVICES_ANALYTICS
            Analytics.CustomEvent("LobbyPlayers", new Dictionary<string, object>
            {
                { "PlayersPerLobby", GameManager.instance.players.Count },
            });

        AnalyticsService.Instance.CustomData("LobbyPlayers", new Dictionary<string, object>
            {
                { "PlayersPerLobby", GameManager.instance.players.Count },
            });
#endif
    }
     
    public void PreparePlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = spawnPositions[i].position;
            players[i].transform.rotation = Quaternion.identity;
            players[i].gfx.transform.rotation = Quaternion.identity;
            players[i].productsInRange.Clear();
        }

        CameraManager.instance.transform.position = center.position + CameraManager.instance.offset;
    }

    private void StartRound()
    {
        GameManager.instance.GetComponent<Timer>().enabled = true;
        GameManager.instance.GetComponent<Timer>().countdownText.gameObject.SetActive(true);
    }

    public void ResetRoom()
    {
        // Reset Basket stacks
        foreach (BasketStack stack in FindObjectsOfType<BasketStack>()) stack.ResetStack();

        foreach (Basket basket in baskets)
        {
            // Reset Basket positions
            basket.transform.position = basket.startPosition;
            basket.transform.rotation = basket.startRotation;

            int products = basket.products.Count;

            if (products > 0)
            {
                for (int i = 0; i < products; i++)
                {
                    // Reset basket canvas
                    basket.productIcons[i].StartCoroutine(basket.FadeIcon(basket.productIcons[i].GetComponent<CanvasGroup>(), 0));

                    // Reset levels of full
                    Transform[] levelItems = basket.transform.GetChild(0).GetChild(i).GetComponentsInChildren<Transform>();

                    foreach (Transform item in levelItems)
                        item.transform.localScale = Vector3.zero;
                }
            }

            // Reset basket items
            basket.products.Clear();
        }

        // Delete Products rolling around
        foreach (Product product in FindObjectsOfType<Product>()) product.Destroy();
    }
}
