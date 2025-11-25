using UnityEngine;

public class Soil : MonoBehaviour
{
    public GameObject carrotPrefab;
    private GameObject plantedCarrot;

    public void PlantCarrot()
    {
        if (plantedCarrot == null)
        {
            // Spawn carrot at the soil position
            plantedCarrot = Instantiate(carrotPrefab, transform.position, Quaternion.identity);

            // Start growth
            plantedCarrot.GetComponent<GrowCropScript>().StartGrowing();
        }
    }
}
