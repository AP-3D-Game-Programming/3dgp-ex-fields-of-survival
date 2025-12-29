using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ToolbarManager : MonoBehaviour
{
    [SerializeField] private ToolbarItem[] items;
    [SerializeField] private int startingIndex = 0;
    private int currentIndex = 0;

    public ToolbarItem CurrentItem => items.Length > 0 ? items[currentIndex] : null;
    public int CurrentIndex => currentIndex;

    public event Action OnToolbarChanged;
    private void NotifyToolbarChanged() => OnToolbarChanged?.Invoke();

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
                        NotifyToolbarChanged();
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
                NotifyToolbarChanged();
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

    // ==================== PLACEABLE ITEM METHODS ====================

    // Get the placeable prefab from the currently selected item if it's a PlaceableItem
    public GameObject GetCurrentPlaceablePrefab()
    {
        if (CurrentItem is PlaceableItem placeableItem && placeableItem.Amount > 0)
        {
            return placeableItem.placeablePrefab;
        }
        return null;
    }

    // Get the placeable type from the currently selected item
    public PlaceableType? GetCurrentPlaceableType()
    {
        if (CurrentItem is PlaceableItem placeableItem && placeableItem.Amount > 0)
        {
            return placeableItem.placeableType;
        }
        return null;
    }

    // Consume one placeable from current item when placing
    public bool TryConsumePlaceable()
    {
        if (CurrentItem is PlaceableItem placeableItem && placeableItem.Amount > 0)
        {
            placeableItem.ConsumeSingleItem();
            return true;
        }
        return false;
    }

    // Try to add placeables to inventory (for pickups, crafting, etc.)
    public bool TryAddPlaceable(PlaceableType placeableType, int amount = 1)
    {
        if (amount <= 0) return false;

        PlaceableItem firstEmptySlot = null;

        // 1) Try to add to existing stack of same placeable type
        foreach (var item in items)
        {
            if (item is PlaceableItem placeableItem)
            {
                if (placeableItem.placeableType == placeableType)
                {
                    bool addedAny = placeableItem.AddToStack(amount);
                    if (addedAny)
                    {
                        Debug.Log($"Added {amount} {placeableType} to inventory. New amount: {placeableItem.Amount}");
                        NotifyToolbarChanged();
                        return true;
                    }
                    else
                    {
                        Debug.Log($"No room to add {amount} {placeableType}; stack at max ({placeableItem.Amount}/{placeableItem.maxStackSize})");
                        return false;
                    }
                }

                // keep the first empty stack (Amount == 0) as a fallback
                if (firstEmptySlot == null && placeableItem.Amount == 0)
                    firstEmptySlot = placeableItem;
            }
        }

        // 2) If no existing stack, use first empty PlaceableItem slot and set its type
        if (firstEmptySlot != null)
        {
            firstEmptySlot.placeableType = placeableType;
            bool added = firstEmptySlot.AddToStack(amount);
            if (added)
            {
                Debug.Log($"Created new stack and added {amount} {placeableType}. New amount: {firstEmptySlot.Amount}");
                NotifyToolbarChanged();
                return true;
            }
            else
            {
                Debug.Log($"Failed to add to new stack for {placeableType}");
                return false;
            }
        }

        Debug.Log($"No room for more {placeableType} items");
        return false;
    }

    // ==================== END PLACEABLE METHODS ====================

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
            // PlaceableItems keep their configured starting amounts
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

    public int GetCropCount(CropType cropType)
    {
        int total = 0;

        foreach (var item in items)
        {
            if (item is CropItem cropItem && cropItem.cropType == cropType)
            {
                total += cropItem.Amount;
            }
        }

        return total;
    }

    public bool TryRemoveCrop(CropType cropType, int amount = 1)
    {
        if (amount <= 0) return true;

        // Eerst check: genoeg totaal?
        if (GetCropCount(cropType) < amount)
            return false;

        int remaining = amount;

        // Verwijder uit stacks (kan over meerdere slots verdeeld zijn)
        foreach (var item in items)
        {
            if (remaining <= 0) break;

            if (item is CropItem cropItem && cropItem.cropType == cropType && cropItem.Amount > 0)
            {
                int take = Mathf.Min(cropItem.Amount, remaining);
                cropItem.SetAmount(cropItem.Amount - take);
                remaining -= take;
            }
        }

        NotifyToolbarChanged();
        return true;
    }

    public ToolbarItem GetItemAt(int index)
    {
        if (items == null) return null;
        if (index < 0 || index >= items.Length) return null;
        return items[index];
    }

    public int ItemCount => items != null ? items.Length : 0;

    public int GetPlaceableCount(PlaceableType type)
    {
        int total = 0;
        foreach (var item in items)
        {
            if (item is PlaceableItem p && p.placeableType == type)
                total += p.Amount;
        }
        return total;
    }

    public bool TryRemovePlaceable(PlaceableType type, int amount = 1)
    {
        if (amount <= 0) return true;

        if (GetPlaceableCount(type) < amount)
            return false;

        int remaining = amount;

        foreach (var item in items)
        {
            if (remaining <= 0) break;

            if (item is PlaceableItem p && p.placeableType == type && p.Amount > 0)
            {
                int take = Mathf.Min(p.Amount, remaining);
                p.SetAmount(p.Amount - take);
                remaining -= take;
            }
        }

        NotifyToolbarChanged();
        return true;
    }
}