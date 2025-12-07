using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("TEST SHOP");  // vervang met jouw scenenaam
        }
    }
}