using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    public static Settings instance;

    public AudioMixer audioMixer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        if (PlayerPrefs.GetInt("Music", 1) == 1)
            audioMixer.SetFloat("Music Volume", -8f);
        else
            audioMixer.SetFloat("Music Volume", -80f);

        if (PlayerPrefs.GetInt("SFX", 1) == 1)
            audioMixer.SetFloat("SFX Volume", 0);
        else
            audioMixer.SetFloat("SFX Volume", -80f);
    }
}
