using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonImage : MonoBehaviour
{
    public Sprite assignedImage;
    public Sprite defaultImage;

    private RectTransform rt;
    private bool sideC;

    [HideInInspector] public bool flipFinished;


    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        rt.eulerAngles = new Vector3(0, 0, 0);

        sideC = true;
        flipFinished = true;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (!flipFinished)
            FlipCard();
    }

    void FlipCard()
    {
        // Speed of card rotation // opening card
        if (sideC)
            rt.eulerAngles = new Vector3(rt.eulerAngles.x, rt.eulerAngles.y + 12, rt.eulerAngles.z);
        // Speed of card rotation  // closing card
        if (!sideC)
            rt.eulerAngles = new Vector3(rt.eulerAngles.x, rt.eulerAngles.y - 4, rt.eulerAngles.z);

        if (rt.eulerAngles.y >= 178f)
        {
            if (sideC)
                flipFinished = true; //CHECK END FLIP
            sideC = false;
        }

        if (rt.eulerAngles.y <= 0)
        {
            if (!sideC)
                flipFinished = true; //CHECK END FLIP
            sideC = true;
        }

        if (rt.eulerAngles.y >= 90)
            GetComponent<Button>().image.sprite = assignedImage;
        else
            GetComponent<Button>().image.sprite = defaultImage;
    }
}
