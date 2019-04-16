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
    [SerializeField] private bool debug;

    private SerializeObject currentFormat;
    public SerializeObject CurrentFormat => currentFormat;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        if ((Input.deviceOrientation == DeviceOrientation.Portrait|| debug)  && currentFormat != portrait)
        {
            landscape.global.SetActive(false);
            currentFormat = portrait;
            currentFormat.global.SetActive(true);
            Mobile();
        }
        else if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight || !debug) && currentFormat == portrait)
        {
            portrait.global.SetActive(false);
            currentFormat = landscape;
            currentFormat.global.SetActive(true);
            Mobile();
        }
        else
        {
            portrait.global.SetActive(false);
            currentFormat = landscape;
            currentFormat.global.SetActive(true);
            Mobile();
        }
#else
        if (currentFormat != landscape)
        {
            portrait.global.SetActive(false);
            currentFormat = landscape;
            currentFormat.global.SetActive(true);
        }
#endif
        DisplayScore();

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID
        if ((Input.deviceOrientation == DeviceOrientation.Portrait)  && currentFormat != portrait)
        {
            currentFormat.global.SetActive(false);
            currentFormat = portrait;
            currentFormat.global.SetActive(true);
            Mobile();
        }
        else if ((Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight || Input.deviceOrientation == DeviceOrientation.Unknown) && currentFormat == portrait)
        {
            currentFormat.global.SetActive(false);
            currentFormat = landscape;
            currentFormat.global.SetActive(true);
            Mobile();
        }
#else
        if (currentFormat != landscape)
        {
            currentFormat.global.SetActive(false);
            currentFormat = landscape;
            currentFormat.global.SetActive(true);
        }
#endif
    }

    public void FullScreen()
    {
        if (GameManager.Instance.UiManagerScript.CurrentFormat.miniMap.activeSelf)
        {
            portrait.map.SetActive(true);
            portrait.miniMap.SetActive(false);

            landscape.map.SetActive(true);
            landscape.miniMap.SetActive(false);
        }
        else
        {
            portrait.map.SetActive(false);
            portrait.miniMap.SetActive(true);

            landscape.map.SetActive(false);
            landscape.miniMap.SetActive(true);
        }
    }

    public void AskToBoat()
    {
        portrait.panelBoat.SetActive(true);

        landscape.panelBoat.SetActive(true);
        GameManager.Instance.PlayerScript.enabled = false;
    }

    public void BoatAccept()
    {
        portrait.panelBoat.SetActive(false);

        landscape.panelBoat.SetActive(false);
        GameManager.Instance.LoadLevel("GameScene");
    }

    public void BoatDecline()
    {
        portrait.panelBoat.SetActive(false);

        landscape.panelBoat.SetActive(false);
        GameManager.Instance.PlayerScript.enabled = true;
    }

    public void DisplayLife(int life)
    {
        if (life >= 0)
        {
            for (int i = life; i < currentFormat.lifePoint.Length; i++)
            {
                portrait.lifePoint[i].SetActive(false);

                landscape.lifePoint[i].SetActive(false);
            }
        }
    }

    public void LaunchGame()
    {
        portrait.scorePanel.SetActive(false);
        portrait.miniMap.SetActive(true);
        portrait.lifePanel.SetActive(true);

        landscape.scorePanel.SetActive(false);
        landscape.miniMap.SetActive(true);
        landscape.lifePanel.SetActive(true);
    }

    public void DisplayScore()
    {
        Debug.Log(currentFormat.global);
        portrait.scoreText.text = GameManager.Instance.CurrentScore.ToString();
        portrait.highScoreText.text = GameManager.Instance.HighScore.ToString();

        landscape.scoreText.text = GameManager.Instance.CurrentScore.ToString();
        landscape.highScoreText.text = GameManager.Instance.HighScore.ToString();
    }

    public void DisplayLoad(int percent)
    {
        portrait.loadingText.text = "Loading ... " + percent + " / 100";

        landscape.loadingText.text = "Loading ... " + percent + " / 100";
    }

    public void Menu()
    {
        GameManager.Instance.CurrentScore = 0;
        GameManager.Instance.LoadLevel("MenuScene");
        
    }

    public void DisplayGameOver()
    {
        portrait.gameOverPanel.SetActive(true);
        portrait.gameOverscoreText.text = GameManager.Instance.CurrentScore.ToString();
        if (GameManager.Instance.HighScore < GameManager.Instance.CurrentScore)
        {
            portrait.newHighScore.SetActive(true);
            landscape.newHighScore.SetActive(true);
        }

        landscape.gameOverPanel.SetActive(true);
        landscape.gameOverscoreText.text = GameManager.Instance.CurrentScore.ToString();
    }


    void Mobile()
    {
        portrait.joystick.gameObject.SetActive(true);
        portrait.button.gameObject.SetActive(true);

        landscape.joystick.gameObject.SetActive(true);
        landscape.button.gameObject.SetActive(true);
    }

    void LevelComplete()
    {
        portrait.levelComplete.SetActive(true);
        landscape.levelComplete.SetActive(true);
    }



}
