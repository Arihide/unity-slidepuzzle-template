using System.Collections;
using UnityEngine;
using GoogleMobileAds.Api;
public class Admob : Singleton<Admob>
{
    public BannerView bannerView;
    public InterstitialAd interstitialAd;

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
        });

        RequestBanner();
    }

    private void RequestBanner()
    {
        if (!enabled) return;

        // TODO: 自身のAdmobのApp Idを入力すること
#if UNITY_ANDROID && !DEBUG
        string appId = "";
#elif UNITY_IOS && !DEBUG
        string appId = "";
#else
        string appId = "ca-app-pub-3940256099942544/6300978111"; // TEST BANNER
#endif

        bannerView = new BannerView(appId, AdSize.SmartBanner, AdPosition.Bottom);
        bannerView.Show();

        AdRequest request = new AdRequest.Builder().Build();

        bannerView.LoadAd(request);
    }

    public void RequestInterstitial()
    {
        if (!enabled) return;

        StartCoroutine(RequestInterstitialImpl());
    }

    private IEnumerator RequestInterstitialImpl()
    {

        // TODO: 自身のAdmobのApp Idを入力すること
#if UNITY_ANDROID && !DEBUG
        string appId = "";
#elif UNITY_IOS && !DEBUG
        string appId = "";
#else
        string appId = "ca-app-pub-3940256099942544/4411468910"; // TEST BANNER
#endif

        interstitialAd = new InterstitialAd(appId);

        AdRequest request = new AdRequest.Builder().Build();
        interstitialAd.LoadAd(request);

        while (!interstitialAd.IsLoaded())
        {
            yield return null;
        }
        interstitialAd.Show();
    }
}