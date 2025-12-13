using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int startingCoins = 5;

    public int Coins { get; private set; }

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Coins = startingCoins;
        OnCoinsChanged?.Invoke(Coins);
    }

    public bool CanAfford(int cost) => Coins >= cost;

    public bool TrySpend(int cost)
    {
        if (cost <= 0) return true;
        if (!CanAfford(cost)) return false;

        Coins -= cost;
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }
}
