using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITest : MonoBehaviour
{
    int UILayer;

    private void Start() => UILayer = LayerMask.NameToLayer("UI");

    private void Update()
    {
        IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private void IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        if (GameManager.instance.keyboardPlayer == null) return;

        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult raycastResult = eventSystemRaysastResults[index];
            if (raycastResult.gameObject.layer == UILayer)
            {
                if (raycastResult.gameObject.TryGetComponent(out MyButton button))
                {
                    if (!button.mouseOver)
                    {
                        CanvasManager.instance.systemUI.actionsAsset = GameManager.instance.keyboardPlayer.input.actions;
                        button.mouseOver = true;
                    }
                }
            }
        }
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

}