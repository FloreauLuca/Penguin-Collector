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
    
    Vector2 desiredVelocity;

    SpriteRenderer spriteRenderer;

    bool isRunning = false;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        distanceJoint2D = GetComponent<DistanceJoint2D>();
        distanceJoint2D.enabled = false;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        isRunning = true;
    }
    
    // Update is called once per frame
    void Update() {
        desiredVelocity = Vector2.zero;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, viewRadius);
        Vector2 seekVelocity = Vector2.zero;

        foreach(Collider2D col in colliders) {
            if(col.GetComponent<Rigidbody2D>() == connectedRigidbody2D && connectedRigidbody2D != null) {
                seekVelocity = col.transform.position - transform.position;
                if (seekVelocity.magnitude < arrivalRadius) {
                    //seekVelocity = seekVelocity.normalized * Mathf.Lerp(0, maxSpeed, seekVelocity.magnitude / arrivalRadius);
                    seekVelocity = Vector2.zero;
                }
                else {
                    seekVelocity = seekVelocity.normalized * maxSpeed;
                }

                desiredVelocity += seekVelocity - rigidbody2D.velocity;
            }
        }

        if(desiredVelocity.magnitude > maxForce) {
            desiredVelocity += seekVelocity - rigidbody2D.velocity;
            desiredVelocity = desiredVelocity.normalized * maxForce;
        }
        rigidbody2D.AddForce(desiredVelocity);

        //Update sprite
        Vector2 dir = rigidbody2D.velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        spriteRenderer.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        spriteRenderer.transform.eulerAngles = new Vector3(0, 0, spriteRenderer.transform.eulerAngles.z - 90);
    }

    void OnDrawGizmos() {
        if(!isRunning) return;
        if(isRunning) return;


        Vector3 position = transform.position;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(position, viewRadius);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(position, arrivalRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + (Vector3)rigidbody2D.velocity);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + (Vector3)desiredVelocity);
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
}
