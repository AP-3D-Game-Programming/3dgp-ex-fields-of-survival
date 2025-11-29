using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float rotateSpeed = 5f;
    [SerializeField] protected float stopDistance = 3f;

    [Header("Combat")]
    [SerializeField] protected int attackDamage = 3;

    private Animator animator;
    protected Crop currentTarget; // Protected: child classes can check target

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        SetTarget();
    }

    protected virtual void Update()
    {
        // Safety if no FarmManager present
        if (FarmManager.Instance == null)
        {
            currentTarget = null;
            OnNoTargetAvailable();
            return;
        }

        // Always re-evaluate closest plant to allow retargeting when new crops are planted
        Crop closest = FarmManager.Instance.GetClosestPlant(transform.position);

        if (closest == null)
        {
            currentTarget = null;
            OnNoTargetAvailable();
            return;
        }

        // Switch target if none, dead, or a different (closer) plant exists
        if (currentTarget == null || currentTarget.IsDead() || closest != currentTarget)
        {
            currentTarget = closest;
        }

        if (currentTarget == null)
        {
            OnNoTargetAvailable();
            return;
        }

        Vector3 toTarget = currentTarget.transform.position - transform.position;

        if (toTarget.magnitude <= stopDistance)
        {
            OnReachedTarget();
        }
        else
        {
            OnMovingToTarget();
            MoveTowardsTarget(toTarget);
        }
    }

    protected virtual void MoveTowardsTarget(Vector3 toTarget)
    {
        // Rotate
        Vector3 direction = toTarget.normalized;
        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        Quaternion lookRotation = GetLookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);

        // Move
        transform.position += direction * speed * Time.deltaTime;
    }

    protected virtual Quaternion GetLookRotation(Vector3 direction)
    {
        return Quaternion.LookRotation(direction);
    }

    protected virtual void OnReachedTarget()
    {
        if (animator != null) animator.SetBool("Attacking", true);
    }

    protected virtual void OnMovingToTarget()
    {
        if (animator != null) animator.SetBool("Attacking", false);
    }

    protected virtual void OnNoTargetAvailable()
    {
        if (animator != null) animator.SetBool("Attacking", false);
    }

    private void SetTarget()
    {
        if (FarmManager.Instance != null)
            currentTarget = FarmManager.Instance.GetClosestPlant(transform.position);
    }

    // Animation event
    private void DealDamageEvent()
    {
        if (currentTarget != null && !currentTarget.IsDead())
        {
            currentTarget.TakeDamage(attackDamage);
        }
    }
}