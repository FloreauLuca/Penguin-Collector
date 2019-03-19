using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : Weapon
{
    [SerializeField] private float speed;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GetComponent<Rigidbody2D>().velocity = transform.rotation * Vector2.up * speed;
    }

    protected override void Collision(GameObject enemy)
    {
        base.Collision(enemy);
        Destroy(gameObject);
    }
}
