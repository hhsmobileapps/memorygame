using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISettings : MonoBehaviour
{

    public string settingName;
    public GameObject on;
    public GameObject off;

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (PlayerPrefs.GetInt(settingName, 1) == 1)
        {
            off.SetActive(false);
            on.SetActive(true);
        }
        else
        {
            on.SetActive(false);
            off.SetActive(true);
        }
    }

    public void ChangeState()
    {
        if (PlayerPrefs.GetInt(settingName, 1) == 1)
            PlayerPrefs.SetInt(settingName, 0);
        else
            PlayerPrefs.SetInt(settingName, 1);

        UpdateUI();
        Settings.instance.UpdateSettings();
    }
}
