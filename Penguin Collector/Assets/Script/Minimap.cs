using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    private Player player;
    private Camera camera;
    [SerializeField] private GameObject minimap;
    [SerializeField] private GameObject map;

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
        if (minimap.activeSelf)
        {
            camera.orthographicSize = 50;
            map.SetActive(true);
            minimap.SetActive(false);
        }
        else
        {
            camera.orthographicSize = 10;
            map.SetActive(false);
            minimap.SetActive(true);
        }
    }
}
