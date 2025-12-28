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

    private Health health;
    private Animator animator;

    protected Transform currentTarget; // Generic target (can be crop or barn)
    private IDamageable currentDamageable; // What we're currently damaging
    private bool targetingBarn = false;

    protected virtual void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError($"Enemy {gameObject.name} requires a Health component!");
        }
    }

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();

        // Subscribe to death event
        if (health != null)
        {
            health.OnDeath.AddListener(OnEnemyDeath);
        }

        SetTarget();
    }

    protected virtual void Update()
    {
        if (health.IsDead()) return;

        // Safety if no FarmManager present
        if (FarmManager.Instance == null)
        {
            currentTarget = null;
            currentDamageable = null;
            OnNoTargetAvailable();
            return;
        }

        // HIERARCHY: first crops, then barn
        // Always re-evaluate closest plant to allow retargeting when new crops are planted
        Transform closestCrop = FarmManager.Instance.GetClosestPlant(transform.position);

        if (closestCrop != null)
        {
            // Crops available (PRIORITY)
            targetingBarn = false;
            currentTarget = closestCrop;
            currentDamageable = closestCrop.GetComponent<IDamageable>();

            Vector3 toTarget = currentTarget.position - transform.position;

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
        else
        {
            // No crops available => target the barn
            Transform barn = FarmManager.Instance.GetBarn();

            if (barn == null)
            {
                OnNoTargetAvailable();
                return;
            }

            targetingBarn = true;
            currentTarget = barn;
            currentDamageable = barn.GetComponent<IDamageable>();

            Vector3 toTarget = currentTarget.position - transform.position;

            if (toTarget.magnitude <= barnStopDistance)
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
        {
            Transform closestCrop = FarmManager.Instance.GetClosestPlant(transform.position);

            if (closestCrop != null)
            {
                currentTarget = closestCrop;
                currentDamageable = closestCrop.GetComponent<IDamageable>();
                targetingBarn = false;
            }
            else
            {
                // No crops, target barn
                Transform barn = FarmManager.Instance.GetBarn();
                if (barn != null)
                {
                    currentTarget = barn;
                    currentDamageable = barn.GetComponent<IDamageable>();
                    targetingBarn = true;
                }
            }
        }
    }

    // Animation event - called from attack animation
    private void DealDamageEvent()
    {
        if (currentDamageable != null && !currentDamageable.IsDead())
        {
            currentDamageable.TakeDamage(attackDamage);

            string targetType = targetingBarn ? "barn" : "crop";
            Debug.Log($"{gameObject.name} attacked {targetType} for {attackDamage} damage!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    private void OnEnemyDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable collider immediately so dead enemies don't block shots
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Disable this script so enemy stops moving/attacking
        enabled = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled();
        }

        TutorialManager tutorial = FindObjectOfType<TutorialManager>();
        if (tutorial != null)
        {
            tutorial.OnTutorialEnemyKilled();
        }

        // Destroy after animation
        Destroy(gameObject, 2f);

        Debug.Log($"{gameObject.name} killed!");
    }

    public bool IsDead()
    {
        return health != null && health.IsDead();
    }

    public int GetCurrentHealth()
    {
        return health != null ? health.GetCurrentHealth() : 0;
    }

    public int GetMaxHealth()
    {
        return health != null ? health.GetMaxHealth() : 0;
    }

    public bool IsTargetingBarn()
    {
        return targetingBarn;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnEnemyDeath);
        }
    }
}