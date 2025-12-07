using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopManager shopManager;

    private void OnTriggerEnter(Collider other)
    {
        // Zorg dat je Player de tag "Player" heeft
        if (other.CompareTag("Player"))
        {
            shopManager.OpenShop();
        }
    }
}
