using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText; // TMP Text
    [SerializeField] private Image background;

    public void SetItem(ToolbarItem item, bool selected)
    {
        if (item == null)
        {
            iconImage.enabled = false;
            amountText.text = "";
            background.color = Color.gray;
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = item.icon;

        string display = item.GetDisplayText();
        amountText.text = string.IsNullOrEmpty(display) ? "" : display;

        background.color = selected ? Color.white : Color.gray;

        // Debug: check of de juiste text wordt weergegeven
        Debug.Log($"Slot {gameObject.name} updated: {display}");
    }
}
