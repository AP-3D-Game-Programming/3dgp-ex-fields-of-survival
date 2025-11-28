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
    protected Plant currentTarget; // Protected: child classes kunnen target checken

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        SetTarget();
    }

    protected virtual void Update()
    {
        // Debug testing
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Death");
        }

        if (currentTarget == null || currentTarget.IsDead())
        {
            SetTarget();
            if (currentTarget == null)
            {
                OnNoTargetAvailable();
                return;
            }
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
        animator.SetBool("Attacking", true);
    }

    protected virtual void OnMovingToTarget()
    {
        animator.SetBool("Attacking", false);
    }

    protected virtual void OnNoTargetAvailable()
    {
        animator.SetBool("Attacking", false);
    }

    private void SetTarget()
    {
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