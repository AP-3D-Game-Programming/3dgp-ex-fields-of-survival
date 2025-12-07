using UnityEngine;

public class ShopBuyButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToolbarManager toolbar; // sleep hier je ToolbarManager in

    [Header("Buy Settings")]
    [SerializeField] private CropType cropTypeToBuy = CropType.Carrot;
    [SerializeField] private int amountToBuy = 1;

    public void Buy()
    {
        if (toolbar == null)
        {
            Debug.LogWarning("ShopBuyButton: ToolbarManager is niet ingesteld!");
            return;
        }

        bool success = toolbar.TryAddCrop(cropTypeToBuy, amountToBuy);

        if (success)
        {
            Debug.Log($"Gekocht: +{amountToBuy} {cropTypeToBuy} in inventory.");
        }
        else
        {
            Debug.Log($"Kon geen {cropTypeToBuy} kopen: inventory/stack vol.");
        }
    }
}
