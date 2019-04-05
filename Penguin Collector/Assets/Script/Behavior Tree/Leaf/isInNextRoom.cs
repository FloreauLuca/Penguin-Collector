using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isInNextRoom : Leaf
{
    private TreeGenerator context;
    public isInNextRoom(TreeGenerator context)
    {
        this.context = context;
    }

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        if (context.me.StartRoom.neighboursRooms.Contains(context.mapScript.GetRoom(Vector2Int.RoundToInt(context.me.transform.position))))
            return BTNodeStatus.SUCCESS;
        else
            return BTNodeStatus.FAILURE;
    }

    public override void OnReset()
    {
    }
}