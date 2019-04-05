using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Composite
{
    private int currentChild = 0;

    public Sequence(BTNode[] nodes) : base(nodes)
    {

    }
    

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        BTNodeStatus ret = children[currentChild].Behave(state);

        switch (ret)
        {
            case BTNodeStatus.SUCCESS:
                currentChild++;
                break;

            case BTNodeStatus.FAILURE:
                return BTNodeStatus.FAILURE;
        }

        if (currentChild >= children.Count)
        {
            return BTNodeStatus.SUCCESS;
        }
        else if (ret == BTNodeStatus.SUCCESS)
        {
            // if we succeeded, don't wait for the next tick to process the next child
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
