using UnityEngine;

public class GrowCropScript : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growDuration = 10f;
    public Vector3 finalScale = Vector3.one;
    public Vector3 startScale = Vector3.zero;

    private float timer = 0f;
    private bool isGrowing = false;

    [Header("State")]
    public bool isFullyGrown = false;

    void Start()
    {
        transform.localScale = startScale;
    }

    void Update()
    {
        if (isGrowing)
        {
            timer += Time.deltaTime;
            float progress = timer / growDuration;
            transform.localScale = Vector3.Lerp(startScale, finalScale, progress);

            if (progress >= 1f)
            {
                isGrowing = false;
                isFullyGrown = true;
                Debug.Log("Crop fully grown!");
            }
        }
    }

    public void StartGrowing()
    {
        isGrowing = true;
        timer = 0f;
        isFullyGrown = false;
    }

    public void Harvest()
    {
        if (isFullyGrown)
        {
            Debug.Log("Crop harvested!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Crop is not ready yet!");
        }
    }
}
