using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Player playerScript;
    

    protected virtual void Start()
    {
        playerScript = GameManager.Instance.PlayerScript;
    }
    
    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !other.isTrigger)
        {
            Collision(other.gameObject);
        }
    }

    protected virtual void Collision(GameObject enemy)
    {
        enemy.GetComponent<Enemy>().Hit(playerScript.Damage);
    }

}
