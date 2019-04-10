using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject panelBoat;

    [SerializeField] private MobileJoystick joystick;
    public MobileJoystick Joystick => joystick;
    [SerializeField] private ButtonFire button;
    public ButtonFire Button => button;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AskToBoat()
    {
        panelBoat.SetActive(true);
    }

    public void BoatAccept()
    {
        panelBoat.SetActive(false);
        GameManager.Instance.LoadLevel("Generation");

    }

    public void BoatDecline()
    {
        panelBoat.SetActive(false);
    }
}
