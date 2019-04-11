using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonFire : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    private bool buttonDown;
    public bool ButtonDown => buttonDown;
    public void OnPointerDown(PointerEventData ped)
    {
        buttonDown = true;
        GetComponent<Image>().color = Color.grey;
    }
    public void OnPointerUp(PointerEventData ped)
    {
        GetComponent<Image>().color = Color.white;
        buttonDown = false;
    }
}
