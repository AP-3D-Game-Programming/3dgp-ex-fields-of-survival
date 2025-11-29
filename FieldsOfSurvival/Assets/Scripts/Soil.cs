using UnityEngine;

public class Soil : MonoBehaviour
{
    public GameObject cropPrefab;
    private GameObject plantedCrop;

    // Plant at soil center/top by default
    public bool PlantCrop(GameObject prefab)
    {
        Vector3 spawnPos = GetPlantCenterPosition(0.05f);
        return PlantCrop(prefab, spawnPos);
    }

    // Old overload: plant at explicit world position (use this when planting via raycast fallback)
    public bool PlantCrop(GameObject prefab, Vector3 spawnPos)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Soil.PlantCrop called with null prefab.");
            return false;
        }

        if (plantedCrop == null)
        {
            plantedCrop = Instantiate(prefab, spawnPos, Quaternion.identity);
            // Parent to soil so it stays associated
            plantedCrop.transform.SetParent(transform, true);

            var grow = plantedCrop.GetComponent<GrowCropScript>();
            if (grow != null)
            {
                grow.StartGrowing();
            }

            Debug.Log("Crop planted via Soil at surface!");
            return true;
        }
        else
        {
            Debug.Log("Soil already has a crop!");
            return false;
        }
    }

    // Returns the top-center world position of the soil collider (with optional upward offset).
    // Falls back to transform.position + up*offset if no collider found.
    public Vector3 GetPlantCenterPosition(float offset = 0.05f)
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Bounds b = col.bounds;
            Vector3 topCenter = new Vector3(b.center.x, b.max.y, b.center.z);
            return topCenter + transform.up * offset;
        }

        // No collider: use transform position slightly up
        return transform.position + transform.up * offset;
    }

    // Try to harvest the planted crop (used by player interaction)
    // Returns true if harvested (crop removed)
    public bool TryHarvest()
    {
        if (plantedCrop != null)
        {
            var grow = plantedCrop.GetComponentInChildren<GrowCropScript>();
            if (grow != null)
            {
                if (grow.isFullyGrown)
                {
                    grow.Harvest();
                    plantedCrop = null; // soil is empty again
                    return true;
                }
                else
                {
                    Debug.Log("Crop is not fully grown yet!");
                    return false;
                }
            }
            else
            {
                // If there's no GrowCropScript, fall back to destroying the crop
                var cropComp = plantedCrop.GetComponentInChildren<Crop>();
                if (cropComp != null)
                    cropComp.Die();
                else
                    Destroy(plantedCrop);

                plantedCrop = null;
                return true;
            }
        }

        return false;
    }

    // Helper: if other systems need to query if soil has a planted crop
    public bool HasCrop()
    {
        return plantedCrop != null;
    }
}
