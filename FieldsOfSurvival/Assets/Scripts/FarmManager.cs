using System.Collections.Generic;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance;

    private List<Plant> plants = new List<Plant>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterPlant(Plant plant)
    {
        if (!plants.Contains(plant))
            plants.Add(plant);
    }

    public void RemovePlant(Plant plant)
    {
        if (plants.Contains(plant))
            plants.Remove(plant);
    }

    public Plant GetClosestPlant(Vector3 referencePosition)
    {
        Plant closestPlant = null;
        float closestDistance = float.MaxValue;

        foreach (var plant in plants)
        {
            if (plant == null || plant.IsDead()) continue;

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
