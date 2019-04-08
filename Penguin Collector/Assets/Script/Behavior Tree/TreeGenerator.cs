using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Context : BehaviourState
{
    public Enemy me;
    public CellularAutomata mapScript;
    public MapNavigation mapNav;

    public Context(Enemy me, CellularAutomata mapScript, MapNavigation mapNav)
    {
        this.me = me;
        this.mapScript = mapScript;
        this.mapNav = mapNav;
    }
}


public class TreeGenerator : MonoBehaviour
{
    private float timer = 0;
    public BTNode behaviourTree;

    public Context behaviourState;
   
    void FixedUpdate()
    {
        if (timer >= 0)
        {
            if (behaviourTree != null)
            {
                behaviourTree.Behave(behaviourState);
            }
            else
            {
                Debug.Log("behavior not init");
            }

            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
        
    }

    public BTNode CreateWalrusBehaviourTree()
    {
        Sequence chaseThePlayer = new Sequence(
            new isInView(),
            new FollowPlayer());

        Sequence tooFarFromHome = new Sequence(
            new Inverter(new isInNextRoom()),
            new Inverter(new isInRoom()),
            new GoBackRoom());

        Selector fightOrStandard = new Selector(
            tooFarFromHome,
            chaseThePlayer,
            new StandardMove());

        Repeater repeater = new Repeater(fightOrStandard);

        return repeater;

    }

    public BTNode CreateBearBehaviourTree(Penguin connectedPenguin)
    {
        
        Sequence protect = new Sequence(
            new Inverter(new isInView()),
            new isPenguin(connectedPenguin),
            new Inverter(new isInRoom()),
            new GoBackRoom());

        Selector penguinOrInView = new Selector(
            new Inverter(new isPenguin(connectedPenguin)),
            new isInView());

        Sequence chaseOrNot= new Sequence(
            penguinOrInView,
            new FollowPlayer());

        Selector fightOrProtected = new Selector(
            protect,
            chaseOrNot,
            new StandardMove());
            
        Repeater repeater = new Repeater(fightOrProtected);

        return repeater;

    }

}
