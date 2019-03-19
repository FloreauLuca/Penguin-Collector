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

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        distanceJoint2D = GetComponent<DistanceJoint2D>();
        distanceJoint2D.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
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
                distanceJoint2D.connectedBody = connectedRigidbody2D;
                distanceJoint2D.distance = 0.5f;
                distanceJoint2D.enabled = true;
                other.gameObject.GetComponent<Player>().PenguinList.Add(this);
            }
        }
    }
}
