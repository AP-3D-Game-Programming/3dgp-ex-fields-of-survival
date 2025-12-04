using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider volumeSlider;

    void Start()
    {
        float value;
        mixer.GetFloat("MasterVolume", out value);
        volumeSlider.value = value;
    }

    public void SetVolume(float v)
    {
        mixer.SetFloat("MasterVolume", v);
    }
}
