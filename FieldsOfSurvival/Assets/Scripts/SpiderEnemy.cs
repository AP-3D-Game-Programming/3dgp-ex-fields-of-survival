using UnityEngine;

public class SpiderEnemy : Enemy
{
    protected override void MoveTowardsTarget(Vector3 toTarget)
    {
        //lock Y-axis
        toTarget.y = transform.position.y;
        base.MoveTowardsTarget(toTarget);
    }
}