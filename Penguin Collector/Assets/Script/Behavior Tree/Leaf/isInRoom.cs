using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isInRoom : Leaf
{
    private Context context;
   
    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        context = (Context)state;
        if (context.me.StartRoom == context.mapScript.GetRoom(Vector2Int.RoundToInt(new Vector2(context.me.transform.position.x - 0.5f, context.me.transform.position.y - 0.5f))))
            return BTNodeStatus.SUCCESS;
        else
            return BTNodeStatus.FAILURE;
    }

    public override void OnReset()
    {
    }
}
