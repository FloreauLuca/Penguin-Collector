using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    private Player player;
    private Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.Instance.PlayerScript;
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Map"))
        {
            FullScreen();
        }
    }

    public void FullScreen()
    {
        if (GameManager.Instance.UiManagerScript.CurrentFormat.miniMap.activeSelf)
        {
            camera.orthographicSize = 50;
            GameManager.Instance.UiManagerScript.CurrentFormat.map.SetActive(true);
            GameManager.Instance.UiManagerScript.CurrentFormat.miniMap.SetActive(false);
        }
        else
        {
            camera.orthographicSize = 10;
            GameManager.Instance.UiManagerScript.CurrentFormat.map.SetActive(false);
            GameManager.Instance.UiManagerScript.CurrentFormat.miniMap.SetActive(true);
        }
    }
}
