using UnityEngine;

public class WeaponItem : ToolbarItem
{
    [Header("Weapon Settings")]
    public int maxAmmo = 30;
    public int currentAmmo;
    public float fireRate = 0.1f;
    public int damage = 10;

    [Header("Visual")]
    public GameObject weaponModel; // The 3D model of the weapon

    private float lastFireTime;

    void Awake()
    {
        // Initialize ammo in Awake so it runs even if the GameObject is inactive at scene load.
        // This prevents currentAmmo from staying 0 when Start() wasn't called.
        if (currentAmmo <= 0)
            currentAmmo = maxAmmo;
    }

    public override void Activate()
    {
        base.Activate();
        if (weaponModel != null)
        {
            weaponModel.SetActive(true);
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }
    }

    public override void Use()
    {
        // Weapon firing is handled in PlayerMovement_FP when looking at enemies
        // This could be used for alternative fire modes or info display
    }

    public bool CanFire()
    {
        return currentAmmo > 0 && Time.time - lastFireTime >= fireRate;
    }

    public void Fire()
    {
        if (!CanFire()) return;

        currentAmmo--;
        lastFireTime = Time.time;

        Debug.Log($"Fired {itemName}! Ammo remaining: {currentAmmo}/{maxAmmo}");
    }

    public void Reload(int ammoAmount)
    {
        currentAmmo = Mathf.Min(currentAmmo + ammoAmount, maxAmmo);
        Debug.Log($"Reloaded! Ammo: {currentAmmo}/{maxAmmo}");
    }

    public override string GetDisplayText()
    {
        return $"{currentAmmo}/{maxAmmo}";
    }
}