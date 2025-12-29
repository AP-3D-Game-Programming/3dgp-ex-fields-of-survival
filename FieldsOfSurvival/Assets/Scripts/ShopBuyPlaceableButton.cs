using UnityEngine;
using UnityEngine.UI;

public class ShopBuyPlaceableButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToolbarManager toolbar;
    [SerializeField] private Button button;

    [Header("Buy Settings")]
    [SerializeField] private PlaceableType placeableToBuy = PlaceableType.BearTrap;
    [SerializeField] private int amountToBuy = 1;
    [SerializeField] private int cost = 5;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged += _ => RefreshInteractable();

        RefreshInteractable();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= _ => RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        if (button == null) return;
        bool canAfford = CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(cost);
        button.interactable = canAfford;
    }

    public void Buy()
    {
        if (toolbar == null)
        {
            Debug.LogWarning("ShopBuyPlaceableButton: ToolbarManager niet ingesteld!");
            return;
        }
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("ShopBuyPlaceableButton: CurrencyManager ontbreekt!");
            return;
        }

        if (!CurrencyManager.Instance.TrySpend(cost))
        {
            Debug.Log("Niet genoeg coins!");
            return;
        }

        bool success = toolbar.TryAddPlaceable(placeableToBuy, amountToBuy);

        if (!success)
        {
            CurrencyManager.Instance.AddCoins(cost);
            Debug.Log($"Kon {placeableToBuy} niet kopen (inventory vol). Coins terug.");
        }
        else
        {
            Debug.Log($"Gekocht: {placeableToBuy} x{amountToBuy} voor {cost} coins.");
        }
    }
}
