using UnityEngine;

public class Barn : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("Barn requires a Health component!");
        }
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(OnBarnDestroyed);
        }
    }

    private void OnBarnDestroyed()
    {
        GameManager.Instance.GameOver();
        Destroy(gameObject, 2f);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnBarnDestroyed);
        }
    }
}