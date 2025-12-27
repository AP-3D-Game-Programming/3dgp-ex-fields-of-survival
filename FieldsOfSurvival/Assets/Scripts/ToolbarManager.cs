using UnityEngine;
using UnityEngine.InputSystem;

public class ToolbarManager : MonoBehaviour
{
    [SerializeField] private ToolbarItem[] items;
    [SerializeField] private int startingIndex = 0;
    private int currentIndex = 0;

    public ToolbarItem CurrentItem => items.Length > 0 ? items[currentIndex] : null;
    public int CurrentIndex => currentIndex;

    void Awake()
    {
        // Ensure defaults and selection happen before other scripts' Start()
        InitializeDefaults();

        // set starting index (clamped)
        if (items != null && items.Length > 0)
        {
            currentIndex = Mathf.Clamp(startingIndex, 0, items.Length - 1);
            UpdateSelection();
        }
    }

    void Update()
    {
        // Scroll wheel to cycle
        float scroll = Mouse.current.scroll.y.ReadValue();
        if (scroll > 0)
        {
            CycleNext();
        }
        else if (scroll < 0)
        {
            CyclePrevious();
        }

        // Number keys for direct selection (1-9)
        for (int i = 0; i < Mathf.Min(items.Length, 9); i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectItem(i);
            }
        }

        // Use current item with left click (handled in PlayerMovement_FP)
        // Planting with F key (handled in PlayerMovement_FP)
    }

    public void CycleNext()
    {
        if (items.Length == 0) return;
        currentIndex = (currentIndex + 1) % items.Length;
        UpdateSelection();
    }

    public void CyclePrevious()
    {
        if (items.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = items.Length - 1;
        UpdateSelection();
    }

    public void SelectItem(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            currentIndex = index;
            UpdateSelection();
        }
    }

    void UpdateSelection()
    {
        // Deactivate all items
        for (int i = 0; i < items.Length; i++)
        {
            items[i].Deactivate();
        }

        // Activate current item
        if (items.Length > 0)
        {
            items[currentIndex].Activate();
        }
    }

    // Call this when harvesting crops
    public bool TryAddCrop(CropType cropType, int amount = 1)
    {
        if (amount <= 0) return false;

        CropItem firstEmptySlot = null;

        // 1) Try to add to existing stack of same crop type
        foreach (var item in items)
        {
            if (item is CropItem cropItem)
            {
                if (cropItem.cropType == cropType)
                {
                    bool addedAny = cropItem.AddToStack(amount);
                    if (addedAny)
                    {
                        Debug.Log($"Added {amount} {cropType} to inventory. New amount: {cropItem.Amount}");
                        return true;
                    }
                    else
                    {
                        Debug.Log($"No room to add {amount} {cropType}; stack at max ({cropItem.Amount}/{cropItem.maxStackSize})");
                        return false;
                    }
                }

                // keep the first empty stack (Amount == 0) as a fallback
                if (firstEmptySlot == null && cropItem.Amount == 0)
                    firstEmptySlot = cropItem;
            }
        }

        // 2) If no existing stack, use first empty CropItem slot and set its type
        if (firstEmptySlot != null)
        {
            firstEmptySlot.cropType = cropType;
            bool added = firstEmptySlot.AddToStack(amount);
            if (added)
            {
                Debug.Log($"Created new stack and added {amount} {cropType}. New amount: {firstEmptySlot.Amount}");
                return true;
            }
            else
            {
                Debug.Log($"Failed to add to new stack for {cropType}");
                return false;
            }
        }

        Debug.Log($"No room for more {cropType} crops");
        return false;
    }

    // Get the crop prefab from the currently selected item if it's a CropItem
    public GameObject GetCurrentCropPrefab()
    {
        if (CurrentItem is CropItem cropItem && cropItem.Amount > 0)
        {
            return cropItem.cropPrefab;
        }
        return null;
    }

    // Get the crop type from the currently selected item
    public CropType? GetCurrentCropType()
    {
        if (CurrentItem is CropItem cropItem && cropItem.Amount > 0)
        {
            return cropItem.cropType;
        }
        return null;
    }

    // Consume one crop from current item when planting
    public bool TryConsumeCrop()
    {
        if (CurrentItem is CropItem cropItem && cropItem.Amount > 0)
        {
            cropItem.ConsumeSingleItem();
            return true;
        }
        return false;
    }

    // ----------------- Defaults (no persistence) -----------------
    // Ensure weapons start with full magazines and carrot slots have at least 1.
    private void InitializeDefaults()
    {
        if (items == null || items.Length == 0) return;

        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item is WeaponItem weapon)
            {
                // Ensure full magazine at start
                weapon.currentAmmo = weapon.maxAmmo;
            }
            else if (item is CropItem crop)
            {
                // Ensure at least 1 carrot if this slot is configured for carrots
                if (crop.cropType == CropType.Carrot && crop.Amount < 1)
                {
                    crop.SetAmount(1);
                }
            }
        }
    }

    // Returns the currently selected WeaponItem if any, otherwise the first WeaponItem found in toolbar (so player can shoot without having the weapon selected)
    public WeaponItem GetPreferredWeapon()
    {
        if (items == null || items.Length == 0) return null;

        if (CurrentItem is WeaponItem selectedWeapon)
            return selectedWeapon;

        foreach (var item in items)
        {
            if (item is WeaponItem weapon)
                return weapon;
        }

        return null;
    }

    public ToolbarItem GetItemAt(int index)
    {
        if (items == null) return null;
        if (index < 0 || index >= items.Length) return null;
        return items[index];
    }

    public int ItemCount => items != null ? items.Length : 0;

}