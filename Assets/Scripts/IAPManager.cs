using UnityEngine;

public class IAPManager : MonoBehaviour
{
    public const string PRODUCT_ID_COIN_PACK_SMALL = "coin_pack_small";
    public const string PRODUCT_ID_COIN_PACK_LARGE = "coin_pack_large";
    public const string PRODUCT_ID_REMOVE_ADS = "remove_ads";

    void Start()
    {
        Debug.LogWarning("[IAPManager] Unity Purchasing SDK not installed.");
    }

    public void InitializePurchasing()
    {
        Debug.LogWarning("[IAPManager] InitializePurchasing: SDK not installed");
    }

    public void BuyCoinPackSmall()
    {
        Debug.LogWarning("[IAPManager] BuyCoinPackSmall: SDK not installed");
    }

    public void BuyCoinPackLarge()
    {
        Debug.LogWarning("[IAPManager] BuyCoinPackLarge: SDK not installed");
    }

    public void BuyRemoveAds()
    {
        Debug.LogWarning("[IAPManager] BuyRemoveAds: SDK not installed");
    }
}
