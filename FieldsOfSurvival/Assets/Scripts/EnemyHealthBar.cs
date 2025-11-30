using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fillImage;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Auto-find enemy if not assigned
        if (enemy == null)
            enemy = GetComponentInParent<Enemy>();
    }

    private void LateUpdate()
    {
        if (enemy == null || mainCamera == null) return;

        // Position above enemy
        transform.position = enemy.transform.position + offset;

        // Always face camera
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);

        // Update fill amount
        float healthPercent = (float)enemy.GetCurrentHealth() / enemy.GetMaxHealth();
        fillImage.fillAmount = healthPercent;

        // Lerp color from green to red
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);

        // Hide when dead
        if (enemy.IsDead())
        {
            canvas.enabled = false;
        }
    }
}