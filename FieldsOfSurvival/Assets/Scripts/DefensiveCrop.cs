using UnityEngine;

public class DefensiveCrop : MonoBehaviour, ITargetable
{
    [Header("Defensive Crop Settings")]
    [SerializeField] private CropType defensiveCropType = CropType.Potato;
    [SerializeField, Range(0f, 100f)] private float damageReflectionPercentage = 100f;

    private Health health;
    private int lastKnownHealth;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError($"DefensiveCrop {gameObject.name} requires a Health component!");
        }
    }

    private void Start()
    {
        // Register as targetable so enemies target it
        if (FarmManager.Instance != null)
        {
            FarmManager.Instance.RegisterPlant(this);
        }

        if (health != null)
        {
            lastKnownHealth = health.GetCurrentHealth();

            // Subscribe to damage event to reflect damage
            health.OnDamaged.AddListener(OnDamageTaken);
            health.OnDeath.AddListener(OnDefensiveCropDestroyed);
        }
    }

    private void OnDamageTaken(int currentHealth)
    {
        // Calculate the actual damage received in this hit
        int damageReceived = lastKnownHealth - currentHealth;
        lastKnownHealth = currentHealth;

        if (damageReceived <= 0) return;

        // Find which enemy just attacked us
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 5f);

        foreach (var col in nearbyColliders)
        {
            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy == null) enemy = col.GetComponent<Enemy>();

            if (enemy != null && !enemy.IsDead())
            {
                // Calculate reflected damage based on damage received
                int reflectedDamage = Mathf.RoundToInt(damageReceived * (damageReflectionPercentage / 100f));

                // Deal damage back to the enemy
                enemy.TakeDamage(reflectedDamage);

                Debug.Log($"DefensiveCrop took {damageReceived} damage, reflected {reflectedDamage} ({damageReflectionPercentage}%) to {enemy.gameObject.name}!");

                // Only damage the closest/first enemy found
                break;
            }
        }
    }

    private void OnDefensiveCropDestroyed()
    {
        if (FarmManager.Instance != null)
        {
            FarmManager.Instance.RemovePlant(this);
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDestroy()
    {
        if (FarmManager.Instance != null)
        {
            FarmManager.Instance.RemovePlant(this);
        }

        if (health != null)
        {
            health.OnDamaged.RemoveListener(OnDamageTaken);
            health.OnDeath.RemoveListener(OnDefensiveCropDestroyed);
        }
    }

    // ITargetable implementation
    public bool IsDead()
    {
        return health != null && health.IsDead();
    }

    public void TakeDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    public CropType Type => defensiveCropType;
}