using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy")]
public class SO_Enemy : ScriptableObject
{
    public GameObject enemyPrefab;
    public Sprite enemySprite;


    [Header("Statistique")]
    public float damage;
    public float life;



    public GameObject Instantiate(Vector2 position, int room)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<Enemy>().SoEnemy = this;
        enemy.GetComponent<Enemy>().Room = room;
        enemy.GetComponentInChildren<SpriteRenderer>().sprite = enemySprite;

        return enemy;
    }
}
