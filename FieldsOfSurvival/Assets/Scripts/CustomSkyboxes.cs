using UnityEngine;

public class Skybox : MonoBehaviour
{
    [Header("Skybox Settings")]
    [SerializeField] private Material daySkybox;

    [Header("Day Settings")]
    [SerializeField] private Color dayTint = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float dayExposure = 1.3f;

    [Header("Night Settings")]
    [SerializeField] private Color nightTint = new Color(0.2f, 0.2f, 0.3f, 1f);
    [SerializeField] private float nightExposure = 0.3f;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 5f;

    private bool isDay = true;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    private Material skyboxMaterial;

    private void Start()
    {
        if (daySkybox != null)
        {
            skyboxMaterial = new Material(daySkybox);
            RenderSettings.skybox = skyboxMaterial;

            // Zet dag instellingen
            SetSkyboxProperties(dayTint, dayExposure);
            DynamicGI.UpdateEnvironment();
        }
    }

    private void Update()
    {
        if (isTransitioning)
        {
            UpdateTransition();
        }
    }

    public void SetDay()
    {
        if (!isDay && !isTransitioning)
        {
            StartTransition(true);
        }
    }

    public void SetNight()
    {
        if (isDay && !isTransitioning)
        {
            StartTransition(false);
        }
    }

    private void StartTransition(bool goToDay)
    {
        isTransitioning = true;
        transitionProgress = 0f;
        isDay = goToDay;
        Debug.Log($"Start transitie naar {(isDay ? "dag" : "nacht")}");
    }

    private void UpdateTransition()
    {
        transitionProgress += Time.deltaTime / transitionDuration;

        if (transitionProgress >= 1f)
        {
            transitionProgress = 1f;
            isTransitioning = false;
            Debug.Log($"Transitie voltooid naar {(isDay ? "dag" : "nacht")}");
        }

        float t = Mathf.SmoothStep(0f, 1f, transitionProgress);

        Color currentTint;
        float currentExposure;

        if (isDay)
        {
            // night to day
            currentTint = Color.Lerp(nightTint, dayTint, t);
            currentExposure = Mathf.Lerp(nightExposure, dayExposure, t);
        }
        else
        {
            // day to night
            currentTint = Color.Lerp(dayTint, nightTint, t);
            currentExposure = Mathf.Lerp(dayExposure, nightExposure, t);
        }

        SetSkyboxProperties(currentTint, currentExposure);
        DynamicGI.UpdateEnvironment();
    }

    private void SetSkyboxProperties(Color tint, float exposure)
    {
        if (skyboxMaterial == null) return;

        if (skyboxMaterial.HasProperty("_Tint"))
            skyboxMaterial.SetColor("_Tint", tint);

        if (skyboxMaterial.HasProperty("_Exposure"))
            skyboxMaterial.SetFloat("_Exposure", exposure);
    }
}