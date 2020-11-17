using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private void Awake()
    {
        AudioManager soundtrack = FindObjectOfType<AudioManager>();
        if (soundtrack != null && soundtrack != this)
        {
            Destroy(soundtrack.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
