using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public List<Player> players;
    public Transform[] spawnPositions;
    public bool listsBound;

    private void Awake() => instance = this;

    void Start()
    {
        GameManager.instance.gameStarted = true;

        players.AddRange(FindObjectsOfType<Player>());

        for (int i =0; i < players.Count; i++)
        {
            players[i].transform.position = spawnPositions[i].position;
            players[i].transform.rotation = Quaternion.identity;
            players[i].gfx.transform.rotation = Quaternion.identity;
            CameraManager.instance.targets.Add(players[i].transform);
        }

        FadePanel.instance.Fade(0);
        Invoke("StartRound", .5f);
    }

    private void StartRound()
    {
        // Depending on GameMode bind shopping lists
        GameManager.instance.GetComponent<Timer>().enabled = true;
        GameManager.instance.GetComponent<Timer>().countdownText.gameObject.SetActive(true);
    }
}