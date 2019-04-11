using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Succeeder : Decorator
{
    public Succeeder(BTNode child) : base(child)
    {

    }
    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        BTNodeStatus ret = child.Behave(state);

        if (ret == BTNodeStatus.RUNNING)
            return BTNodeStatus.RUNNING;

        return BTNodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
