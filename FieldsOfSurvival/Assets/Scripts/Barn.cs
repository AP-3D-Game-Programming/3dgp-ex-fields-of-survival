using UnityEngine;
using UnityEngine.Events;

public class Barn : MonoBehaviour
{
    private Health health;

    public UnityEvent OnBarnHealthChanged;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("Barn requires a Health component!");
        }
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(OnBarnDestroyed);
            health.OnDamaged.AddListener(_ => OnBarnHealthChanged?.Invoke());
        }
    }

    private void OnBarnDestroyed()
    {
        GameManager.Instance.GameOver();
        Destroy(gameObject, 2f);
    }

    public bool IsFullyRepaired()
    {
        return health != null && health.GetCurrentHealth() >= health.GetMaxHealth();
    }

    public void RepairToFull()
    {
        if (health == null || health.IsDead()) return;

        int missingHealth = health.GetMaxHealth() - health.GetCurrentHealth();
        if (missingHealth <= 0) return;

        health.Heal(missingHealth);
        OnBarnHealthChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnBarnDestroyed);
        }
    }
}