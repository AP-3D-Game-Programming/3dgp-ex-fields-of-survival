using UnityEngine;
using UnityEngine.UI;

public class ShopSellButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToolbarManager toolbar;
    [SerializeField] private Button button;

    [Header("Sell Settings")]
    [SerializeField] private CropType cropTypeToSell = CropType.Carrot;
    [SerializeField] private int amountToSell = 1;
    [SerializeField] private int payout = 1; // coins die je krijgt per verkoop

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
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
        if (button == null || toolbar == null)
            return;

        button.interactable = toolbar.GetCropCount(cropTypeToSell) >= amountToSell;
    }

    // Koppel dit aan OnClick()
    public void Sell()
    {
        if (toolbar == null)
        {
            Debug.LogWarning("ShopSellButton: ToolbarManager is niet ingesteld!");
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("ShopSellButton: CurrencyManager ontbreekt in de scene!");
            return;
        }

        // 1) probeer te verwijderen
        if (!toolbar.TryRemoveCrop(cropTypeToSell, amountToSell))
        {
            Debug.Log($"Geen {cropTypeToSell} om te verkopen.");
            RefreshInteractable();
            return;
        }

        // 2) coins toevoegen
        CurrencyManager.Instance.AddCoins(payout);

        Debug.Log($"Verkocht: -{amountToSell} {cropTypeToSell}, +{payout} coins.");
        RefreshInteractable();
    }
}
