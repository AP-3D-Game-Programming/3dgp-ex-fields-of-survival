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

    [Header("Toolbar")]
    [SerializeField] private ToolbarManager toolbarManager;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 5f;
    [Tooltip("Either tag the ground with 'Soil' or set this mask to allow planting on those layers.")]
    [SerializeField] private LayerMask plantableLayer = 0;
    [SerializeField] private float plantClearRadius = 0.5f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 10f;
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

    // Runtime state about current look target
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

        if (toolbarManager == null)
        {
            Debug.LogWarning("ToolbarManager not assigned to PlayerMovement_FP!");
        }
    }

    void Update()
    {
        isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        // Movement (WASD)
        float forward = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
        float strafe = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);

        Vector3 move = (transform.right * strafe + transform.forward * forward).normalized;
        float speed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        cc.Move(move * speed * Time.deltaTime);

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        RotateCamera();

        // Interaction
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

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            lastHit = hit;

            // 1) Check for Enemy
            var enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy == null) enemy = hit.collider.GetComponentInChildren<Enemy>();
            if (enemy != null && hit.distance <= attackRange)
            {
                lookedEnemy = enemy;
                return;
            }

            // 2) Check for Crop
            var crop = hit.collider.GetComponentInParent<Crop>();
            if (crop == null) crop = hit.collider.GetComponentInChildren<Crop>();
            if (crop != null)
            {
                lookedCrop = crop;
                return;
            }

            // 3) Check for Soil component
            var soil = hit.collider.GetComponentInParent<Soil>();
            if (soil != null)
            {
                lookedSoilComponent = soil;
                lookedSoil = true;
                return;
            }

            // 4) Fallback: tag-based soil
            if (hit.collider.CompareTag("Soil"))
            {
                lookedSoil = true;
                return;
            }

            // 5) Layer fallback
            if (plantableLayer != 0 && (plantableLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                lookedSoil = true;
                return;
            }
        }
    }

    private Vector3 GetColliderTopCenter(Collider col, float offset = 0.05f)
    {
        if (col == null) return transform.position + transform.up * offset;
        Bounds b = col.bounds;
        Vector3 topCenter = new Vector3(b.center.x, b.max.y, b.center.z);
        return topCenter + (lastHit.normal * offset);
    }

    private void HandleInput()
    {
        // Attack — only when the weapon is the active toolbar item
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (toolbarManager == null) return;

            // Only allow shooting if the active toolbar item is a WeaponItem
            var currentWeapon = toolbarManager.CurrentItem as WeaponItem;
            if (currentWeapon == null) return; // weapon not active — cannot shoot

            if (!currentWeapon.CanFire()) return;

            // Fire weapon (handles ammo / rate)
            currentWeapon.Fire();

            if (gunAnimator != null)
            {
                gunAnimator.SetTrigger("Shoot");
            }

            // Perform a firing raycast from the camera so shots can hit whatever you're aiming at.
            // This allows firing even when you're not currently pointing at an Enemy (no lookedEnemy check).
            if (cameraTransform != null)
            {
                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
                {
                    var enemy = hit.collider.GetComponentInParent<Enemy>();
                    if (enemy == null) enemy = hit.collider.GetComponentInChildren<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(currentWeapon.damage);
                    }
                }
            }
        }

        // Plant - now uses toolbar system
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (lookedSoil && toolbarManager != null)
            {
                GameObject prefab = toolbarManager.GetCurrentCropPrefab();
                CropType? cropType = toolbarManager.GetCurrentCropType();

                if (prefab == null || !cropType.HasValue)
                {
                    Debug.Log("No crop selected or out of crops!");
                    return;
                }

                // If we have a Soil component, plant there
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

                        // Consume the crop from inventory
                        toolbarManager.TryConsumeCrop();
                    }
                    else
                    {
                        Debug.Log("Soil already occupied.");
                    }

                    return;
                }

                // Fallback: plant at raycast hit point
                Vector3 fallbackSpawn = lastHit.point + lastHit.normal * 0.05f;

                Collider[] overlaps = Physics.OverlapSphere(fallbackSpawn, plantClearRadius);
                foreach (var c in overlaps)
                {
                    if (c.GetComponentInParent<Crop>() != null || c.GetComponentInChildren<Crop>() != null)
                    {
                        Debug.Log("Too close to another crop!");
                        return;
                    }
                }

                GameObject go = Instantiate(prefab, fallbackSpawn, Quaternion.identity);
                Crop cropComp = go.GetComponent<Crop>();
                if (cropComp != null)
                {
                    cropComp.Initialize(cropType.Value);
                }

                var grow = go.GetComponent<GrowCropScript>();
                if (grow != null) grow.StartGrowing();

                // Consume the crop from inventory
                toolbarManager.TryConsumeCrop();
            }
        }

        // Harvest
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            // Prefer harvesting via Soil component
            if (lookedSoilComponent != null)
            {
                // Read crop info before harvesting so we can determine yield
                Crop cropBeforeHarvest = lookedSoilComponent.GetComponentInChildren<Crop>();
                if (cropBeforeHarvest != null)
                {
                    int yield = cropBeforeHarvest.HarvestYield;
                    bool harvested = lookedSoilComponent.TryHarvest();
                    if (harvested && toolbarManager != null)
                    {
                        // Add multiple units based on crop's HarvestYield
                        toolbarManager.TryAddCrop(cropBeforeHarvest.Type, yield);
                    }
                }
                else
                {
                    // No Crop component found; still attempt harvest (fallback behavior)
                    bool harvested = lookedSoilComponent.TryHarvest();
                    if (harvested && toolbarManager != null)
                    {
                        // Unknown type, nothing to add
                    }
                }

                return;
            }

            // If pointing at a crop directly
            if (lookedCrop != null)
            {
                var grow = lookedCrop.GetComponent<GrowCropScript>();
                if (grow == null)
                    grow = lookedCrop.GetComponentInChildren<GrowCropScript>();

                if (grow != null && grow.isFullyGrown)
                {
                    CropType cropType = lookedCrop.Type;
                    int yield = lookedCrop.HarvestYield;

                    grow.Harvest();

                    // Add to inventory using the crop's yield
                    if (toolbarManager != null)
                    {
                        toolbarManager.TryAddCrop(cropType, yield);
                    }
                }
                else if (grow != null)
                {
                    Debug.Log("Crop is not fully grown yet!");
                }
                else
                {
                    // Fallback: just destroy
                    lookedCrop.Die();
                }
            }
        }
    }

    private void OnGUI()
    {
        // Draw crosshair
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

        // Display current toolbar item
        if (toolbarManager != null && toolbarManager.CurrentItem != null)
        {
            GUIStyle itemStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 16,
                normal = { textColor = Color.white }
            };

            string displayText = toolbarManager.CurrentItem.itemName;
            string amount = toolbarManager.CurrentItem.GetDisplayText();
            if (!string.IsNullOrEmpty(amount))
            {
                displayText += $" x{amount}";
            }

            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 40), displayText, itemStyle);
        }
    }
}