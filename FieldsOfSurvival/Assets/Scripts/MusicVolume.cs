using UnityEngine;
using UnityEngine.UI;

public class MusicVolume : MonoBehaviour
{
    public AudioSource musicSource;
    public Slider volumeSlider; // Can be null in non-title scenes

    const string VolumeKey = "MusicVolume";
    private static MusicVolume instance;

    void Awake()
    {
        // Singleton: only one music object should exist
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Restore saved volume
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        musicSource.volume = savedVolume;
    }

    void Start()
    {
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }
}
