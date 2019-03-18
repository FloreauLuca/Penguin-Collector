using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private SO_Enemy soEnemy;

    public SO_Enemy SoEnemy
    {
        get { return soEnemy; }
        set { soEnemy = value; }
    }

    private int room = -1;

    public int Room
    {
        get { return room; }
        set { room = value; }
    }

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
            other.GetComponent<Player>().Hit(soEnemy.damage);
        }
    }
}
