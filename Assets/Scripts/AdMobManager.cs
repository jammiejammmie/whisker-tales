using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif
/// <summary>
/// AdMob 광고 관리 매니저
/// Google Mobile Ads SDK가 설치되어 있을 때만 컴파일됨
/// </summary>
public class AdMobManager : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IOS
    private RewardedAd rewardedAd; 
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test Ad Unit ID 
    public void Start() 
    { 
        MobileAds.Initialize(initStatus => { 
            LoadRewardedAd(); 
        }); 
    } 
    public void LoadRewardedAd() 
    { 
        if (rewardedAd != null) 
        { 
            rewardedAd.Destroy(); 
            rewardedAd = null; 
        } 
        var adRequest = new AdRequest.Builder().Build(); 
        RewardedAd.Load(adUnitId, adRequest, 
            (RewardedAd ad, LoadAdError error) => 
            { 
                if (error != null || ad == null) 
                { 
                    Debug.LogError("Rewarded ad failed to load with error: " + error); 
                    return; 
                } 
                Debug.Log("Rewarded ad loaded successfully."); 
                rewardedAd = ad; 
                RegisterEventHandlers(rewardedAd); 
            }); 
    } 
    public void ShowRewardedAd() 
    { 
        if (rewardedAd != null && rewardedAd.CanShowAd()) 
        { 
            rewardedAd.Show((RewardItem rewardItem) => 
            { 
                Debug.Log($"User rewarded with {rewardItem.Amount} {rewardItem.Type}"); 
                // TODO: Implement actual reward logic (e.g., give coins, items) 
            }); 
        } 
        else 
        { 
            Debug.LogWarning("Rewarded ad not ready yet."); 
            LoadRewardedAd(); // Try loading again 
        } 
    } 
    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to show with error: " + error.GetMessage());
            LoadRewardedAd();
        };
    }
#else
    public void Start()
    {
        Debug.LogWarning("[AdMobManager] Google Mobile Ads SDK not available on this platform");
    }
    public void LoadRewardedAd()
    {
        Debug.LogWarning("[AdMobManager] LoadRewardedAd: Not available without Google Mobile Ads SDK");
    }
    public void ShowRewardedAd()
    {
        Debug.LogWarning("[AdMobManager] ShowRewardedAd: Not available without Google Mobile Ads SDK");
    }
#endif
}
