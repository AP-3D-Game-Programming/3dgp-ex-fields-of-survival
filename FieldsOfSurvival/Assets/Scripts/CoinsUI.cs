using TMPro;
using UnityEngine;

public class CoinsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += UpdateText;
            UpdateText(CurrencyManager.Instance.Coins);
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= UpdateText;
    }

    private void UpdateText(int coins)
    {
        if (coinsText != null)
            coinsText.text = $"Coins: {coins}";
    }
}
