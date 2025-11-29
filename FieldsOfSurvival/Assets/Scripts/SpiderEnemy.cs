using UnityEngine;

public class SpiderEnemy : Enemy
{
    protected override void MoveTowardsTarget(Vector3 toTarget)
    {
        // Lock movement to the XZ plane by removing vertical component
        Vector3 horizontal = new Vector3(toTarget.x, 0f, toTarget.z);

        // If zero horizontal direction, skip movement to avoid invalid rotation
        if (horizontal.sqrMagnitude <= Mathf.Epsilon) return;

        base.MoveTowardsTarget(horizontal);
    }
}