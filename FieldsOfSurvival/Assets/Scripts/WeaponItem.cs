using UnityEngine;

public class WeaponItem : ToolbarItem
{
    [Header("Weapon Settings")]
    public int maxAmmo = 999;
    public int currentAmmo;
    public float fireRate = 0.1f;
    public int damage = 10;

    [Header("Visual")]
    public GameObject weaponModel; // The 3D model of the weapon

    // initialize so the weapon is immediately ready to fire
    private float lastFireTime = -Mathf.Infinity;

    void Awake()
    {
        // Ensure ammo initialized even if Start/Awake order is weird
        if (currentAmmo <= 0)
            currentAmmo = maxAmmo;

        // Make weapon ready immediately on awake (no initial cooldown)
        lastFireTime = Time.time - fireRate;
    }

    public override void Activate()
    {
        base.Activate();
        // When the weapon becomes active, ensure it's ready to fire immediately
        lastFireTime = Time.time - fireRate;

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
        return currentAmmo > 0 && (Time.time - lastFireTime) >= fireRate;
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
        return $"Gun {currentAmmo}/{maxAmmo}";
    }
}