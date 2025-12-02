using UnityEngine;

public abstract class ToolbarItem : MonoBehaviour
{
    public Sprite icon;
    public string itemName;

    public virtual void Activate()
    {
        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public abstract void Use();

    // Optional: return display text for UI (ammo count, stack size, etc.)
    public virtual string GetDisplayText()
    {
        return string.Empty;
    }
}