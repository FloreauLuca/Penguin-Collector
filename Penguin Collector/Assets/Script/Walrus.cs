using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walrus : Enemy
{
    private Vector2 lastStandardPosition = new Vector2Int();
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        treeGenerator.behaviourState = new Context(this, GameManager.Instance.MapScript, GameManager.Instance.MapNav);
        treeGenerator.behaviourTree = treeGenerator.CreateWalrusBehaviourTree();
        lastStandardPosition = startPosition;

    }

    public override bool GoBackRoom()
    {
        lastStandardPosition = transform.position;
        return base.GoBackRoom();
    }


    public override void StandardMove()
    {
        rigidbody2D.velocity = lastStandardPosition - (Vector2)transform.position;
        rigidbody2D.velocity = rigidbody2D.velocity.normalized * 2f;

        if (Vector2.Distance(transform.position, lastStandardPosition) < 0.2f)
        {
            BoundsInt boundsPenguin = new BoundsInt(-1, -1, 0, 3, 3, 1);
            Vector2Int newPosition = Vector2Int.RoundToInt(new Vector2(lastStandardPosition.x - 0.5f, lastStandardPosition.y - 0.5f));
            List<Vector2Int> nextPosition = new List<Vector2Int>();
            foreach (Vector3Int b in boundsPenguin.allPositionsWithin)
            {
                if (newPosition.x + b.x < 0 || newPosition.x + b.x >= GameManager.Instance.MapScript.MapSize) continue;
                if (newPosition.y + b.y < 0 || newPosition.y + b.y >= GameManager.Instance.MapScript.MapSize) continue;
                if (b.x != 0 && b.y != 0) continue;
                if (b.x == 0 && b.y == 0) continue;
                if (!GameManager.Instance.MapScript.MapOfCells[newPosition.x + b.x, newPosition.y + b.y].isAlive) continue;
                if (!GameManager.Instance.MapScript.MapOfCells[newPosition.x + b.x, newPosition.y + b.y].occuped)
                {
                    nextPosition.Add(new Vector2Int(newPosition.x + b.x, newPosition.y + b.y));
                }
            }

            if (nextPosition.Count > 0)
            {
                lastStandardPosition = nextPosition[Random.Range(0, nextPosition.Count)];
                lastStandardPosition += Vector2.one / 2;

            }
        }
    }


    public override bool ViewPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, viewRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Vector3 position = transform.position;
        Gizmos.color = new Color(0.5f,0.25f,0);
        Gizmos.DrawWireSphere(position, viewRadius);
    }
}
