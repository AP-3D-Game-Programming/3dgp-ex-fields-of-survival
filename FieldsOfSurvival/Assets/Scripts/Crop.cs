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
