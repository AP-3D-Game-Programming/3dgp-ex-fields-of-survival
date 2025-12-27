using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Plaats dit script bij elke gap in je basis map hekken.
/// Druk op B om te unlocken en de MapExtension te spawnen.
/// </summary>
public class FieldGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private string gateName = "North Field";
    [SerializeField] private int purchaseCost = 10;
    [SerializeField] private bool isUnlocked = false;

    [Header("Invisible Wall")]
    [SerializeField] private BoxCollider invisibleWallCollider;

    [Header("Map Extension Spawning")]
    [SerializeField] private GameObject mapExtensionPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Interaction")]
    [SerializeField] private KeyCode unlockKey = KeyCode.B;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text promptText;  //TextMeshPro/TextMeshProUGUI

    [Header("Events")]
    public UnityEvent OnGateUnlocked;
    public UnityEvent OnPurchaseFailed;
    public UnityEvent<GameObject> OnExtensionSpawned;

    public bool IsUnlocked => isUnlocked;
    public string GateName => gateName;
    public int PurchaseCost => purchaseCost;

    private bool playerInRange = false;
    private GameObject spawnedExtension;

    private void Start()
    {
        if (invisibleWallCollider != null)
        {
            invisibleWallCollider.enabled = !isUnlocked;
            invisibleWallCollider.isTrigger = false;
        }

        UpdateVisuals();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (isUnlocked && spawnedExtension == null)
            SpawnExtension();
    }

    private void Update()
    {
        if (playerInRange && !isUnlocked && Input.GetKeyDown(unlockKey))
        {
            TryPurchase();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isUnlocked && interactionPrompt != null)
            {
                UpdatePromptText();
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    private void UpdatePromptText()
    {
        if (promptText == null) return;

        int currentCoins = CurrencyManager.Instance != null ? CurrencyManager.Instance.Coins : 0;
        bool canAfford = currentCoins >= purchaseCost;

        promptText.text = $"Press B to Buy\n{purchaseCost} coins ({currentCoins})";
        promptText.color = canAfford ? Color.white : Color.red;
    }

    private void TryPurchase()
    {
        if (isUnlocked) return;

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("CurrencyManager not found! Unlocking for free.");
            Unlock();
            return;
        }

        if (CurrencyManager.Instance.TrySpend(purchaseCost))
        {
            Unlock();
        }
        else
        {
            Debug.Log($"Cannot afford {gateName} - Need {purchaseCost} coins, have {CurrencyManager.Instance.Coins}");
            OnPurchaseFailed?.Invoke();
        }
    }

    public void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;

        if (invisibleWallCollider != null)
            invisibleWallCollider.enabled = false;

        SpawnExtension();
        UpdateVisuals();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        Debug.Log($"{gateName} unlocked!");
        OnGateUnlocked?.Invoke();
    }

    private void SpawnExtension()
    {
        if (mapExtensionPrefab == null)
        {
            Debug.LogWarning($"No MapExtension prefab assigned to {gateName}!");
            return;
        }

        if (spawnedExtension != null) return;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        spawnedExtension = Instantiate(mapExtensionPrefab, spawnPos, spawnRot);
        spawnedExtension.name = $"MapExtension_{gateName}";

        OnExtensionSpawned?.Invoke(spawnedExtension);
    }

    private void UpdateVisuals()
    {
        if (lockedIndicator != null)
            lockedIndicator.SetActive(!isUnlocked);
    }

    private void OnDrawGizmos()
    {
        if (invisibleWallCollider != null)
        {
            Gizmos.color = isUnlocked ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.5f);
            Gizmos.matrix = invisibleWallCollider.transform.localToWorldMatrix;
            Gizmos.DrawCube(invisibleWallCollider.center, invisibleWallCollider.size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnPos, new Vector3(5f, 0.5f, 5f));
        Gizmos.DrawLine(transform.position, spawnPos);
    }
}