using UnityEngine;

public class Plant : MonoBehaviour
{
    [SerializeField] private int maxHP = 20;
    private int currentHP;
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    void Start()
    {
        currentHP = maxHP;
        FarmManager.Instance.RegisterPlant(this);
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
        FarmManager.Instance.RemovePlant(this);
        Destroy(gameObject);
    }
}
