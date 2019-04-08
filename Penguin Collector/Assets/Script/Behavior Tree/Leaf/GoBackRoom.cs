using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoBackRoom : Leaf
{

    private Context context;
    

    public override BTNodeStatus OnBehave(BehaviourState state)
    {
        context = (Context)state;
        if (context.me.GoBackRoom())
        {
            return BTNodeStatus.SUCCESS;
        }
        else
        {
            return BTNodeStatus.RUNNING;
        }
    }

    public override void OnReset()
    {
    }
}
