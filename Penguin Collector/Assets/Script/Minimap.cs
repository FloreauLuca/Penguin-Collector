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
        GameManager.Instance.UiManagerScript.FullScreen();
        if (camera.orthographicSize == 10)
        {
            camera.orthographicSize = 50;
        }
        else
        {
            camera.orthographicSize = 10;
        }
    }

}
