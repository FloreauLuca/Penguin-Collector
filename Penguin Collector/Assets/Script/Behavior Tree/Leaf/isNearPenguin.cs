using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isNearPenguin : Leaf
{
    private Context context;

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        context = (Context)state;
        if (context.me.ViewPlayer())
        {
            return BTNodeStatus.SUCCESS;

        }
        else
        {
            return BTNodeStatus.FAILURE;
        }

    }

    public override void OnReset()
    {
    }
}
