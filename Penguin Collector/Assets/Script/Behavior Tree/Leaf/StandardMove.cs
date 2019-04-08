using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardMove : Leaf
{
    private Context context;
    

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        context = (Context)state;
        context.me.StandardMove();

        // TODO - perhaps should test success of the actual attack and return failure if we missed

        return BTNodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
