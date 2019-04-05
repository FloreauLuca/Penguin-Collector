using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isPenguin : Leaf
{
    
    private TreeGenerator context;
    private Penguin penguin;
    public isPenguin(TreeGenerator context, Penguin penguin)
    {
        this.context = context;
        this.penguin = penguin;
    }

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        if (penguin.Connected)
            return BTNodeStatus.FAILURE;
        else
            return BTNodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
