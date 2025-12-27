using UnityEngine;

public class CropItem : ToolbarItem
{
    [Header("Stack Settings")]
    public int maxStackSize = 99;
    [SerializeField] private int currentAmount = 1;

    public int Amount
    {
        get => currentAmount;
        private set => currentAmount = Mathf.Clamp(value, 0, maxStackSize);
    }

    [Header("Crop Settings")]
    public GameObject cropPrefab; // Your Crop prefab with Crop + GrowCropScript
    public CropType cropType; // Match the type in the prefab

    public override void Use()
    {
        // The actual planting logic is handled in PlayerMovement_FP
        // This is just here for structure - left click could show info or do nothing
        Debug.Log($"{itemName} selected. Press F to plant. ({currentAmount} remaining)");
    }

    // Consume one item when planting
    public void ConsumeSingleItem()
    {
        if (currentAmount > 0)
        {
            currentAmount--;
            Debug.Log($"Used 1 {itemName}. Remaining: {currentAmount}");

            if (currentAmount <= 0)
            {
                OnDepleted();
            }
        }
    }

    // Add up to `amount` to this stack. Returns true if any units were added.
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
        // Keep the slot but show as empty
    }

    public override string GetDisplayText()
    {
        return currentAmount > 0 ? currentAmount.ToString() : "";
    }
}