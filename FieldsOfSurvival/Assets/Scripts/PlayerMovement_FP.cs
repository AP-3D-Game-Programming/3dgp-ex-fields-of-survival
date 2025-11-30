using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement_FP : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    public Transform cameraTransform;
    public float lookSpeed = 0.1f;

    [Header("Interaction")]
    [Tooltip("Assign crop prefabs (index 0 should be Carrot for now). Prefab root should have a Crop component.")]
    public List<GameObject> cropPrefabs = new List<GameObject>();
    [Tooltip("Index in cropPrefabs to plant when pressing F")]
    public int selectedCropIndex = 0;
    [SerializeField] private float interactRange = 5f;
    [Tooltip("Either tag the ground with 'Soil' or set this mask to allow planting on those layers. Leave empty (0) to disable layer fallback.")]
    [SerializeField] private LayerMask plantableLayer = 0; // default: no fallback
    [SerializeField] private float plantClearRadius = 0.5f; // prevents overlapping crops

    [Header("Combat")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Animator gunAnimator;

    [Header("Crosshair")]
    [SerializeField] private int crosshairSize = 18;
    [SerializeField] private Color crosshairDefault = Color.white;
    [SerializeField] private Color crosshairTarget = Color.green;
    [SerializeField] private Color crosshairEnemy = Color.red;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    private float xRotation = 0f;

    // runtime state about current look target
    private Crop lookedCrop;
    private bool lookedSoil;
    private RaycastHit lastHit;
    private Soil lookedSoilComponent;
    private Enemy lookedEnemy;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Movement (AZERTY)
        float forward = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
        float strafe = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);

        Vector3 move = transform.right * strafe + transform.forward * forward;
        float speed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        cc.Move(move * speed * Time.deltaTime);

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        RotateCamera();

        // Interaction (plant / harvest)
        UpdateLookTarget();
        HandleInput();
    }

    void RotateCamera()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * lookSpeed;
        float mouseY = Mouse.current.delta.y.ReadValue() * lookSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraTransform != null) cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void UpdateLookTarget()
    {
        lookedCrop = null;
        lookedSoil = false;
        lookedSoilComponent = null;
        lookedEnemy = null;

        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Max(interactRange, attackRange));

        if (hits == null || hits.Length == 0)
            return;

        // sort hits by distance so we consider nearest first
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            lastHit = hit;

            // 1) Check for Enemy (highest priority for combat)
            var enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy == null) enemy = hit.collider.GetComponentInChildren<Enemy>();
            if (enemy != null && hit.distance <= attackRange)
            {
                lookedEnemy = enemy;
                return;
            }

            // 2) check for a Crop (parent then children)
            var crop = hit.collider.GetComponentInParent<Crop>();
            if (crop == null) crop = hit.collider.GetComponentInChildren<Crop>();
            if (crop != null)
            {
                lookedCrop = crop;
                return;
            }

            // 3) check for a Soil component on this hit (preferred)
            var soil = hit.collider.GetComponentInParent<Soil>();
            if (soil != null)
            {
                lookedSoilComponent = soil;
                lookedSoil = true;
                return;
            }

            // 4) fallback: tag-based soil
            if (hit.collider.CompareTag("Soil"))
            {
                lookedSoil = true;
                return;
            }

            // 5) layer fallback (only if user explicitly set plantableLayer)
            if (plantableLayer != 0 && (plantableLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                lookedSoil = true;
                return;
            }

            // otherwise keep iterating to the next hit
        }
    }

    // Compute top-center of the specific collider that was hit.
    private Vector3 GetColliderTopCenter(Collider col, float offset = 0.05f)
    {
        if (col == null) return transform.position + transform.up * offset;
        Bounds b = col.bounds;
        Vector3 topCenter = new Vector3(b.center.x, b.max.y, b.center.z);
        return topCenter + (lastHit.normal * offset);
    }

    private void HandleInput()
    {
        //attack
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(lookedEnemy != null)
            {
                lookedEnemy.TakeDamage(attackDamage);

                // Trigger gun shoot animation
                if (gunAnimator != null)
                {
                    gunAnimator.SetTrigger("Shoot");
                }
            }

        }

        // Plant
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (lookedSoil)
            {
                if (selectedCropIndex < 0 || selectedCropIndex >= cropPrefabs.Count)
                {
                    Debug.LogWarning("Selected crop index invalid or no prefab assigned.");
                    return;
                }

                GameObject prefab = cropPrefabs[selectedCropIndex];
                if (prefab == null)
                {
                    Debug.LogWarning("Crop prefab is null at selected index.");
                    return;
                }

                // If we found a Soil component, compute the top-center of the specific hit collider and plant there
                if (lookedSoilComponent != null)
                {
                    if (!lookedSoilComponent.HasCrop())
                    {
                        Vector3 spawnPos;
                        if (lastHit.collider != null)
                        {
                            spawnPos = GetColliderTopCenter(lastHit.collider, 0.05f);
                        }
                        else
                        {
                            spawnPos = lookedSoilComponent.GetPlantCenterPosition(0.05f);
                        }

                        lookedSoilComponent.PlantCrop(prefab, spawnPos);
                    }
                    else
                    {
                        Debug.Log("Soil already occupied.");
                    }

                    return;
                }

                // Fallback: if we don't have a Soil component, plant at the raycast hit point (preserve spacing check)
                Vector3 fallbackSpawn = lastHit.point + lastHit.normal * 0.05f;

                Collider[] overlaps = Physics.OverlapSphere(fallbackSpawn, plantClearRadius);
                foreach (var c in overlaps)
                {
                    if (c.GetComponentInParent<Crop>() != null || c.GetComponentInChildren<Crop>() != null)
                    {
                        // there's already a crop closeby
                        return;
                    }
                }

                GameObject go = Instantiate(prefab, fallbackSpawn, Quaternion.identity);
                Crop cropComp = go.GetComponent<Crop>();
                if (cropComp != null)
                {
                    int idx = Mathf.Clamp(selectedCropIndex, 0, System.Enum.GetValues(typeof(CropType)).Length - 1);
                    cropComp.Initialize((CropType)idx);
                }

                // Start growth if prefab contains GrowCropScript
                var grow = go.GetComponent<GrowCropScript>();
                if (grow != null) grow.StartGrowing();
            }
        }

        // Harvest
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            // Prefer harvesting via Soil if pointing at soil with a Soil component
            if (lookedSoilComponent != null)
            {
                bool harvested = lookedSoilComponent.TryHarvest();
                if (harvested) return;
            }

            // If pointing at a crop directly, try to harvest via GrowCropScript (respects growth state)
            if (lookedCrop != null)
            {
                // Ensure we get the GrowCropScript from the Crop GameObject (parent/children)
                var grow = lookedCrop.GetComponent<GrowCropScript>();
                if (grow == null)
                    grow = lookedCrop.GetComponentInChildren<GrowCropScript>();

                if (grow != null)
                {
                    grow.Harvest();
                }
                else
                {
                    // fallback: destroy / die
                    lookedCrop.Die();
                }
            }
        }
    }

    private void OnGUI()
    {
        // draw simple centered crosshair; change color when looking at a plantable target or a crop
        Color prevColor = GUI.color;
        if (lookedEnemy != null)
            GUI.color = crosshairEnemy;
        else if (lookedCrop != null || lookedSoil)
            GUI.color = crosshairTarget;
        else
            GUI.color = crosshairDefault;

        float x = (Screen.width - crosshairSize) / 2f;
        float y = (Screen.height - crosshairSize) / 2f;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = crosshairSize,
            normal = { textColor = GUI.color }
        };

        GUI.Label(new Rect(x - crosshairSize * 0.25f, y - crosshairSize * 0.5f, crosshairSize * 2f, crosshairSize), "+", style);

        GUI.color = prevColor;
    }
}
