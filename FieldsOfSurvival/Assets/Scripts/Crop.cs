using UnityEngine;

public enum CropType
{
    Carrot = 0,
    Potato = 1,
}

public class Crop : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private CropType cropType = CropType.Potato;
    public CropType Type => cropType;

    [Header("Harvest")]
    [Tooltip("Base units returned to inventory when this crop is harvested (before any bonus drops)")]
    [SerializeField] private int harvestYield = 1;

    [Tooltip("Chance (0.0 - 1.0) to drop +1 extra unit on harvest (e.g. 0.25 = 25% chance)")]
    [SerializeField, Range(0f, 1f)] private float bonusDropChance = 0.25f;

    private Health health;

    // Each harvest call will compute the final amount (base + possible bonus).
    public int HarvestYield => Mathf.Max(1, harvestYield) + (Random.value <= bonusDropChance ? 1 : 0);

    public bool IsDead()
    {
        return health != null && health.IsDead();
    }

    // Allow setting the crop type when instantiated
    public void Initialize(CropType type)
    {
        cropType = type;
        // placeholder for type-specific setup
    }

    void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError($"Crop {gameObject.name} requires a Health component!");
        }
    }

    void Start()
    {
        if (FarmManager.Instance != null)
            FarmManager.Instance.RegisterPlant(this);
        else
            Debug.LogWarning("FarmManager.Instance is null when registering crop. Ensure FarmManager exists in scene.");

        // Subscribe to death event
        if (health != null)
        {
            health.OnDeath.AddListener(OnCropDeath);
        }
    }

    void OnDestroy()
    {
        // Ensure removal from FarmManager if destroyed by other means
        if (FarmManager.Instance != null)
            FarmManager.Instance.RemovePlant(this);

        if (health != null)
        {
            health.OnDeath.RemoveListener(OnCropDeath);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (health != null)
        {
            health.TakeDamage(dmg);
        }
    }

    private void OnCropDeath()
    {
        Debug.Log($"{name} Destroyed");
        Die();
    }

    public void Die()
    {
        if (FarmManager.Instance != null)
            FarmManager.Instance.RemovePlant(this);
        Destroy(gameObject);
    }
}