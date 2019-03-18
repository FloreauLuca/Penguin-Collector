﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Player playerScript;

    public Player PlayerScript
    {
        get { return playerScript; }
        set { playerScript = value; }
    }

    private CellularAutomata mapScript;

    public CellularAutomata MapScript
    {
        get { return mapScript; }
        set { mapScript = value; }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoadingScene;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoadingScene;
    }

    //this function is activated every time a scene is loaded
    private void OnLevelFinishedLoadingScene(Scene scene, LoadSceneMode mode)
    {
        Setup();
        Debug.Log("Scene Loaded");
    }

    private void Setup()
    {
        playerScript = FindObjectOfType<Player>();
        mapScript = FindObjectOfType<CellularAutomata>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Setup();
    }

    public void LoadLevel(string nameLevel)
    {
        SceneManager.LoadScene(nameLevel);
    }

    public void MapLoaded()
    {
        Vector2Int newPositionInt = mapScript.GetSpawn();
        Vector2 newPosition = newPositionInt;
        playerScript.transform.position = newPosition;
        playerScript.CurrentRoom = mapScript.GetRoom(newPositionInt);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR || UNITY_WEBGL
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}