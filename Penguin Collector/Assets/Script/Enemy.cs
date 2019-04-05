using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEditor.Animations;


public class Enemy : MonoBehaviour
{
    [SerializeField] private SO_Enemy soEnemy;
    public SO_Enemy SoEnemy
    {
        get { return soEnemy; }
        set { soEnemy = value; }
    }
    
    [SerializeField] float separateRadius;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;


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
    private Animator animator;

    private Rigidbody2D rigidbody2D;

    private Vector2 startPosition;

    public Vector2 StartPosition => startPosition;
    private Room startRoom;

    public Room StartRoom => startRoom;

    private float timer = 1000;
    // Start is called before the first frame update
    void Start()
    {
        life = soEnemy.life;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = soEnemy.animator;
        startPosition = transform.position;
        startRoom = GameManager.Instance.MapScript.GetRoom(Vector2Int.RoundToInt(startPosition));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followingPath != null)
        {

            Vector2 separateForce = Separate();
            FollowPath();
            if (timer >= 2)
            {
                followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }

            rigidbody2D.AddForce(separateForce);
        }
           animator.SetFloat("velX", rigidbody2D.velocity.x);
           animator.SetFloat("velY", rigidbody2D.velocity.y);
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
              //followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
            
        }
    }


    public void FollowPlayer()
    {
        followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
        
    }

    public void GoBackRoom()
    {
        followingPath = GameManager.Instance.MapNav.Astar(transform.position, startPosition);

    }

    public virtual void StandardMove()
    {

    }

    Vector2 Separate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, separateRadius);

        Vector2 separateForce = Vector2.zero;

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject.CompareTag("Penguin"))
            {
                Vector2 seekVelocity = transform.position - col.transform.position;
                seekVelocity = seekVelocity.normalized * maxSpeed;
                separateForce += seekVelocity - rigidbody2D.velocity;
            }
        }

        if (separateForce.magnitude > maxForce)
        {
            separateForce = separateForce.normalized * maxForce;
        }

        return separateForce;
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

        if (Vector2.Distance(transform.position, followingPath[indexPath]) < 0.2f)
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
