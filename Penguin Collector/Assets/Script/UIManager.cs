using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class SerializeObject
{
    public GameObject miniMap;
    public GameObject map;

    public GameObject panelBoat;

    public MobileJoystick joystick;
    public ButtonFire button;

    public GameObject lifePanel;
    public GameObject[] lifePoint;

    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI highScoreText;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverscoreText;
    public GameObject newHighScore;

    public GameObject levelComplete;

    public GameObject global;
}


public class UIManager : MonoBehaviour
{
    [SerializeField] private SerializeObject portrait;
    [SerializeField] private SerializeObject landscape;

    private SerializeObject currentFormat;
    public SerializeObject CurrentFormat => currentFormat;

    // Start is called before the first frame update
    void Start()
    {
        DisplayScore();
#if UNITY_ANDROID
        Mobile();
#endif

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID
        if (Input.deviceOrientation == DeviceOrientation.Portrait && currentFormat != portrait)
        {
            currentFormat = portrait;
            currentFormat.global.SetActive(true);
        }
        else
        {
            currentFormat = landscape;
            currentFormat.global.SetActive(true);
        }
#endif
    }

    public void AskToBoat()
    {
        currentFormat.panelBoat.SetActive(true);
        GameManager.Instance.PlayerScript.enabled = false;
    }

    public void BoatAccept()
    {
        currentFormat.panelBoat.SetActive(false);
        GameManager.Instance.LoadLevel("GameScene");
    }

    public void BoatDecline()
    {
        currentFormat.panelBoat.SetActive(false);
        GameManager.Instance.PlayerScript.enabled = true;
    }

    public void DisplayLife(int life)
    {
        for (int i = life; i < currentFormat.lifePoint.Length; i++)
        {
            currentFormat.lifePoint[i].SetActive(false);
        }
    }

    public void LaunchGame()
    {
        currentFormat.scorePanel.SetActive(false);
        currentFormat.miniMap.SetActive(true);
        currentFormat.lifePanel.SetActive(true);
    }

    public void DisplayScore()
    {
        currentFormat.scoreText.text = GameManager.Instance.CurrentScore.ToString();
        currentFormat.highScoreText.text = GameManager.Instance.HighScore.ToString();
    }

    public void DisplayLoad(int percent)
    {
        currentFormat.loadingText.text = "Loading ... " + percent + " / 100";
    }

    public void Menu()
    {
        GameManager.Instance.LoadLevel("MenuScene");
    }

    public void DisplayGameOver()
    {
        currentFormat.gameOverPanel.SetActive(true);
        currentFormat.gameOverscoreText.text = GameManager.Instance.CurrentScore.ToString();
        currentFormat.newHighScore.SetActive(true);
    }


    void Mobile()
    {
        currentFormat.joystick.gameObject.SetActive(true);
        currentFormat.button.gameObject.SetActive(true);
    }

    void LevelComplete()
    {
        currentFormat.levelComplete.SetActive(true);
    }



}
