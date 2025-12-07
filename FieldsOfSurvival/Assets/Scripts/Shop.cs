using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;

    private void OnMouseDown()
    {
        if (shopUI != null)
        {
            shopUI.Open();
        }
    }
}