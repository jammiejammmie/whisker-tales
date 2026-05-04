using UnityEngine; 
using GoogleMobileAds.Api; 

public class AdMobManager : MonoBehaviour 
{ 
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
            Debug.LogError("Rewarded ad failed to show with error: " + error.Get  Message()); 
            LoadRewardedAd(); 
        }; 
    } 
}
