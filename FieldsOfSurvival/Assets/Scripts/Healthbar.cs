using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private bool hideWhenDead = true;
    [SerializeField] private bool hideWhenFullHealth = false;

    private Camera mainCamera;
    private Transform targetTransform;

    private void Start()
    {
        mainCamera = Camera.main;

        // Auto-find health component if not assigned
        if (health == null)
            health = GetComponentInParent<Health>();

        if (health == null)
        {
            Debug.LogError($"HealthBar on {gameObject.name} could not find a Health component!");
            enabled = false;
            return;
        }

        // Get the transform to follow
        targetTransform = health.transform;

        health.OnDeath.AddListener(OnTargetDeath);

        UpdateHealthBar();
    }

    private void LateUpdate()
    {
        if (health == null || mainCamera == null) return;

        // Position above target
        transform.position = targetTransform.position + offset;

        // Always face camera
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (health == null || fillImage == null) return;

        float healthPercent = health.GetHealthPercent();
        fillImage.fillAmount = healthPercent;

        // Lerp color from red to green
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);

        // Hide/show logic
        if (hideWhenFullHealth && healthPercent >= 1f)
        {
            canvas.enabled = false;
        }
        else if (!health.IsDead())
        {
            canvas.enabled = true;
        }
    }

    private void OnTargetDeath()
    {
        if (hideWhenDead && canvas != null)
        {
            canvas.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnTargetDeath);
        }
    }
}