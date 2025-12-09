using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float rotateSpeed = 5f;
    [SerializeField] protected float stopDistance = 3f;
    [SerializeField] protected float barnStopDistance = 5f;

    [Header("Combat")]
    [SerializeField] protected int attackDamage = 3;
    [SerializeField] protected int maxHealth = 50;

    private int currentHealth;
    private Animator animator;
    protected Crop currentTarget; // Protected: child classes can check target
    protected Transform barnTarget; // New: barn target when no crops available
    private bool isDead = false;
    private bool targetingBarn = false; // Track if we're targeting barn

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        SetTarget();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // Safety if no FarmManager present
        if (FarmManager.Instance == null)
        {
            currentTarget = null;
            barnTarget = null;
            OnNoTargetAvailable();
            return;
        }

        // Always re-evaluate closest plant to allow retargeting when new crops are planted
        Crop closest = FarmManager.Instance.GetClosestPlant(transform.position);

        if (closest == null)
        {
            // No crops available, target the barn
            currentTarget = null;

            if (barnTarget == null)
            {
                barnTarget = FarmManager.Instance.GetBarn();
            }

            if (barnTarget == null)
            {
                OnNoTargetAvailable();
                return;
            }

            targetingBarn = true;
            MoveTowardsBarn();
        }
        else
        {
            // Crops available, target them
            targetingBarn = false;
            barnTarget = null;

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
    }

    private void MoveTowardsBarn()
    {
        if (barnTarget == null) return;

        Vector3 toTarget = barnTarget.position - transform.position;

        if (toTarget.magnitude <= barnStopDistance)
        {
            OnReachedBarn();
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

    protected virtual void OnReachedBarn()
    {
        // Enemy reached the barn - play attack animation
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
        {
            currentTarget = FarmManager.Instance.GetClosestPlant(transform.position);

            // If no crops, get barn
            if (currentTarget == null)
            {
                barnTarget = FarmManager.Instance.GetBarn();
            }
        }
    }

    // Animation event
    private void DealDamageEvent()
    {
        if (targetingBarn && barnTarget != null)
        {
            // Deal damage to barn (you'll need to implement barn health system)
            // For now, just log it
            Debug.Log($"{gameObject.name} attacked the barn for {attackDamage} damage!");

            // Optional: Add barn damage logic here
            // Example: BarnHealth.Instance?.TakeDamage(attackDamage);
        }
        else if (currentTarget != null && !currentTarget.IsDead())
        {
            currentTarget.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        //prevent multiple Die() activations
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable this script so enemy stops moving/attacking
        enabled = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled();
        }

        // Destroy after animation
        Destroy(gameObject, 2f);

        Debug.Log("Enemy killed!");
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsTargetingBarn()
    {
        return targetingBarn;
    }
}