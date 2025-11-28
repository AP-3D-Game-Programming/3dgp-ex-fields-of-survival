using UnityEngine;

public class WaspEnemy : Enemy
{
    protected override Quaternion GetLookRotation(Vector3 direction)
    {
        // Wasp model has a 90 degree offset
        return Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f);
    }
}
