using UnityEngine;
using UnityEngine.UI;

public class ShopCloseButton : MonoBehaviour
{
    public ShopManager shopManager;

    private void Awake()
    {
        // Pak de Button op hetzelfde GameObject
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnCloseClicked);
        }
        else
        {
            Debug.LogError("Geen Button component gevonden op " + gameObject.name);
        }
    }

    private void OnCloseClicked()
    {
        if (shopManager != null)
        {
            shopManager.CloseShopAndTeleport();
        }
        else
        {
            Debug.LogError("ShopManager niet toegewezen in ShopCloseButton!");
        }
    }
}
