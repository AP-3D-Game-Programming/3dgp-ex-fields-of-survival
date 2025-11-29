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

    [Header("Crosshair")]
    [SerializeField] private int crosshairSize = 18;
    [SerializeField] private Color crosshairDefault = Color.white;
    [SerializeField] private Color crosshairTarget = Color.green;

    private CharacterController cc;
    private Vector3 velocity;
    private bool isGrounded;

    private float xRotation = 0f;

    // runtime state about current look target
    private Crop lookedCrop;
    private bool lookedSoil;
    private RaycastHit lastHit;
    private Soil lookedSoilComponent;

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

        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            lastHit = hit;

            // Robust crop detection: check parent first, then children of the hit collider
            lookedCrop = hit.collider.GetComponentInParent<Crop>();
            if (lookedCrop == null)
                lookedCrop = hit.collider.GetComponentInChildren<Crop>();

            if (lookedCrop != null) return;

            // Prefer explicit Soil component (recommended) — this ensures planting only on proper soil
            lookedSoilComponent = hit.collider.GetComponentInParent<Soil>();
            if (lookedSoilComponent != null)
            {
                lookedSoil = true;
                return;
            }

            // Fallback: allow planting when the collider is tagged "Soil"
            if (hit.collider.CompareTag("Soil"))
            {
                lookedSoil = true;
                return;
            }

            // Layer fallback only if user explicitly set plantableLayer in Inspector (non-zero)
            if (plantableLayer != 0 && (plantableLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                lookedSoil = true;
            }
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
        GUI.color = (lookedCrop != null || lookedSoil) ? crosshairTarget : crosshairDefault;

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
