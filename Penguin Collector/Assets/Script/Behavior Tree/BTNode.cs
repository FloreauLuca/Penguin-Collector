using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BTNodeStatus
{
    FAILURE,
    SUCCESS,
    RUNNING,
}

public abstract class BehaviourState
{
}


public abstract class BTNode
{
    private bool debug = false;
    public virtual BTNodeStatus Behave(BehaviourState state)
    {
        BTNodeStatus retNodeStatus = OnBehave(state);
        
        if (debug)
        {
            string result = "Unknow";
            switch (retNodeStatus)
            {
                case BTNodeStatus.FAILURE:
                    result = "failure";
                    break;

                case BTNodeStatus.RUNNING:
                    result = "running";
                    break;

                case BTNodeStatus.SUCCESS:
                    result = "success";
                    break;
            }

            Debug.Log("Behaving: " + GetType().Name + " - " + result);
        }

        if (retNodeStatus != BTNodeStatus.RUNNING)
            Reset();
        
        return retNodeStatus;

    }

    public abstract BTNodeStatus OnBehave(BehaviourState state);

    public void Reset()
    {
        OnReset();
    }

    public abstract void OnReset();


}

public abstract class Decorator : BTNode
{
    protected BTNode child;

    public Decorator(BTNode node)
    {
        child = node;
    }
}

public abstract class Leaf : BTNode
{

}

public abstract class Composite : BTNode
{
    protected List<BTNode> children = new List<BTNode>();

    public Composite(BTNode[] nodes)
    {
        children.AddRange(nodes);
    }
}
