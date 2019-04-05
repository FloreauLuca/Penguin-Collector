using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public Enemy me;
    public CellularAutomata mapScript;
    public MapNavigation mapNav;


    BTNode behaviourTree;

    void Start()
    {
        behaviourTree = CreateBehaviourTree();

    }

    void FixedUpdate()
    {
        behaviourTree.Behave(myBehaviourContext);
    }

    BTNode CreateBehaviourTree()
    {
        /*
        Sequence separate = new Sequence(
            new TooCloseToEnemy(0.2f),
            new SetRandomDestination(),
            new Move());

        Sequence moveTowardsEnemy = new Sequence("moveTowardsEnemy",
            new HasEnemy(),
            new SetMoveTargetToEnemy(),
            new Inverter(new CanAttackEnemy()),
            new Inverter(new Succeeder(new Move())));

        Sequence attackEnemy = new Sequence("attackEnemy",
            new HasEnemy(),
            new CanAttackEnemy(),
            new StopMoving(),
            new AttackEnemy());

        Sequence needHeal = new Sequence("needHeal",
            new Inverter(new AmIHurt(15)),
            new AmIHurt(35),
            new FindClosestHeal(30),
            new Move());

        Selector chooseEnemy = new Selector("chooseEnemy",
            new TargetNemesis(),
            new TargetClosestEnemy(30));

        Sequence collectPowerup = new Sequence("collectPowerup",
            new FindClosestPowerup(50),
            new Move());

        Selector fightOrFlight = new Selector("fightOrFlight",
            new Inverter(new Succeeder(chooseEnemy)),
            separate,
            needHeal,
            moveTowardsEnemy,
            attackEnemy);
            */
        Repeater repeater = new Repeater(new StandardMove(this));

        return repeater;
        
    }

}
