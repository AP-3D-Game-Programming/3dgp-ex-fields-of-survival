using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopCanvas;           // je Canvas/ShopUI

    [Header("Player")]
    public MonoBehaviour[] playerScripts;   // bv. PlayerMovement, MouseLook, etc.

    private bool shopOpen = false;

    public void OpenShop()
    {
        if (shopOpen) return;

        shopOpen = true;
        shopCanvas.SetActive(true);

        // Cursor tonen & unlocken
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Player "freeze": movement scripts uitzetten
        foreach (var script in playerScripts)
        {
            script.enabled = false;
        }
    }

    public void CloseShop()
    {
        if (!shopOpen) return;

        shopOpen = false;
        shopCanvas.SetActive(false);

        // Cursor verstoppen & locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Player weer laten bewegen
        foreach (var script in playerScripts)
        {
            script.enabled = true;
        }
    }

    // Deze koppel je aan de CLOSE button
    public void CloseShopAndTeleport()
    {
        CloseShop();

       
    }
}
