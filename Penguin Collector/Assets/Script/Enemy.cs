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

    private int region = -1;

    public int Region
    {
        get { return region; }
        set { region = value; }
    }

    private float life;

    public float Life
    {
        get { return life; }
        set { life = value; }
    }


    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        life = soEnemy.life;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().Hit(soEnemy.damage);
        }
    }


    public void Hit(float hitDamage)
    {
        life -= hitDamage;

        if (life < 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(Invincibility());
        }
    }

    IEnumerator Invincibility()
    {
        spriteRenderer.color = new Color(1, 0, 0, 1);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = new Color(1, 1, 1, 1);

    }
}
