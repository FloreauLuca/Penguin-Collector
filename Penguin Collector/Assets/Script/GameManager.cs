using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public int highscore = 0;
}



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

    private MapNavigation mapNav;
    public MapNavigation MapNav
    {
        get { return mapNav; }
        set { mapNav = value; }
    }

    private UIManager uiManagerScript;
    public UIManager UiManagerScript
    {
        get { return uiManagerScript; }
        set { uiManagerScript = value; }
    }

    private int currentScore = 0;
    public int CurrentScore
    {
        get { return currentScore; }
        set { currentScore = value; }
    }

    private int highScore = 0;
    public int HighScore
    {
        get { return highScore; }
        set { highScore = value; }
    }



    private string fileName;
    private string saveDataJson;


    private SaveData saveDataInstance;
    public SaveData SaveDataInstance
    {
        get { return saveDataInstance; }
        set { saveDataInstance = value; }
    }




    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoadingScene;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }

        if (Input.GetButtonDown("Restart"))
        {
            LoadLevel("GameScene");
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
        mapNav = FindObjectOfType<MapNavigation>();
        uiManagerScript = FindObjectOfType<UIManager>();
    }

    private void Awake()
    {
        saveDataInstance = new SaveData();
        fileName = Application.persistentDataPath + "/SaveData.json";
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Load();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Setup();
        
    }

    private void Start()
    {
        highScore = GameManager.Instance.saveDataInstance.highscore;
    }

    public void LoadLevel(string nameLevel)
    {
        SceneManager.LoadScene(nameLevel);
    }

    public void SpawnPlayer()
    {
        Vector2Int newPositionInt = mapScript.GetPlayerSpawn();
        Vector2 newPosition = new Vector2(newPositionInt.x + 0.5f, newPositionInt.y + 0.5f);
        playerScript.transform.position = newPosition;
        playerScript.CurrentRegion = mapScript.GetRegion(newPositionInt);
        playerScript.CurrentRoom = mapScript.GetRoomIndex(newPositionInt);
        mapScript.SetPlayerRoom(playerScript.CurrentRoom);
    }

    public void MapLoaded()
    {
        uiManagerScript.LaunchGame();
    }

    public void GameOver()
    {

        uiManagerScript.DisplayGameOver();
        if (highScore <= currentScore)
        {
            highScore = currentScore;
        }
        Save();
    }



    void Save()
    {
        saveDataInstance.highscore = highScore;
        saveDataJson = JsonUtility.ToJson(saveDataInstance);
            StreamWriter sw = File.CreateText(fileName);
            sw.Write(saveDataJson);
            sw.Close();
        Debug.Log(saveDataJson);
    }

    private void OnDestroy()
    {
        Save();
    }

    void Load()
    {
        if (File.Exists(fileName))
        {
            saveDataJson = File.ReadAllText(fileName);
            JsonUtility.FromJsonOverwrite(saveDataJson, saveDataInstance);
        }
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
#if !UNITY_WEBGL
		Application.Quit();
#endif
#endif
    }
}
