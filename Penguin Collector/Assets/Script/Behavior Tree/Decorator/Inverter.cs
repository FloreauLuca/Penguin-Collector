using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Decorator
{
    public Inverter(BTNode child) : base(child)
    {

    }
    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        switch (child.Behave(state))
        {
            case BTNodeStatus.RUNNING:
                return BTNodeStatus.RUNNING;

            case BTNodeStatus.SUCCESS:
                return BTNodeStatus.FAILURE;

            case BTNodeStatus.FAILURE:
                return BTNodeStatus.SUCCESS;
        }

        Debug.LogError("INVERTER ERROR");
        return BTNodeStatus.FAILURE;
    }

    public override void OnReset()
    {
    }
}
