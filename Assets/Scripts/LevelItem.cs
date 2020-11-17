using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelItem : MonoBehaviour
{
    public int levelIndex;
    public Text levelText;
    public Image lockSpr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevelText(int _index)
    {
        if (PlayerPrefs.GetInt("Level" + _index.ToString()) == 1)
            levelText.text = _index.ToString();
        else
            levelText.text = "";
    }

    public void SetBgImage(int _index)
    {
        if (PlayerPrefs.GetInt("Level" + _index.ToString()) == 1)
        {
            levelText.enabled = true;
            lockSpr.enabled = false;
        }
        else
        {
            levelText.enabled = false;
            lockSpr.enabled = true;
        }
    }


    public void LoadLevel()
    {
        if(PlayerPrefs.GetInt("Level" + levelIndex.ToString()) == 1)
        {
            PlayerPrefs.SetInt("LastLevelIndex", levelIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Game");
        }

    }
}
