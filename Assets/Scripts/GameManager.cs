using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite bgImage;
    public Text levelText, hintText;
    public AudioSource correctGuessSound, wrongGuessSound;
    public GameObject confettiEffect;

    [Header("Panels")]
    public GameObject cardsPanel;
    public GameObject winPanel;
    public GameObject gameInfoPanel;
    public GameObject noRewardPanel;
    public GameObject hintPanel;
    public GameObject timeoutPanel;
    public GameObject completedPanel;

    [Header("Timer")]
    public Text timerText;
    public Animator timerAnimator;
    public AudioSource timerAudioSource;

    List<Button> cardButtons = new List<Button>();

    Sprite[] allImages;
    List<Sprite> lockedImages = new List<Sprite>();
    List<Sprite> selectedImages = new List<Sprite>();
    
    bool firstGuess, secondGuess;
    int countGuesses, countCorrectGuesses, gameGuesses;
    int firstGuessIndex, secondGuessIndex;
    string firstGuessName, secondGuessName;
    int lastLevelIndex;

    int columns, rows, numOfCards, numOfHints;
    float levelTimeAmount, cellSpace;

    float currentTimer;
    bool canCount = true;
    bool flag1 = true;
    bool flag2 = true;

    private void Awake()
    {
        // current level info
        lastLevelIndex = PlayerPrefs.GetInt("LastLevelIndex", 1);

        // Get the grid variables FORMAT: { NUM OF COLUMNS, NUM OF ROWS, NUM OF CARDS, NUM OF HINTS, GIVEN TIME, CELL SPACE }
        int[] dimensions = GetLevelVariables(lastLevelIndex);
        columns = dimensions[0];
        rows = dimensions[1];
        numOfCards = dimensions[2];
        numOfHints = dimensions[3];
        levelTimeAmount = dimensions[4];
        cellSpace = dimensions[5];

        timerAnimator.enabled = false;
        timerAudioSource.enabled = false;

        // Create cards
        for (int i = 0; i < numOfCards; i++)
        {
            GameObject cardButton = Instantiate(cardPrefab);
            cardButton.name = "" + i;
            cardButton.transform.SetParent(cardsPanel.transform, false);
        }
        // load images
        allImages = Resources.LoadAll<Sprite>("Cards");

    }

    void Start()
    {
        getCards();
        AddListeners();
        FindLockedImages();
        AddImagesToCards();
        Shuffle(selectedImages);
        AssignImages();
        AdjustGridLayout();
        gameGuesses = selectedImages.Count / 2;
        
        levelText.text = "LEVEL " + lastLevelIndex;

        timerText.text = TimeSpan.FromSeconds(levelTimeAmount).ToString(@"mm\:ss");
        currentTimer = levelTimeAmount;
    }

    private void Update()
    {
        // Countdown Timer
        // if not paused and not reached to zero, decrement by 1, update the text
        if (currentTimer >= 0f && canCount)
        {
            currentTimer -= Time.deltaTime;
            timerText.text = TimeSpan.FromSeconds(currentTimer).ToString(@"mm\:ss");

            // Start animation of warning
            if (currentTimer <= 10f && flag1)
            {
                flag1 = false;
                timerAudioSource.enabled = true;
                timerAnimator.enabled = true;
                timerText.color = Color.yellow;
            }
        }
        // if time is out, show timeout panel, play sound
        else if (currentTimer <= 0f && flag2)
        {
            canCount = false;
            flag2 = false;
            timerAnimator.enabled = false;
            timerAudioSource.enabled = false;
            currentTimer = 0f;
            timerText.text = TimeSpan.FromSeconds(currentTimer).ToString(@"mm\:ss");
            timeoutPanel.SetActive(true);
        }
    }

    // find all cards and set background image
    void getCards()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Card");
        for (int i = 0; i < objects.Length; i++)
        {
            cardButtons.Add(objects[i].GetComponent<Button>());
            cardButtons[i].image.sprite = bgImage;
        }
    }
    
    // add the listeners to buttons (cards)
    void AddListeners()
    {
        foreach (Button btn in cardButtons)
        {
            btn.onClick.AddListener(() => CardClick());
        }
    }

    void FindLockedImages()
    {
        for (int i = 0; i < allImages.Length; i++)
        {
            if (PlayerPrefs.GetInt(allImages[i].name) == 0)
                lockedImages.Add(allImages[i]);
        }
    }

    void AddImagesToCards()
    {
        // number of images required from collection
        int count = cardButtons.Count / 2;
        // if there exists enough locked images take from the very beginning
        if (lockedImages.Count >= count)
            selectedImages.AddRange(lockedImages.GetRange(0, count));
        // if locked images are insufficient, take the existing ones and complete remaining from all images
        else
        {
            selectedImages.AddRange(lockedImages);
            int remainder = count - lockedImages.Count;
            for (int i = 0; i < remainder; i++)
            {
                selectedImages.Add(allImages[UnityEngine.Random.Range(0, allImages.Length)]);
            }
        }
        // duplicate the list (to make a copy of the same card)
        for (int i = 0; i < count; i++)
            selectedImages.Add(selectedImages[i]);
    }

    // Shuffles the list
    void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Assign the images to the ButtonImage script
    void AssignImages()
    {
        for (int i = 0; i < selectedImages.Count; i++)
            cardButtons[i].GetComponent<ButtonImage>().assignedImage = selectedImages[i];
    }
    
    // method that will be executed when a card clicked
    public void CardClick()
    {
        if (!firstGuess)
        {
            firstGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
            firstGuessName = selectedImages[firstGuessIndex].name;
            cardButtons[firstGuessIndex].GetComponent<ButtonImage>().flipFinished = false;
            firstGuess = true;
        }
        else if (!secondGuess)
        {
            secondGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
            secondGuessName = selectedImages[secondGuessIndex].name;
            cardButtons[secondGuessIndex].GetComponent<ButtonImage>().flipFinished = false;
            secondGuess = true;

            countGuesses++;
            StartCoroutine(IsCardsMatch());
        }
    }

    IEnumerator IsCardsMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstGuessName == secondGuessName)
        {
            yield return new WaitForSeconds(.5f);

            correctGuessSound.Play();

            cardButtons[firstGuessIndex].GetComponent<Animator>().enabled = true;
            cardButtons[secondGuessIndex].GetComponent<Animator>().enabled = true;

            cardButtons[firstGuessIndex].interactable = false;
            cardButtons[secondGuessIndex].interactable = false;

            countCorrectGuesses++;

            // Is Game Finished ???
            if (countCorrectGuesses == gameGuesses)
            {
                //Debug.Log("Game Finished, it took you " + countGuesses + " guesses");
                correctGuessSound.Stop();

                WinGame();
            }
        }
        else
        {
            yield return new WaitForSeconds(.5f);

            wrongGuessSound.Play();
            cardButtons[firstGuessIndex].GetComponent<ButtonImage>().flipFinished = false;
            cardButtons[secondGuessIndex].GetComponent<ButtonImage>().flipFinished = false;
        }

        yield return new WaitForSeconds(.5f);

        firstGuess = secondGuess = false;
    }

    void WinGame()
    {
        // If last level isn't reached yet, show normal win panel
        if (lastLevelIndex < 75)
            StartCoroutine(WinGameCoroutine());
        // if last level is finished, then show the game completed panel
        else
        {
            confettiEffect.SetActive(true);
            completedPanel.SetActive(true);
            PlayerPrefs.SetInt("LastLevelIndex", 1);
            // unlock the images to show in collection
            for (int i = 0; i < selectedImages.Count; i++)
                PlayerPrefs.SetInt(selectedImages[i].name, 1);
            PlayerPrefs.Save();
        }
    }

    IEnumerator WinGameCoroutine()
    {
        // reset timer variables
        canCount = false;
        timerAnimator.enabled = false;
        timerAudioSource.enabled = false;

        // show confetti
        confettiEffect.SetActive(true);

        // show win panel and hide gameinfo panel
        winPanel.SetActive(true);
        gameInfoPanel.SetActive(false);

        // increment level index, set player pref's
        lastLevelIndex++;
        PlayerPrefs.SetInt("LastLevelIndex", lastLevelIndex);
        PlayerPrefs.SetInt("Level" + lastLevelIndex.ToString(), 1);
        // unlock the images to show in collection
        for (int i = 0; i < selectedImages.Count; i++)
            PlayerPrefs.SetInt(selectedImages[i].name, 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(2f);
        // Show Interstitial Ad
        AdsManager.Instance.ShowInterstitialAd();
        yield return new WaitForSeconds(0.5f);

        // show Continue and Home buttons
        foreach (Transform child in winPanel.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    // Adjusts the grid layout wrt the number of cards
    void AdjustGridLayout()
    {
        // set the number fix column count
        cardsPanel.GetComponent<GridLayoutGroup>().constraintCount = columns;
        cardsPanel.GetComponent<GridLayoutGroup>().spacing = new Vector2(cellSpace, cellSpace);
        // Get the width and height of the panel/canvas
        float gridWidth = cardsPanel.GetComponent<RectTransform>().rect.width;
        float gridHeight = cardsPanel.GetComponent<RectTransform>().rect.height;
        // calculate the total horizontal/vertical space (paddings + space between adjacent buttons)
        float horSpaces = cardsPanel.GetComponent<GridLayoutGroup>().padding.horizontal + cellSpace * (columns - 1);
        float verSpaces = cardsPanel.GetComponent<GridLayoutGroup>().padding.vertical + cellSpace * (rows - 1);
        // calculate each button's available width / height
        float horCellSize = (gridWidth - horSpaces) / columns;
        float verCellSize = (gridHeight - verSpaces) / rows;
        // set the minimum
        float cellSize = Mathf.Min(horCellSize, verCellSize, 350f);
        cardsPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
    }

    // FORMAT: { NUM OF COLUMNS, NUM OF ROWS, NUM OF CARDS, NUM OF HINTS, GIVEN TIME, CELL SPACE }
    int[] GetLevelVariables(int levelIndex) 
    {
        if (levelIndex <= 2)
            return new int[] { 2, 2, 4, 1, 30, 100 };
        else if (levelIndex >= 3 && levelIndex <= 5)
            return new int[] { 2, 3, 6, 1, 45, 80 };
        else if (levelIndex >= 6 && levelIndex <= 8)
            return new int[] { 2, 4, 8, 1, 60, 60 };
        else if (levelIndex >= 9 && levelIndex <= 12)
            return new int[] { 3, 4, 10, 1, 75, 50 };
        else if (levelIndex >= 13 && levelIndex <= 16)
            return new int[] { 3, 4, 12, 1, 90, 50 };
        else if (levelIndex >= 17 && levelIndex <= 21)
            return new int[] { 3, 5, 14, 1, 105, 40 };
        else if (levelIndex >= 22 && levelIndex <= 26)
            return new int[] { 3, 6, 16, 1, 120, 30 };
        else if (levelIndex >= 27 && levelIndex <= 32)
            return new int[] { 4, 5, 18, 1, 140, 30 };
        else if (levelIndex >= 33 && levelIndex <= 38)
            return new int[] { 4, 5, 20, 2, 160, 20 };
        else if (levelIndex >= 39 && levelIndex <= 44)
            return new int[] { 4, 6, 22, 2, 180, 15 };
        else if (levelIndex >= 45 && levelIndex <= 50)
            return new int[] { 4, 6, 24, 2, 200, 15 };
        else if (levelIndex >= 51 && levelIndex <= 55)
            return new int[] { 5, 6, 26, 2, 220, 10 };
        else if (levelIndex >= 56 && levelIndex <= 60)
            return new int[] { 5, 6, 28, 2, 240, 10 };
        else if (levelIndex >= 61 && levelIndex <= 65)
            return new int[] { 5, 6, 30, 2, 260, 10 };
        else if (levelIndex >= 66 && levelIndex <= 70)
            return new int[] { 5, 7, 32, 2, 280, 10 };
        else if (levelIndex >= 71 && levelIndex <= 75)
            return new int[] { 5, 7, 34, 2, 300, 10 };
        else
            return new int[] { 4, 4, 16, 1, 180, 40 };
    }

    public void ShowHintPanel()
    {
        canCount = false;
        hintPanel.SetActive(true);
        hintText.text = numOfHints * 2 + " CARDS \n WILL BE OPENED";
    }

    public void ShowHint()
    {
        StartCoroutine(HintCoroutine());
    }

    IEnumerator HintCoroutine()
    {
        for (int i = 1; i <= numOfHints; i++)
        {
            yield return new WaitForSeconds(0.75f);

            for (int j = 0; j < cardButtons.Count; j++)
            {
                // Find the first card that is not opened yet
                if (cardButtons[j].interactable)
                {
                    // Find the other image of the pair
                    for (int k = j + 1; k < cardButtons.Count; k++)
                    {
                        // after two same card is found, apply the correct guess process
                        if (selectedImages[j].name == selectedImages[k].name)
                        {
                            cardButtons[j].GetComponent<ButtonImage>().flipFinished = false;
                            cardButtons[k].GetComponent<ButtonImage>().flipFinished = false;

                            yield return new WaitForSeconds(1f);

                            correctGuessSound.Play();

                            cardButtons[j].GetComponent<Animator>().enabled = true;
                            cardButtons[k].GetComponent<Animator>().enabled = true;

                            cardButtons[j].interactable = false;
                            cardButtons[k].interactable = false;

                            countCorrectGuesses++;

                            // Is Game Finished ???
                            if (countCorrectGuesses == gameGuesses)
                            {
                                yield return new WaitForSeconds(.75f);
                                WinGame();
                            }

                            break;
                        }
                    }
                    break;
                }
            }
        }
        canCount = true;
    }

    // Adds extra time to the timer (after rewarded video is watched)
    public void AddExtraTime()
    {
        timeoutPanel.SetActive(false);
        currentTimer = levelTimeAmount;
        canCount = true;
        flag1 = true;
        flag2 = true;

        timerText.transform.parent.localScale = Vector3.one;
        timerText.text = TimeSpan.FromSeconds(currentTimer).ToString(@"mm\:ss");
        timerText.color = Color.white;
    }
}
