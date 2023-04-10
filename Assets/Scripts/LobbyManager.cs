using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public RawImage[] icons;

    public void AddToPlayers(PlayerInput input)
    {
        icons[input.playerIndex].color = Color.white;
    }

    public void RemoveFromPlayers(PlayerInput input)
    {
        icons[input.playerIndex].color = new Color(0,0,0,0);
    }
}
