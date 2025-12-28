using UnityEngine;

public class PlaceableItem : ToolbarItem
{
    [Header("Stack Settings")]
    public int maxStackSize = 99;
    [SerializeField] private int currentAmount = 1;

    public int Amount
    {
        get => currentAmount;
        private set => currentAmount = Mathf.Clamp(value, 0, maxStackSize);
    }

    [Header("Placeable Settings")]
    public GameObject placeablePrefab; // The prefab to instantiate when placing
    public PlaceableType placeableType;

    [Header("Visual")]
    public GameObject heldModel; // Optional: model shown when item is selected

    public override void Activate()
    {
        base.Activate();
        if (heldModel != null)
        {
            heldModel.SetActive(true);
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (heldModel != null)
        {
            heldModel.SetActive(false);
        }
    }

    public override void Use()
    {
        // Actual placement logic is handled in PlayerMovement_FP
        Debug.Log($"{itemName} selected. Press F to place. ({currentAmount} remaining)");
    }

    public void ConsumeSingleItem()
    {
        if (currentAmount > 0)
        {
            currentAmount--;
            Debug.Log($"Placed 1 {itemName}. Remaining: {currentAmount}");

            if (currentAmount <= 0)
            {
                OnDepleted();
            }
        }
    }

    public bool AddToStack(int amount)
    {
        if (amount <= 0) return false;
        if (currentAmount >= maxStackSize) return false;

        int space = maxStackSize - currentAmount;
        int toAdd = Mathf.Min(space, amount);
        currentAmount += toAdd;

        Debug.Log($"Added {toAdd} {itemName} to stack. New amount: {currentAmount}/{maxStackSize}");
        return toAdd > 0;
    }

    public void SetAmount(int amount)
    {
        Amount = amount;
    }

    protected virtual void OnDepleted()
    {
        Debug.Log($"{itemName} depleted!");
    }

    public override string GetDisplayText()
    {
        return currentAmount > 0 ? currentAmount.ToString() : "";
    }
}

public enum PlaceableType
{
    BearTrap,
    FakeCrop,
    DefensiveCrop
}