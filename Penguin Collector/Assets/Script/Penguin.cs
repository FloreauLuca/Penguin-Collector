using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Penguin : MonoBehaviour
{
    private Rigidbody2D connectedRigidbody2D;
    private Rigidbody2D rigidbody2D;

    public Rigidbody2D MyRigidbody2D
    {
        get { return rigidbody2D; }
    }

    private bool connected;

    public bool Connected => connected;

    


    [Header("Boids")]
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;
    [SerializeField] float viewRadius;
    [SerializeField] float arrivalRadius;

    [SerializeField] float separateRadius;
    [SerializeField] bool group;

    [SerializeField] float detectRadius;


    Vector2 desiredVelocity;

    private List<Vector2> followingPath;
    private int indexPath = 0;

    SpriteRenderer spriteRenderer;
    Animator animator;

    bool isRunning = false;


    private float timer = 1000;

    private float debugTimer = 0;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        isRunning = true;
    }

    private void FixedUpdate()
    {
        if (connectedRigidbody2D != null)
        {
            if (Vector2.Distance(transform.position, connectedRigidbody2D.position) < viewRadius)
            {
                followingPath = null;
                indexPath = 0;
                desiredVelocity = Vector2.zero;

                Vector2 seekForce = Seek();
                Vector2 separateForce = Separate();
                Vector2 fleeForce = Flee();
                /*
                seekForce *= 1.5f;
                separateForce *= 0.5f;
                fleeForce *= 2f;
                */
                Vector2 newForce = (seekForce * 2 + separateForce * 1 + fleeForce * 5)/8;

                rigidbody2D.AddForce(newForce);
                
            }
            else
            {
                Vector2 separateForce = Separate();
                
                if (timer >= 3)
                {
                    if (Vector2.Distance(GameManager.Instance.MapNav.GetNode(transform.position).pos, transform.position) > 0.3f)
                    {
                        transform.position = GameManager.Instance.MapNav.GetNode(transform.position).pos;
                    }
                    followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
                    indexPath = 0;
                    timer = 0;
                }
                else
                {
                    timer += Time.deltaTime;
                }

                if (followingPath != null)
                {
                    FollowPath();
                }

                rigidbody2D.AddForce(separateForce);
            }
        }
        animator.SetFloat("velX", rigidbody2D.velocity.x);
        animator.SetFloat("velY", rigidbody2D.velocity.y);
    }


    public void FollowPath()
    {
        if (debugTimer > 2)
        {
            transform.position = GameManager.Instance.MapNav.GetNode(transform.position).pos;
        }

        if (indexPath >= followingPath.Count)
        {
            rigidbody2D.velocity = Vector2.zero;
            indexPath = 0;
            if (Vector2.Distance(GameManager.Instance.MapNav.GetNode(transform.position).pos, transform.position) > 0.5f)
            {
                transform.position = GameManager.Instance.MapNav.GetNode(transform.position).pos;
            }
            followingPath = GameManager.Instance.MapNav.Astar((Vector2)transform.position, GameManager.Instance.PlayerScript.transform.position);
            debugTimer = 0;
            return;
        }

        rigidbody2D.velocity = followingPath[indexPath] - (Vector2)transform.position;
        rigidbody2D.velocity = rigidbody2D.velocity.normalized * 5f;

        if (Vector2.Distance(transform.position, followingPath[indexPath]) < 0.5f)
        {
            indexPath++;
            debugTimer = 0;
        }

        debugTimer += Time.deltaTime;

    }


    // Update is called once per frame
    Vector2 Seek()
    {
        desiredVelocity = Vector2.zero;

        Vector2 seekVelocity = Vector2.zero;

        if (connectedRigidbody2D != null)
        {
            seekVelocity = connectedRigidbody2D.transform.position - transform.position;
            if (seekVelocity.magnitude < arrivalRadius)
            {
                //seekVelocity = seekVelocity.normalized * Mathf.Lerp(0, maxSpeed, seekVelocity.magnitude / arrivalRadius);
                seekVelocity = Vector2.zero;
            }
            else
            {
                seekVelocity = seekVelocity.normalized * maxSpeed;
            }

            desiredVelocity += seekVelocity - rigidbody2D.velocity;
        }

        if (desiredVelocity.magnitude > maxForce)
        {
            desiredVelocity += seekVelocity - rigidbody2D.velocity;
            desiredVelocity = desiredVelocity.normalized * maxForce;
        }

        return desiredVelocity;


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

    Vector2 Flee()
    {

        desiredVelocity = Vector2.zero;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, separateRadius);

        foreach (Collider2D col in colliders)
        {
            if ((col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Player")) && !col.isTrigger)
            {
                Vector2 seekVelocity = transform.position - col.transform.position;
                seekVelocity = seekVelocity.normalized * maxSpeed;
                desiredVelocity += seekVelocity - rigidbody2D.velocity;
            }
        }

        if (desiredVelocity.magnitude > maxForce)
        {
            desiredVelocity = desiredVelocity.normalized * maxForce;
        }

        return desiredVelocity;

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!other.gameObject.GetComponent<Player>().PenguinList.Contains(this))
            {
                
                if (other.gameObject.GetComponent<Player>().PenguinList.Count > 0 && !group)
                {
                    connectedRigidbody2D = other.gameObject.GetComponent<Player>().PenguinList[other.gameObject.GetComponent<Player>().PenguinList.Count - 1].MyRigidbody2D;
                }
                else
                {
                    connectedRigidbody2D = other.gameObject.GetComponent<Rigidbody2D>();
                }
                /*
                distanceJoint2D.connectedBody = connectedRigidbody2D;
                distanceJoint2D.distance = 0.25f;
                distanceJoint2D.enabled = true;
                */
                other.gameObject.GetComponent<Player>().PenguinList.Add(this);
                rigidbody2D.isKinematic = false;
                connected = true;
                if (!GetComponent<AudioSource>().isPlaying)
                {
                    GetComponent<AudioSource>().Play();
                }

            }
        }

    }


    public bool ViewPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }


    void OnDrawGizmos()
    {
       

        if (!isRunning) return;

        Vector3 position = transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, detectRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(position, viewRadius);
        /*
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(position, arrivalRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + (Vector3)rigidbody2D.velocity);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + (Vector3)desiredVelocity);
        */
        if (followingPath == null) return;
            foreach (Vector2 node in followingPath)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawWireSphere(node, 0.1f);

            }
    }
}
