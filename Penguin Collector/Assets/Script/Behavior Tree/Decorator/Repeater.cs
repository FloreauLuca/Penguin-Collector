using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeater : Decorator
{
    public Repeater(BTNode child) : base(child)
    {

    }
    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        BTNodeStatus returnedState = child.Behave(state);
        if (returnedState != BTNodeStatus.RUNNING)
        {
            Reset();
            child.Reset();
        }
        return BTNodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
