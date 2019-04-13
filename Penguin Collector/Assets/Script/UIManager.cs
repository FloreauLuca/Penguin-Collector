﻿using System.Collections;
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
    [SerializeField] private TextMeshProUGUI gameOverscoreText;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private GameObject lifePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject levelComplete;
    [SerializeField] private GameObject newHighScore;



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
        
    }

    public void AskToBoat()
    {
        panelBoat.SetActive(true);
        GameManager.Instance.PlayerScript.enabled = false;
    }

    public void BoatAccept()
    {
        panelBoat.SetActive(false);
        GameManager.Instance.LoadLevel("GameScene");
    }

    public void BoatDecline()
    {
        panelBoat.SetActive(false);
        GameManager.Instance.PlayerScript.enabled = true;
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
        highScoreText.text = GameManager.Instance.HighScore.ToString();
    }

    public void DisplayLoad(int percent)
    {
        loadingText.text = "Loading ... " + percent + " / 100";
    }

    public void Menu()
    {
        GameManager.Instance.LoadLevel("MenuScene");
    }

    public void DisplayGameOver()
    {
        gameOverPanel.SetActive(true);
        gameOverscoreText.text = GameManager.Instance.CurrentScore.ToString();
        newHighScore.SetActive(true);
    }


    void Mobile()
    {
        joystick.gameObject.SetActive(true);
        button.gameObject.SetActive(true);
    }

    void LevelComplete()
    {
        levelComplete.SetActive(true);
    }



}
