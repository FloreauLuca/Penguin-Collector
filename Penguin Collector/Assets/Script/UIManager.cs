using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject panelBoat;

    [SerializeField] private MobileJoystick joystick;
    public MobileJoystick Joystick => joystick;
    [SerializeField] private ButtonFire button;
    public ButtonFire Button => button;

    [SerializeField] private GameObject[] lifePoint;

    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private GameObject lifePanel;
    [SerializeField] private GameObject GameOverPanel;


    // Start is called before the first frame update
    void Start()
    {
        DisplayScore();
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
        GameManager.Instance.LoadLevel("GameScene");
    }

    public void BoatDecline()
    {
        panelBoat.SetActive(false);
    }

    public void DisplayLife(int life)
    {
        for (int i = life; i < lifePoint.Length; i++)
        {
            lifePoint[i].SetActive(false);
        }
    }

    public void LaunchGame()
    {
        scorePanel.SetActive(false);
        miniMap.SetActive(true);
        lifePanel.SetActive(true);
    }

    public void DisplayScore()
    {
        scoreText.text = GameManager.Instance.CurrentScore.ToString();
    }

    public void Menu()
    {
        GameManager.Instance.LoadLevel("MenuScene");
    }

    public void DisplayGameOver()
    {
        GameOverPanel.SetActive(true);
    }
}
