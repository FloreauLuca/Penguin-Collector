using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bear : Enemy
{
    private Penguin connectedPenguin;

    public Penguin ConnectedPenguin
    {
        get { return connectedPenguin; }
        set { connectedPenguin = value; }
    }

    private List<Vector2> standardMove;
    private int indexStandardMove;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        treeGenerator.behaviourState = new Context(this, GameManager.Instance.MapScript, GameManager.Instance.MapNav);
        treeGenerator.behaviourTree = treeGenerator.CreateBearBehaviourTree(connectedPenguin);

        standardMove = new List<Vector2>();
        standardMove.Add(connectedPenguin.transform.position + Vector3.right);
        standardMove.Add(connectedPenguin.transform.position + Vector3.right+Vector3.down);
        standardMove.Add(connectedPenguin.transform.position + Vector3.down);
        standardMove.Add(connectedPenguin.transform.position + Vector3.left + Vector3.down);
        standardMove.Add(connectedPenguin.transform.position + Vector3.left);
        standardMove.Add(connectedPenguin.transform.position + Vector3.left + Vector3.up);
        standardMove.Add(connectedPenguin.transform.position + Vector3.up);
        standardMove.Add(connectedPenguin.transform.position + Vector3.right + Vector3.up);
    }

    public override void StandardMove()
    {
        if (indexStandardMove >= standardMove.Count)
        {
            rigidbody2D.velocity = Vector2.zero;
            indexStandardMove = 0;
            return;
        }

        rigidbody2D.velocity = standardMove[indexStandardMove] - (Vector2)transform.position;
        rigidbody2D.velocity = rigidbody2D.velocity.normalized * 2f;

        if (Vector2.Distance(transform.position, standardMove[indexStandardMove]) < 0.1f)
        {
            indexStandardMove++;
        }
    }


    public override bool ViewPlayer()
    {
        return connectedPenguin.ViewPlayer();
    }
    
}
