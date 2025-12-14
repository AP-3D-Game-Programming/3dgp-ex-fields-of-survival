using UnityEngine;

public class BearTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private int damage = 50;
    [SerializeField] private bool oneTimeUse = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if already triggered (for one-time use traps)
        if (oneTimeUse && hasTriggered)
            return;

        // Check if the collider belongs to an enemy
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            // Try to find enemy component in the collider itself
            enemy = other.GetComponent<Enemy>();

            if (enemy == null)
                return;
        }

        if (enemy.IsDead())
            return;

        TriggerTrap(enemy);
    }

    private void TriggerTrap(Enemy enemy)
    {
        hasTriggered = true;

        // Start coroutine with delay
        StartCoroutine(TriggerTrapDelayed(enemy));
    }

    private System.Collections.IEnumerator TriggerTrapDelayed(Enemy enemy)
    {
        // Small delay to let enemy walk over trap
        yield return new WaitForSeconds(0.3f);

        // TODO: Add animation later

        // Deal damage to enemy
        if (enemy != null && !enemy.IsDead())
        {
            enemy.TakeDamage(damage);
        }

        // Optional: Destroy trap after use
        if (oneTimeUse)
        {
            Destroy(gameObject, 2f);
        }
    }

    // Optional: Reset trap (for reusable traps)
    public void ResetTrap()
    {
        hasTriggered = false;
    }
}