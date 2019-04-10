using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonFire : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    private bool buttonDown;
    public bool ButtonDown => buttonDown;

    private bool onButtonDown;
    public bool OnButtonDown => onButtonDown;

    public void OnPointerDown(PointerEventData ped)
    {
        buttonDown = true;
        onButtonDown = true;
    }

    private void PointerStayDown()
    {
        onButtonDown = false;
    }

    public void OnPointerUp(PointerEventData ped)
    {
        buttonDown = false;
        onButtonDown = false;
    }
}
