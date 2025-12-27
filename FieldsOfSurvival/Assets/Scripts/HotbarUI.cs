using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private ToolbarManager toolbarManager;
    [SerializeField] private HotbarSlotUI[] slots;

    void Update()
    {
        if (toolbarManager == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            ToolbarItem item = i < toolbarManager.ItemCount
                ? toolbarManager.GetItemAt(i)
                : null;

            bool selected = (i == toolbarManager.CurrentIndex);

            slots[i].SetItem(item, selected);
        }
    }
}
