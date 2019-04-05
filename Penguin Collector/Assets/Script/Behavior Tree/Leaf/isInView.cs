using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isInView : Leaf
{
    private TreeGenerator context;
    private Vector2 point;
    private float radius;
    public isInView(Vector2 point, float radius, TreeGenerator context)
    {
        this.point = point;
        this.radius = radius;
        this.context = context;
    }

    public override BTNodeStatus OnBehave(BehaviourState state)
    {

        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return BTNodeStatus.SUCCESS;
            }
        }

        return BTNodeStatus.FAILURE;
    }

    public override void OnReset()
    {
    }
}
