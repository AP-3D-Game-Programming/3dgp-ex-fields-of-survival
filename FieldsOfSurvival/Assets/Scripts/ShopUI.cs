using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject root;             
   // [SerializeField] private PlayerResources playerResources;

    [Header("Item prices")]
    [SerializeField] private int carrotPrice = 5;

    private void Awake()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Open()
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Optioneel: game pauzeren als shop open is
        // Time.timeScale = 0f;
    }

    public void Close()
    {
        if (root != null)
        {
            root.SetActive(false);
        }

        // Optioneel: game weer verder laten lopen
        // Time.timeScale = 1f;
    }

    public void OnBuyCarrotButton()
    {
        //if (playerResources == null)
        //{
        //    Debug.LogWarning("ShopUI: geen PlayerResources gekoppeld.");
        //    return;
        //}

        //bool success = playerResources.TrySpendMoney(carrotPrice);
        //if (success)
        //{
        //    playerResources.AddCarrotSeed(1);   // Voor nu gewoon +1 seed
        //    Debug.Log("Je hebt een wortelseed gekocht!");
        //}
        //else
        //{
        //    Debug.Log("Niet genoeg geld voor wortel.");
        //}
    }
}
