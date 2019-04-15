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
    
    [SerializeField] float separateRadius;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;
    [SerializeField] protected float viewRadius;


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

    protected Rigidbody2D rigidbody2D;

    protected Vector2 startPosition;
    public Vector2 StartPosition => startPosition;

    private Room startRoom;

    public Room StartRoom => startRoom;

    private float timer;

    protected TreeGenerator treeGenerator;

    public enum EnemyState
    {
        STANDARDMOVE,
        GOBACKROOM,
        FOLLOWPLAYER
    }

    protected EnemyState currentEnemyState;
    public EnemyState CurrentEnemyState
    {
        get { return currentEnemyState; }
        set { currentEnemyState = value; }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        life = soEnemy.life;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = soEnemy.animator;
        startPosition = transform.position;
        startRoom = GameManager.Instance.MapScript.GetRoom(Vector2Int.RoundToInt(new Vector2(startPosition.x-0.5f, startPosition.y-0.5f)));
        treeGenerator = GetComponent<TreeGenerator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followingPath != null)
        {

            Vector2 separateForce = Separate();
            FollowPath();
            rigidbody2D.AddForce(separateForce * 5);
        }

        if (timer >= 0.1f)
        {
            timer = 0;
            switch (currentEnemyState)
            {
                case EnemyState.FOLLOWPLAYER:
                    if (followingPath != null)
                    {
                        FollowPath();
                    }
                    else
                    {
                        FollowPlayer();
                    }
                    animator.SetBool("nrv", true);
                    break;
                case EnemyState.GOBACKROOM:
                    if (followingPath != null)
                    {
                        FollowPath();
                    }
                    else
                    {
                        GoBackRoom();
                    }
                    animator.SetBool("nrv", false);

                    break;
                case EnemyState.STANDARDMOVE:
                    StandardMove();
                    animator.SetBool("nrv", false);

                    break;
            }
        }
        else
        {
            timer += Time.deltaTime;
        }



        animator.SetFloat("velX", rigidbody2D.velocity.x*10);
           animator.SetFloat("velY", rigidbody2D.velocity.y*10);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().Hit(soEnemy.damage);
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().Hit(soEnemy.damage);
        }
    }


    public void FollowPlayer()
    {
        if (Vector2.Distance(GameManager.Instance.MapNav.GetNode(transform.position).pos, transform.position) > 0.5f)
        {
            transform.position = GameManager.Instance.MapNav.GetNode(transform.position).pos;
        }
        if (followingPath != GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position))
        {
            followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
            indexPath = 1;
        }
    }

    public virtual bool GoBackRoom()
    {
        if (Vector2.Distance(GameManager.Instance.MapNav.GetNode(transform.position).pos, transform.position) > 0.5f)
        {
            transform.position = GameManager.Instance.MapNav.GetNode(transform.position).pos;
        }
        if (followingPath != GameManager.Instance.MapNav.Astar(transform.position, startPosition))
        {
            followingPath = GameManager.Instance.MapNav.Astar(transform.position, startPosition);
            indexPath = 1;
        }
        if (Mathf.Abs(transform.position.x - startPosition.x) > 0.5f || Mathf.Abs(transform.position.y - startPosition.y) >0.5f)
        {
            return false;
        }
        else
        {
            followingPath = null;
            return true;
        }

    }

    public virtual void StandardMove()
    {

    }


    public virtual bool ViewPlayer()
    {
        return false;
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
            return;
        }

        rigidbody2D.velocity = followingPath[indexPath] - (Vector2)transform.position;
        rigidbody2D.velocity = rigidbody2D.velocity.normalized * 2f;

        if (Vector2.Distance(transform.position, followingPath[indexPath]) < 0.1f)
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
        return;

        if (followingPath == null) return;
        if (followingPath.Count <= indexPath) return;
        foreach (Vector2 node in followingPath)
        {
            Gizmos.color = Color.green;
            if (node == followingPath[indexPath])
            {
                Gizmos.color = Color.yellow;
            }
            Gizmos.DrawWireSphere(node, 0.1f);
            
        }
    }
}
