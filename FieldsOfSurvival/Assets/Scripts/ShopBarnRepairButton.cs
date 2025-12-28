using UnityEngine;
using UnityEngine.UI;

public class ShopBarnRepairButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Barn barn;
    [SerializeField] private Button button;

    [Header("Repair Settings")]
    [SerializeField] private int repairCost = 10;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (barn != null)
            barn.OnBarnHealthChanged.AddListener(RefreshInteractable);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged += _ => RefreshInteractable();

        RefreshInteractable();
    }

    private void OnDisable()
    {
        if (barn != null)
            barn.OnBarnHealthChanged.RemoveListener(RefreshInteractable);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= _ => RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        if (button == null || barn == null || CurrencyManager.Instance == null)
            return;

        bool canAfford = CurrencyManager.Instance.CanAfford(repairCost);
        bool needsRepair = !barn.IsFullyRepaired();

        button.interactable = canAfford && needsRepair;
    }

    // Koppel aan OnClick()
    public void RepairBarn()
    {
        if (barn == null || CurrencyManager.Instance == null) return;

        if (barn.IsFullyRepaired())
            return;

        if (!CurrencyManager.Instance.TrySpend(repairCost))
            return;

        barn.RepairToFull();

        Debug.Log($"Barn repaired for {repairCost} coins.");
        RefreshInteractable();
    }
}
