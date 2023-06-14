using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrailerManager : MonoBehaviour
{
    public List<Camera> cameras;
    public int index;

    void Start()
    {
        cameras.Insert(0, CameraManager.instance.GetComponent<Camera>());    
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.F1].wasPressedThisFrame)
        {
            cameras[index].enabled = false;
            index++;
            if (index >= cameras.Count) index = 0;
            cameras[index].enabled = true;

            if (index == 0)
            {
                GameManager.instance.shoppingList1.gameObject.SetActive(true);
                GameManager.instance.shoppingList2.gameObject.SetActive(true);
            }
            else
            {
                GameManager.instance.shoppingList1.gameObject.SetActive(false);
                GameManager.instance.shoppingList2.gameObject.SetActive(false);
            }
        }
    }
}
