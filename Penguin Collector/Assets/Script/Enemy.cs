using System.Collections;
using System.Collections.Generic;
using System.Net;
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

    private List<Vector2> followingPath;
    private int indexPath = 0;

    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rigidbody2D;

    private int timer = 1000;
    // Start is called before the first frame update
    void Start()
    {
        life = soEnemy.life;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (followingPath != null)
        {
            FollowPath();
            if (++timer >= 10)
            {
                followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
                timer = 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().Hit(soEnemy.damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
              followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
            
        }
    }

    public void FollowPath()
    {

        if (indexPath >= followingPath.Count)
        {
            rigidbody2D.velocity = Vector2.zero;
            indexPath = 0;
            followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
            return;
        }

        rigidbody2D.velocity = followingPath[indexPath] - (Vector2)transform.position;
        rigidbody2D.velocity = rigidbody2D.velocity.normalized * 2f;

        if (Vector2.Distance((Vector2) transform.position, followingPath[indexPath]) < 0.5f)
        {
            indexPath++;
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

    void OnDrawGizmos()
    {
        if (followingPath == null) return;
        foreach (Vector2 node in followingPath)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(node, 0.1f);
            
        }
    }
}
