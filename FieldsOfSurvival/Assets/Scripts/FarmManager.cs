using System.Collections.Generic;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance;

    private List<Crop> plants = new List<Crop>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple FarmManager instances detected. Destroying duplicate.");
            Destroy(this);
        }
    }

    public void RegisterPlant(Crop plant)
    {
        if (plant == null) return;
        if (!plants.Contains(plant))
            plants.Add(plant);
    }

    public void RemovePlant(Crop plant)
    {
        if (plant == null) return;
        if (plants.Contains(plant))
            plants.Remove(plant);
    }

    public Crop GetClosestPlant(Vector3 referencePosition)
    {
        Crop closestPlant = null;
        float closestDistance = float.MaxValue;

        for (int i = plants.Count - 1; i >= 0; i--)
        {
            var plant = plants[i];
            if (plant == null)
            {
                plants.RemoveAt(i);
                continue;
            }

            if (plant.IsDead()) continue;

            float distanceToPlant = Vector3.Distance(referencePosition, plant.transform.position);
            if (distanceToPlant < closestDistance)
            {
                closestPlant = plant;
                closestDistance = distanceToPlant;
            }
        }

        return closestPlant;
    }
}
