using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Penguin : MonoBehaviour
{
    private DistanceJoint2D distanceJoint2D;
    private Rigidbody2D connectedRigidbody2D;
    private Rigidbody2D rigidbody2D;

    public Rigidbody2D MyRigidbody2D
    {
        get { return rigidbody2D; }
    }


    [Header("Boids")]
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;
    [SerializeField] float viewRadius;
    [SerializeField] float arrivalRadius;

    [SerializeField] float separateRadius;

    Vector2 desiredVelocity;

    private List<Vector2> followingPath;
    private int indexPath = 0;

    SpriteRenderer spriteRenderer;
    Animator animator;

    bool isRunning = false;


    private int timer = 1000;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        distanceJoint2D = GetComponent<DistanceJoint2D>();
        distanceJoint2D.enabled = false;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        isRunning = true;
    }

    private void Update()
    {

        if (connectedRigidbody2D != null)
        {
            if (Vector2.Distance(transform.position, connectedRigidbody2D.position) < viewRadius)
            {
                desiredVelocity = Vector2.zero;

                Vector2 seekForce = Seek();
                Vector2 separateForce = Separate();

                seekForce *= 1.5f;
                separateForce *= 0.5f;

                rigidbody2D.AddForce(seekForce);
                rigidbody2D.AddForce(separateForce);

                /*
                //Update sprite
                Vector2 dir = rigidbody2D.velocity;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                spriteRenderer.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                spriteRenderer.transform.eulerAngles = new Vector3(0, 0, spriteRenderer.transform.eulerAngles.z - 90);
                */
            }
            else
            {
                if (++timer >= 10)
                {
                    followingPath = GameManager.Instance.MapNav.Astar(transform.position, GameManager.Instance.PlayerScript.transform.position);
                    timer = 0;
                }

                if (followingPath != null)
                {
                    FollowPath();
                }

            }
        }
        animator.SetFloat("velX", rigidbody2D.velocity.x);
        animator.SetFloat("velY", rigidbody2D.velocity.y);
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
        rigidbody2D.velocity = rigidbody2D.velocity.normalized * 4f;

        if (Vector2.Distance(transform.position, followingPath[indexPath]) < 0.5f)
        {
            indexPath++;
        }

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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!other.gameObject.GetComponent<Player>().PenguinList.Contains(this))
            {
                if (other.gameObject.GetComponent<Player>().PenguinList.Count > 0)
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
                
            }
        }

    }


    void OnDrawGizmos()
    {
        if (!isRunning) return;
        //if(isRunning) return;


        Vector3 position = transform.position;

        Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(position, viewRadius);
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
