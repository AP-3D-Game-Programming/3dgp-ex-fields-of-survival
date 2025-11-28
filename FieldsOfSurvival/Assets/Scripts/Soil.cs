using UnityEngine;
using UnityEngine.InputSystem;

public class Soil : MonoBehaviour
{
    public GameObject cropPrefab;
    private GameObject plantedCrop;

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            PlantCrop();
        }

        // Harvest with H
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            TryHarvest();
        }
    }

    public void PlantCrop()
    {
        if (plantedCrop == null)
        {
            plantedCrop = Instantiate(cropPrefab, transform.position, Quaternion.identity);

            var grow = plantedCrop.GetComponent<GrowCropScript>();
            grow.StartGrowing();

            Debug.Log("Crop planted!");
        }
        else
        {
            Debug.Log("Soil already has a crop!");
        }
    }

    private void TryHarvest()
    {
        if (plantedCrop != null)
        {
            var grow = plantedCrop.GetComponent<GrowCropScript>();

            if (grow.isFullyGrown)
            {
                grow.Harvest();
                plantedCrop = null; // soil is empty again
            }
            else
            {
                Debug.Log("Crop is not fully grown yet!");
            }
        }
    }
}
