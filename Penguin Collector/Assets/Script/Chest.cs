using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private int price;

    private bool open;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!open)
            {
                other.gameObject.GetComponent<Player>().Reward(price);
                open = true;
                GetComponentInChildren<SpriteRenderer>().color = Color.red;
            }
        }
    }
}
