using UnityEngine;

public class FakeCrop : MonoBehaviour, ITargetable
{
    [Header("Fake Crop Settings")]
    [SerializeField] private CropType fakeCropType = CropType.Carrot; //doesn't matter which

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError($"FakeCrop {gameObject.name} requires a Health component!");
        }
    }

    private void Start()
    {
        // Register as a targetable so enemies target it
        if (FarmManager.Instance != null)
        {
            FarmManager.Instance.RegisterPlant(this);
        }

        // Subscribe to death event
        if (health != null)
        {
            health.OnDeath.AddListener(OnFakeCropDestroyed);
        }
    }

    private void OnFakeCropDestroyed()
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
            health.OnDeath.RemoveListener(OnFakeCropDestroyed);
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

    // Fake property to match Crop.Type
    public CropType Type => fakeCropType;
}