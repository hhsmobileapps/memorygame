using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public GameObject pages;
    public GameObject pagePrefab;
    public Button prevPageButton, nextPageButton;

    public GameObject popupWindow;
    public Image popupImage;
    public GameObject swipeHelp;

    private int cardsInOnePage = 15; // !!!!!! pageprefab deki buton sayısı ile bu sayı aynı olmalı
    private int pageCount; // number of pages
    private Sprite[] allCards;
    private List<Sprite> unlockedCards;

    private int currentPageIndex;
    private int popupImageIndex;
    
    private void Awake()
    {
        allCards = Resources.LoadAll<Sprite>("Cards");
        unlockedCards = new List<Sprite>();
        if (allCards.Length % cardsInOnePage > 0)
            pageCount = allCards.Length / cardsInOnePage + 1;
        else
            pageCount = allCards.Length / cardsInOnePage;
    }
    
    private void Start()
    {
        CreatePages();
        currentPageIndex = 0;
        prevPageButton.onClick.AddListener(delegate { PreviousPage(); });
        nextPageButton.onClick.AddListener(delegate { NextPage(); });
    }

    void CreatePages()
    {
        for (int i = 0; i < pageCount; i++)
        {
            // instantiate page prefab, find the image buttons
            var newPage = Instantiate(pagePrefab, new Vector3(0, 0, 0), Quaternion.identity, pages.transform);
            Button[] buttons = newPage.GetComponentsInChildren<Button>(true);

            for (int j = 0; j < cardsInOnePage; j++)
            {
                if ((i * cardsInOnePage + j) < allCards.Length)
                {
                    // set the image of each button
                    buttons[j].GetComponent<Image>().sprite = allCards[(i * cardsInOnePage) + j];
                    // check if image is unlocked, if locked turn it to black, if unlocked add click listener
                    string imageName = buttons[j].image.sprite.name;
                    if (PlayerPrefs.GetInt(imageName) == 1)
                    {
                        buttons[j].image.color = Color.white;
                        buttons[j].onClick.AddListener(delegate { ShowPopup(imageName); });
                        unlockedCards.Add(buttons[j].image.sprite);
                    }
                    else
                        buttons[j].image.color = Color.black;
                }
                else
                    Destroy(buttons[j].gameObject);
            }

            // show the first page on start up and hide others
            if (i == 0)
                newPage.SetActive(true);
            else
                newPage.SetActive(false);

            // Set the page number text
            Text pageNumber = newPage.GetComponentInChildren<Text>(true);
            pageNumber.text = "PAGE - " + (i + 1);
        }
    }

    // find the index of clicked card and set the popup image
    void ShowPopup(string imgName)
    {
        int index = Array.FindIndex(allCards, card => card.name == imgName);
        popupImage.sprite = allCards[index];

        popupWindow.SetActive(true);

        // show the swipe hand anim only once
        if(PlayerPrefs.GetInt("Swipe Anim", 0) == 0)
        {
            swipeHelp.SetActive(true);
            PlayerPrefs.SetInt("Swipe Anim", 1);
        }
    }

    void PreviousPage()
    {
        FindActivePage();
        if (currentPageIndex == 0)
            currentPageIndex = pageCount - 1;
        else
            currentPageIndex--;
        ActivateNewPage();
    }

    void NextPage()
    {
        FindActivePage();
        if (currentPageIndex == pageCount - 1)
            currentPageIndex = 0;
        else
            currentPageIndex++;
        ActivateNewPage();
    }

    void FindActivePage()
    {
        for (int i = 0; i < pages.transform.childCount; i++)
        {
            if (pages.transform.GetChild(i).gameObject.activeSelf == true)
            {
                currentPageIndex = i;
                break;
            }
        }
    }

    void ActivateNewPage()
    {
        for (int i = 0; i < pages.transform.childCount; i++)
        {
            if (i == currentPageIndex)
                pages.transform.GetChild(i).gameObject.SetActive(true);
            else
                pages.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    
    // Check for swipe action
    void Update()
    {
        if (popupWindow.activeSelf)
            if (popupImage.sprite != null)
                popupImageIndex = unlockedCards.FindIndex(card => card.name == popupImage.sprite.name);
        
        Swipe();
    }


    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;

    public void Swipe()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y); //save began touch 2d point

        if (Input.GetMouseButtonUp(0))
        {
            //save ended touch 2d point
            secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //create vector from the two points
            currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);
            //normalize the 2d vector
            currentSwipe.Normalize();

            ChangePopupImage();
        }
#endif

        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                firstPressPos = new Vector2(t.position.x, t.position.y); //save began touch 2d point

            if (t.phase == TouchPhase.Ended)
            {
                //save ended touch 2d point
                secondPressPos = new Vector2(t.position.x, t.position.y);
                //create vector from the two points
                currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);
                //normalize the 2d vector
                currentSwipe.Normalize();

                ChangePopupImage();
            }
        }
    }

    void ChangePopupImage()
    {
        //swipe left (go forward)
        if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
        {
            if (popupImageIndex < unlockedCards.Count - 1)
                popupImage.sprite = unlockedCards[popupImageIndex + 1];
            else
                popupImage.sprite = unlockedCards[0];

            swipeHelp.SetActive(false);
        }
        //swipe right (go back)
        if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
        {
            if (popupImageIndex > 0)
                popupImage.sprite = unlockedCards[popupImageIndex - 1];
            else
                popupImage.sprite = unlockedCards[unlockedCards.Count - 1];

            swipeHelp.SetActive(false);
        }
    }
}
