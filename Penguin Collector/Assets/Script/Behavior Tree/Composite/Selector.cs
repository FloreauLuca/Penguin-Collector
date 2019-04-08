using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Composite
{
    private int currentChild = 0;

    public Selector(params BTNode[] nodes) : base(nodes)
    {

    }

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        if (currentChild >= children.Count)
        {
            return BTNodeStatus.FAILURE;
        }

        BTNodeStatus returnedStatus = children[currentChild].Behave(state);

        switch (returnedStatus)
        {
            case BTNodeStatus.SUCCESS:
                return BTNodeStatus.SUCCESS;

            case BTNodeStatus.FAILURE:
                currentChild++;

                return OnBehave(state);
        }

        return BTNodeStatus.RUNNING;
    }

    public override void OnReset()
    {
        currentChild = 0;
        foreach (BTNode child in children)
        {
            child.Reset();
        }
    }
}
