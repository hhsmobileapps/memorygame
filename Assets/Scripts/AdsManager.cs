using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; set; }

    public bool isTestMode;
    public string admobAppId, bannerAdUnitId, interstitialAdUnitId, rewardedAdUnitId;

    [HideInInspector] public int rewardType; // 1: Hint 2: Extra Time

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private int interstitialInterval = 2;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            DestroyImmediate(gameObject);
    }

    void Start()
    {
        // Test IDs
        if (isTestMode)
        {
            bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
            interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
            rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        }

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(admobAppId);

        //Request Ads
        RequestBanner();
        RequestInterstitialAd();
        RequestRewardedAd();
    }


    #region BANNER AD
    private void RequestBanner()
    {
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        bannerView.OnAdLoaded += HandleBannerOnAdLoaded;
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
    }

    private void HandleBannerOnAdLoaded(object sender, EventArgs e)
    {
        bannerView.Show();
    }
    #endregion


    #region INTERSTITIAL AD
    private void RequestInterstitialAd()
    {
        interstitialAd = new InterstitialAd(interstitialAdUnitId);

        interstitialAd.OnAdLoaded += HandleInterstitialLoaded;
        interstitialAd.OnAdOpening += HandleInterstitialOpened;
        interstitialAd.OnAdClosed += HandleInterstitialClosed;
        interstitialAd.OnAdFailedToLoad += HandleInterstitialFailed;

        AdRequest request = new AdRequest.Builder().Build();
        interstitialAd.LoadAd(request);
    }

    // Reset the counter for failed ads
    private void HandleInterstitialLoaded(object sender, EventArgs args)
    {
        PlayerPrefs.SetInt("InterstitialFailed", 0);
    }
    
    // Reset the counter for ad clicks
    private void HandleInterstitialOpened(object sender, EventArgs args)
    {
        PlayerPrefs.SetInt("ClickCounter", 0);
    }

    // Request new interstital ad
    private void HandleInterstitialClosed(object sender, EventArgs args)
    {
        RequestInterstitialAd();
    }

    // Try to load new ad if ad is failed (limit the trial with 3 times)
    private void HandleInterstitialFailed(object sender, AdFailedToLoadEventArgs args)
    {
        StartCoroutine(InterstitialFailedAsync());
    }

    IEnumerator InterstitialFailedAsync()
    {
        yield return new WaitForSeconds(3f);
        int trial = PlayerPrefs.GetInt("InterstitialFailed", 0);
        trial++;
        if (trial < 3)
            RequestInterstitialAd();
    }

    // !!!!!!!!!! LEVEL INDEX LERE DİKKAT !!!!!!!!!!!!
    public void ShowInterstitialAd()
    {
        int levelIndex = PlayerPrefs.GetInt("LastLevelIndex", 1);

        // Set interstitial interval according to the last selected level
        if (levelIndex >= 1 && levelIndex <= 12)
            interstitialInterval = 3;
        else if (levelIndex > 12 && levelIndex <= 30)
            interstitialInterval = 2;
        else
            interstitialInterval = 1;

        // Increment the counter
        int counter = PlayerPrefs.GetInt("ClickCounter", 0);
        counter++;
        PlayerPrefs.SetInt("ClickCounter", counter);

        // If counter reaches ... and ad is loaded then show it
        if (interstitialAd.IsLoaded() && counter >= interstitialInterval)
            interstitialAd.Show();
    }

    #endregion


    #region REWARDED AD
    private void RequestRewardedAd()
    {
        rewardedAd = new RewardedAd(rewardedAdUnitId);

        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;
        rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        
        AdRequest request = new AdRequest.Builder().Build();
        rewardedAd.LoadAd(request);
    }

    // Reset the counter for failed ads
    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        PlayerPrefs.SetInt("RewardedFailed", 0);
    }

    // Show Hint OR give extra time to the user as a reward
    public void HandleUserEarnedReward(object sender, Reward args) 
    {
        //PlayerPrefs.SetInt("ClickCounter", 0);
        if (rewardType == 1)
            FindObjectOfType<GameManager>().ShowHintPanel();
        else if (rewardType == 2)
            FindObjectOfType<GameManager>().AddExtraTime();
    }

    // Request new rewarded ad
    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        RequestRewardedAd();
    }

    // Try to load new ad if ad is failed (limit the trial with 3 times)
    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        StartCoroutine(RewardedFailedAsync());
    }

    IEnumerator RewardedFailedAsync()
    {
        yield return new WaitForSeconds(3f);
        int trial = PlayerPrefs.GetInt("RewardedFailed", 0);
        trial++;
        if (trial < 3)
            RequestRewardedAd();
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd.IsLoaded())
            rewardedAd.Show();
        else
            FindObjectOfType<GameManager>().noRewardPanel.SetActive(true);
    }

    #endregion

    private void OnDestroy()
    {
        if(bannerView != null)
            bannerView.Destroy();
        if(interstitialAd != null)
            interstitialAd.Destroy();
    }

}
