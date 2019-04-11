using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy")]
public class SO_Enemy : ScriptableObject
{
    public GameObject enemyPrefab;
    public RuntimeAnimatorController animator;


    [Header("Statistique")]
    public float damage;
    public float life;



    public GameObject Instantiate(Vector2 position, int room)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<Enemy>().SoEnemy = this;
        enemy.GetComponent<Enemy>().Region = room;

        return enemy;
    }
}
