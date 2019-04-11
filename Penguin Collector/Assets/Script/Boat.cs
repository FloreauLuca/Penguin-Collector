using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    private int penguinCount = 0;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            penguinCount += GameManager.Instance.PlayerScript.PenguinList.Count;
            if (GameManager.Instance.PlayerScript.PenguinList.Count>0)
            {
                foreach (Penguin penguin in GameManager.Instance.PlayerScript.PenguinList)
                {
                    penguin.gameObject.SetActive(false);
                }
                GameManager.Instance.PlayerScript.PenguinList = new List<Penguin>();
            }

            if (penguinCount > 0)
            {
                GameManager.Instance.UiManagerScript.AskToBoat();
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.CurrentScore += penguinCount;
    }
}
