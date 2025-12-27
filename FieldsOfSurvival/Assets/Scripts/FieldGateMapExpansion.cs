using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Plaats dit script bij elke gap in je basis map hekken.
/// Druk op B om te unlocken en de MapExtension te spawnen.
/// </summary>
public class FieldGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [SerializeField] private string gateName = "North Field";
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
    [SerializeField] private GameObject interactionPrompt;   // "Press B" UI

    [Header("Events")]
    public UnityEvent OnGateUnlocked;
    public UnityEvent<GameObject> OnExtensionSpawned;

    public bool IsUnlocked => isUnlocked;
    public string GateName => gateName;

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
            Unlock();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isUnlocked && interactionPrompt != null)
                interactionPrompt.SetActive(true);
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