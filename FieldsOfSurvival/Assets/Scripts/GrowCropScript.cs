using UnityEngine;

public class GrowCropScript : MonoBehaviour
{
    public float growDuration = 10f;  // seconds to fully grow
    public Vector3 finalScale = new Vector3(1f, 1f, 1f);
    private Vector3 startScale;

    private float growTimer = 0f;
    private bool isGrowing = false;

    void Start()
    {
        startScale = transform.localScale;
        transform.localScale = Vector3.zero; // start invisible/tiny
    }

    void Update()
    {
        if (isGrowing)
        {
            growTimer += Time.deltaTime;
            float t = growTimer / growDuration;

            // Smooth growth
            transform.localScale = Vector3.Lerp(startScale, finalScale, t);

            if (t >= 1f)
            {
                isGrowing = false; // fully grown
            }
        }
    }

    public void StartGrowing()
    {
        isGrowing = true;
    }
}
