using UnityEngine;

public enum CropType
{
    Carrot = 0,
    // Add more crop types later
}

public class Crop : MonoBehaviour
{
    [SerializeField] private int maxHP = 20;
    private int currentHP;

    [Header("Type")]
    [SerializeField] private CropType cropType = CropType.Carrot;
    public CropType Type => cropType;

    [Header("Harvest")]
    [Tooltip("Base units returned to inventory when this crop is harvested (before any bonus drops)")]
    [SerializeField] private int harvestYield = 1;

    [Tooltip("Chance (0.0 - 1.0) to drop +1 extra unit on harvest (e.g. 0.25 = 25% chance)")]
    [SerializeField, Range(0f, 1f)] private float bonusDropChance = 0.25f;

    // Each harvest call will compute the final amount (base + possible bonus).
    public int HarvestYield => Mathf.Max(1, harvestYield) + (Random.value <= bonusDropChance ? 1 : 0);

    public bool IsDead()
    {
        return currentHP <= 0;
    }

    // Allow setting the crop type when instantiated
    public void Initialize(CropType type)
    {
        cropType = type;
        // placeholder for type-specific setup
    }

    void Start()
    {
        currentHP = maxHP;
        if (FarmManager.Instance != null)
            FarmManager.Instance.RegisterPlant(this);
        else
            Debug.LogWarning("FarmManager.Instance is null when registering crop. Ensure FarmManager exists in scene.");
    }

    void OnDestroy()
    {
        // Ensure removal from FarmManager if destroyed by other means
        if (FarmManager.Instance != null)
            FarmManager.Instance.RemovePlant(this);
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead()) return;

        currentHP -= dmg;
        Debug.Log($"{name} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Debug.Log($"{name} Destroyed");
            Die();
        }
    }

    public void Die()
    {
        if (FarmManager.Instance != null)
            FarmManager.Instance.RemovePlant(this);
        Destroy(gameObject);
    }
}
