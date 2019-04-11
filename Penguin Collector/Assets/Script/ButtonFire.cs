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
        onButtonDown = true;
        Invoke("PointerStayDown", 0.1f);
    }

    private void PointerStayDown()
    {
        buttonDown = true;
        onButtonDown = false;
    }

    public void OnPointerUp(PointerEventData ped)
    {
        buttonDown = false;
        onButtonDown = false;
    }
}
