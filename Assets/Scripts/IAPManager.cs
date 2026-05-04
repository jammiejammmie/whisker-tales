using UnityEngine; 
using UnityEngine.Purchasing; 

public class IAPManager : MonoBehaviour, IStoreListener 
{ 
    private static IStoreController m_StoreController;          // The Unity Purchasing system. 
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing extensions. 

    // Product IDs for our in-app purchases 
    public const string PRODUCT_ID_COIN_PACK_SMALL = "coin_pack_small"; 
    public const string PRODUCT_ID_COIN_PACK_LARGE = "coin_pack_large"; 
    public const string PRODUCT_ID_REMOVE_ADS = "remove_ads"; 

    void Start() 
    { 
        if (m_StoreController == null) 
        { 
            InitializePurchasing(); 
        } 
    } 

    public void InitializePurchasing() 
    { 
        if (IsInitialized()) 
        { 
            return; 
        } 

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()); 

        builder.AddProduct(PRODUCT_ID_COIN_PACK_SMALL, ProductType.Consumable); 
        builder.AddProduct(PRODUCT_ID_COIN_PACK_LARGE, ProductType.Consumable); 
        builder.AddProduct(PRODUCT_ID_REMOVE_ADS, ProductType.NonConsumable); 

        UnityPurchasing.Initialize(this, builder); 
    } 

    private bool IsInitialized() 
    { 
        return m_StoreController != null && m_StoreExtensionProvider != null; 
    } 

    public void BuyCoinPackSmall() 
    { 
        BuyProductID(PRODUCT_ID_COIN_PACK_SMALL); 
    } 

    public void BuyCoinPackLarge() 
    { 
        BuyProductID(PRODUCT_ID_COIN_PACK_LARGE); 
    } 

    public void BuyRemoveAds() 
    { 
        BuyProductID(PRODUCT_ID_REMOVE_ADS); 
    } 

    void BuyProductID(string productId) 
    { 
        if (IsInitialized()) 
        { 
            Product product = m_StoreController.products.WithID(productId); 

            if (product != null && product.availableToPurchase) 
            { 
                Debug.Log($"Purchasing product asynchronously: {product.id}"); 
                m_StoreController.InitiatePurchase(product); 
            } 
            else 
            { 
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase"); 
            } 
        } 
        else 
        { 
            Debug.Log("BuyProductID FAIL. Not initialized."); 
        } 
    } 

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) 
    { 
        Debug.Log("OnInitialized: PASS"); 

        m_StoreController = controller; 
        m_StoreExtensionProvider = extensions; 
    } 

    public void OnInitializeFailed(InitializationFailureReason error) 
    { 
        Debug.Log($"OnInitializeFailed InitializationFailureReason: {error}"); 
    } 

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
    { 
        if (string.Equals(args.purchasedProduct.id, PRODUCT_ID_COIN_PACK_SMALL, System.StringComparison.Ordinal)) 
        { 
            Debug.Log("ProcessPurchase: PASS. Product: " + args.purchasedProduct.id); 
            // TODO: Grant small coin pack reward 
        } 
        else if (string.Equals(args.purchasedProduct.id, PRODUCT_ID_COIN_PACK_LARGE, System.StringComparison.Ordinal)) 
        { 
            Debug.Log("ProcessPurchase: PASS. Product: " + args.purchasedProduct.id); 
            // TODO: Grant large coin pack reward 
        } 
        else if (string.Equals(args.purchasedProduct.id, PRODUCT_ID_REMOVE_ADS, System.StringComparison.Ordinal)) 
        { 
            Debug.Log("ProcessPurchase: PASS. Product: " + args.purchasedProduct.id); 
            // TODO: Remove ads for the user 
        } 
        else 
        { 
            Debug.Log("ProcessPurchase: FAIL. Unrecognized product: " + args.purchasedProduct.id); 
        } 

        return PurchaseProcessingResult.Complete; 
    } 

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) 
    { 
        Debug.Log($"OnPurchaseFailed: FAIL. Product: {product.id}, PurchaseFailureReason: {failureReason}"); 
    } 
}
