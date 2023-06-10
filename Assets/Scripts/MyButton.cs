using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MyButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
    public bool disableOnClick;
    public bool mouseOver;
    private Button button;
    private Coroutine runningCoroutine;

    Vector3 clickScale = new Vector2(1.2f, 1.2f);
    Vector3 enterScale = new Vector2(1.1f, 1.1f);
    Vector3 exitScale = new Vector2(1f, 1f);

    void Awake() => button = GetComponent<Button>();

    IEnumerator Click()
    {
        // Pop up
        while (transform.localScale.x < clickScale.x - .03f && mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, clickScale, .5f);
            yield return null;
        }
        transform.localScale = clickScale;

        // Scale Down
        while (transform.localScale.x > exitScale.x + .02f && mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, exitScale, .5f);
            yield return null;
        }
        transform.localScale = exitScale;
    }

    IEnumerator Enter()
    {
        if (button != null) button.Select();

        // Pop Up
        while (transform.localScale.x < enterScale.x - .02f && mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, enterScale, .5f);
            yield return null;
        }
        transform.localScale = enterScale;
    }

    IEnumerator Exit()
    {
        while (transform.localScale.x > exitScale.x + .02f && !mouseOver)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, exitScale, .5f);
            yield return null;
        }
        transform.localScale = exitScale;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (mouseOver) return;

        if (CanvasManager.instance.selectedButton != null && CanvasManager.instance.selectedButton.gameObject.activeInHierarchy)
            CanvasManager.instance.selectedButton.StartCoroutine(CanvasManager.instance.selectedButton.Exit());

        CanvasManager.instance.selectedButton = this;
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(Enter());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (disableOnClick) return;

        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(Click());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CanvasManager.instance.focusedButton = this;
        mouseOver = true;
        transform.localScale = exitScale;
        if (runningCoroutine != null) StopCoroutine(runningCoroutine);
        runningCoroutine = StartCoroutine(Enter());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        transform.localScale = enterScale;

        if (CanvasManager.instance.selectedButton != this)
        {
            if (runningCoroutine != null) StopCoroutine(runningCoroutine);
            runningCoroutine = StartCoroutine(Exit());
        }
    }

    public void GrantPriority(PlayerInput input) // To Whom?
    {
        // If a player with device Keyboard&Mouse spawned assign the UI to him

        var device = input.devices[0];

        print(device.GetType().ToString() + " has connected");
        if (device.GetType().ToString() == "UnityEngine.InputSystem.FastKeyboard" ||
            device.GetType().ToString() == "UnityEngine.InputSystem.Keyboard")
        {
            print("assigning input here");
            InputSystemUIInputModule uiModule = FindObjectOfType<InputSystemUIInputModule>();
            // Share the first player's action with the UI.
            uiModule.actionsAsset = input.actions;

            // Link to existing action instead of create new one
            uiModule.leftClick = InputActionReference.Create(input.actions["UI/Click"]);
        }

        // OnActivateMenu - switch
        // OnMouseEnter - switch
        // OnStickMove - switch
        // OnConfirm (both keyboard and joystick) - switch
    }
}
