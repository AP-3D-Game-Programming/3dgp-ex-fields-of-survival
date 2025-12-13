using UnityEngine;
using UnityEngine.UI;

public class ShopBuyButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToolbarManager toolbar;
    [SerializeField] private Button button; // sleep hier de Button component in (of laat leeg, hij pakt 'm zelf)

    [Header("Buy Settings")]
    [SerializeField] private CropType cropTypeToBuy = CropType.Carrot;
    [SerializeField] private int amountToBuy = 1;
    [SerializeField] private int cost = 1;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged += OnCoinsChanged;

        RefreshInteractable();
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int _)
    {
        RefreshInteractable();
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
            Debug.LogWarning("ShopBuyButton: ToolbarManager is niet ingesteld!");
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("ShopBuyButton: CurrencyManager ontbreekt in de scene!");
            return;
        }

        // 1) Eerst check geld
        if (!CurrencyManager.Instance.TrySpend(cost))
        {
            Debug.Log("Niet genoeg coins!");
            return;
        }

        // 2) Dan pas item proberen toe te voegen
        bool success = toolbar.TryAddCrop(cropTypeToBuy, amountToBuy);

        if (success)
        {
            Debug.Log($"Gekocht: -{cost} coins, +{amountToBuy} {cropTypeToBuy}.");
        }
        else
        {
            // 3) Mislukt? Geld terug (belangrijk!)
            CurrencyManager.Instance.AddCoins(cost);
            Debug.Log($"Kon geen {cropTypeToBuy} kopen (inventory/stack vol). Coins teruggegeven.");
        }
    }
}
