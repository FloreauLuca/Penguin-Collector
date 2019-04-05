using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoBackRoom : Leaf
{

    private TreeGenerator context;
    public GoBackRoom(TreeGenerator context)
    {
        this.context = context;
    }

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        context.me.GoBackRoom();

        // TODO - perhaps should test success of the actual attack and return failure if we missed

        return BTNodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
