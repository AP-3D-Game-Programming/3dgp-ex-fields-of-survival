using UnityEngine;
using UnityEngine.UI;

public class ShopSellPlaceableButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToolbarManager toolbar;
    [SerializeField] private Button button;

    [Header("Sell Settings")]
    [SerializeField] private PlaceableType placeableToSell = PlaceableType.BearTrap;
    [SerializeField] private int amountToSell = 1;
    [SerializeField] private int payout = 1;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (toolbar != null)
            toolbar.OnToolbarChanged += RefreshInteractable;

        RefreshInteractable();
    }

    private void OnDisable()
    {
        if (toolbar != null)
            toolbar.OnToolbarChanged -= RefreshInteractable;
    }

    private void RefreshInteractable()
    {
        if (button == null || toolbar == null) return;
        button.interactable = toolbar.GetPlaceableCount(placeableToSell) >= amountToSell;
    }

    public void Sell()
    {
        if (toolbar == null || CurrencyManager.Instance == null) return;

        if (!toolbar.TryRemovePlaceable(placeableToSell, amountToSell))
        {
            Debug.Log($"Geen {placeableToSell} meer om te verkopen.");
            RefreshInteractable();
            return;
        }

        CurrencyManager.Instance.AddCoins(payout);
        Debug.Log($"Verkocht: {placeableToSell} x{amountToSell}, +{payout} coins.");
        RefreshInteractable();
    }
}
