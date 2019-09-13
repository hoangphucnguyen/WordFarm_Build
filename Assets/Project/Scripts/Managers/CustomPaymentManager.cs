using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using GameFramework.Billing.Components;
using GameFramework.GameStructure;
using GameFramework.Billing.Messages;
using GameFramework.Messaging;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Preferences;
using GameFramework.Localisation;
using GameSparks.Api.Requests;
using GameSparks.Core;
using System.Globalization;
using GameFramework.Localisation.Messages;
using GameFramework.Billing;
using UnityEngine.Purchasing.Security;
using GameFramework.GameObjects.Components;
using UnityEngine.Assertions;
using System.Text;
using GameFramework.Debugging;

public class CustomPaymentManager : SingletonPersistant<CustomPaymentManager>, IStoreListener
{
    [Header("Payment Setup")]

    /// <summary>
    /// Whether to initialise the payment backend on awake. 
    /// </summary>
    /// If you set this to false then be sure to manually call InitializePurchasing
    [Tooltip("Whether to initialise the payment backend on awake. \n\nIf you set this to false then be sure to manually call InitializePurchasing.")]
    public bool InitOnAwake = true;

    /// <summary>
    /// A list of the product id's that you use in your game. 
    /// </summary>
    /// These can either the build in product id's or your own custom ones. These should be the same as in the backend store.
    [Tooltip("A list of the product id's that you use in your game. THese can either the build in product id's or your own custom ones.\n\nThese should be the same as in the backend store.")]
    public PaymentProduct[] Products;

    // setup references
    IStoreController _controller;              // Reference to the Purchasing system.
    IExtensionProvider _extensions;            // Reference to store-specific Purchasing subsystems.


    [HideInInspector]
    public static PaymentProductCoins[] Coins;

    private bool PurchaseInProgress;
    private string LastPurchaseProductId;

    private CrossPlatformValidator validator;
    private StringBuilder stringBuilder = new StringBuilder();

    public Product Product(string productId)
    {
        if (Controller() == null)
        {
            return null;
        }

        return Controller().products.WithID(productId);
    }

    public IStoreController Controller()
    {
        return this._controller;
    }

    protected override void GameSetup()
    {
        PaymentProductCoins[] coins = new PaymentProductCoins[6];
        coins[0] = new PaymentProductCoins { Price = 0.99m, Currency = "USD", Coins = 1500, NoAds = 0, EventCode = "IAP_BunchofCoins", Description = LocaliseText.Get("Payment.BunchOfCoins"), LanguageKey = "Payment.BunchOfCoins", Product = new PaymentProduct { Name = "com.wordfarm.iap.BunchOfCoins", ProductType = ProductType.Consumable } };
        coins[1] = new PaymentProductCoins { Price = 4.99m, Currency = "USD", Coins = 8700, NoAds = 0, EventCode = "IAP_BagofCoins", Description = LocaliseText.Get("Payment.BagOfCoins"), LanguageKey = "Payment.BagOfCoins", Product = new PaymentProduct { Name = "com.wordfarm.iap.BagOfCoins", ProductType = ProductType.Consumable } };
        coins[2] = new PaymentProductCoins { Price = 7.99m, Currency = "USD", Coins = 17500, NoAds = 0, EventCode = "IAP_SackofCoins", Description = LocaliseText.Get("Payment.SackOfCoins"), LanguageKey = "Payment.SackOfCoins", Product = new PaymentProduct { Name = "com.wordfarm.iap.SackOfCoins", ProductType = ProductType.Consumable } };
        coins[3] = new PaymentProductCoins { Price = 11.99m, Currency = "USD", Coins = 38900, NoAds = 0, EventCode = "IAP_PotofCoins", Description = LocaliseText.Get("Payment.PotOfCoins"), LanguageKey = "Payment.PotOfCoins", Product = new PaymentProduct { Name = "com.wordfarm.iap.PotOfCoins", ProductType = ProductType.Consumable } };
        coins[4] = new PaymentProductCoins { Price = 16.99m, Currency = "USD", Coins = 60000, NoAds = 0, EventCode = "IAP_ChestofCoins", Description = LocaliseText.Get("Payment.ChestOfCoins"), LanguageKey = "Payment.ChestOfCoins", Product = new PaymentProduct { Name = "com.wordfarm.iap.ChestOfCoins", ProductType = ProductType.Consumable } };
        coins[5] = new PaymentProductCoins { Price = 1.99m, Currency = "USD", Coins = 0, NoAds = 1, EventCode = "IAP_NoAds", Description = LocaliseText.Get("Payment.NoAds"), LanguageKey = "Payment.NoAds", Product = new PaymentProduct { Name = "com.wordfarm.iap.noads", ProductType = ProductType.NonConsumable } };

        CustomPaymentManager.Coins = coins;

        Products = new PaymentProduct[coins.Length];

        for (int i = 0; i < coins.Length; i++)
        {
#if UNITY_ANDROID
            coins[i].Product.Name = coins[i].Product.Name.ToLower(); // android play store allow only lowercase
#endif
			Products[i] = coins[i].Product;
        }

        // Initialise purchasing
        if (InitOnAwake)
        {
            InitializePurchasing();
        }

		GameManager.SafeAddListener<ItemPurchasedMessage> (ItemPurchasedHandler);
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);

        validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(),
                                               UnityChannelTangle.Data(), Application.identifier);
	}

	protected override void GameDestroy()
	{
		GameManager.SafeRemoveListener<ItemPurchasedMessage>(ItemPurchasedHandler);
		GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
	}

	bool LocalisationHandler(BaseMessage message)
	{
		for (int i = 0; i < CustomPaymentManager.Coins.Length; i++)
		{
			CustomPaymentManager.Coins[i].Description = LocaliseText.Get(CustomPaymentManager.Coins[i].LanguageKey);
		}

		return true;
	}

    /// <summary>
    /// Method to initialise the payment backend
    /// </summary>
    /// This is called automatically if you enable InitOnAwake, otherwise you will need to call it yourself
    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add products to sell / restore by way of its identifier, associating the general identifier with its store-specific identifiers.
        Assert.IsTrue(Products.Length > 0, "You need to add products if using Payments");
        foreach (PaymentProduct product in Products)
            builder.AddProduct(product.Name, product.ProductType);

        UnityPurchasing.Initialize(this, builder);
    }


    /// <summary>
    /// Returns whether the payment system is initialised and ready for use.
    /// </summary>
    /// <returns></returns>
    public bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return _controller != null && _extensions != null;
    }

    public virtual void BuyProductId(string productId)
    {
        if (PurchaseInProgress == true)
        {
            DialogManager.Instance.ShowInfo("Purchase processing...");
            return;
        }

        stringBuilder.Length = 0;
        PurchaseInProgress = true;
        LastPurchaseProductId = productId;

        //

        // If the stores throw an unexpected exception, use try..catch to protect my logic here.
        try
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing system's products collection.
                Product product = _controller.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Loading.Instance.Show();
                    stringBuilder.AppendFormat("Buy product {0} at {1}, coins {2}, points {3}\n", productId, UnbiasedTime.Instance.UTCNow().ToString(CultureInfo.InvariantCulture), GameManager.Instance.Player.Coins, GameManager.Instance.Player.Score);

                    MyDebug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    _controller.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    DialogManager.Instance.ShowError(textKey: "Billing.NotAvailable");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
                DialogManager.Instance.ShowError(textKey: "Billing.NotInitialised");
            }
        }
        // Complete the unexpected exception handling ...
        catch (Exception e)
        {
            // ... by reporting any unexpected exception for later diagnosis.
            DialogManager.Instance.ShowError(LocaliseText.Format("GeneralMessage.Error.GeneralError", e.ToString()));
        }
    }

    /// <summary>
    /// Restore purchases previously made by this customer. 
    /// </summary>
    /// Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
    public void RestorePurchases()
    {
        // If Purchasing has been initialised ...
        if (IsInitialized())
        {
            //TODO: the below conditional should not be needed as interfaces should return empty on unsupported platforms!
            // If we are running on an Apple device ... 
            //if (Application.platform == RuntimePlatform.IPhonePlayer || 
            //  Application.platform == RuntimePlatform.OSXPlayer)
            //{
            //  // ... begin restoring purchases
            MyDebug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = _extensions.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                MyDebug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                if (result)
                {
                    // This does not mean anything was restored,
                    // merely that the restoration process succeeded.
                    DialogManager.Instance.ShowInfo(textKey: "Billing.RestoreSucceeded");
                }
                else
                {
                    // Restoration failed.
                    DialogManager.Instance.ShowError(textKey: "Billing.RestoreFailed");
                }
            });
            //}
            //// Otherwise ...
            //else
            //{
            //  // We are not running on an Apple device. No work is necessary to restore purchases.
            //  MyDebug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            //}
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
            DialogManager.Instance.ShowError(textKey: "Billing.NotInitialised");
        }
    }

    /// <summary>
    /// Called when a purchase completes. This automatically handles certain types of purchase and notifications
    /// </summary>
    /// If you need custom processing then you may subclass PaymentManager and override this method.
    /// This may be called at any time after OnInitialized().
    public virtual PurchaseProcessingResult ProcessPurchase(string productId)
    {
        Payment.ProcessPurchase(productId);

        stringBuilder.AppendFormat("Product {0} was purchased at {1}, coins {2}, points {3}\n", LastPurchaseProductId, UnbiasedTime.Instance.UTCNow().ToString(CultureInfo.InvariantCulture), GameManager.Instance.Player.Coins, GameManager.Instance.Player.Score);

        SaveLog();

        return PurchaseProcessingResult.Complete;
    }

    //  
    // --- IStoreListener
    //

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        MyDebug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        this._controller = controller;
        // Store specific subsystem, for accessing device-specific store features.
        this._extensions = extensions;
    }


    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);

        Fabric.Crashlytics.Crashlytics.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        PurchaseInProgress = false;
        Loading.Instance.Hide();

        try
        {
            var result = validator.Validate(args.purchasedProduct.receipt);
            var products = new List<string>();

            foreach (IPurchaseReceipt productReceipt in result)
            {
                if (args.purchasedProduct.transactionID == productReceipt.transactionID)
                {
                    products.Add(productReceipt.productID);
                }
            }

            if ( !products.Contains(LastPurchaseProductId) ) {
                return PurchaseProcessingResult.Complete;
            }

            return ProcessPurchase(args.purchasedProduct.definition.id);
        }
        catch (IAPSecurityException ex)
        {
            return PurchaseProcessingResult.Complete;
        }
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        PurchaseInProgress = false;
        Loading.Instance.Hide();

        stringBuilder.AppendFormat("Product {0} failed at {1}, reason: {2}\n", LastPurchaseProductId, UnbiasedTime.Instance.UTCNow().ToString(CultureInfo.InvariantCulture), failureReason);

        SaveLog();

        MyDebug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

        Fabric.Crashlytics.Crashlytics.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
        switch (failureReason)
        {
            // for these cases we don't need to inform further
            case PurchaseFailureReason.UserCancelled:
                break;
            // for these we show an error
            default:
                DialogManager.Instance.ShowError(LocaliseText.Format("GeneralMessage.Error.GeneralError", failureReason));
                break;
        }
    }

    void SaveLog() {
        if ( stringBuilder.Length == 0 ) {
            return;
        }

        GSRequestData logData = new GSRequestData();
        logData.AddString("key", "InAppPurchase");
        logData.AddString("message", "User purchase in-app");

        GSData _d = new GSData(new Dictionary<string, object>(){
                        {"purchase", stringBuilder},
                    });

        logData.AddObject("data", _d);

        GameSparksManager.Instance.Log(logData);

        string purchaseLog = PreferencesFactory.GetString("PurchaseLog", "");

        purchaseLog = string.Format("{0}\n-----------\n{1}", purchaseLog, stringBuilder);

        PreferencesFactory.SetString("PurchaseLog", purchaseLog);

        stringBuilder.Length = 0;
    }

	bool ItemPurchasedHandler(BaseMessage message) {
		ItemPurchasedMessage msg = message as ItemPurchasedMessage;

		PaymentProductCoins[] coins = CustomPaymentManager.Coins;
		for (int i = 0; i < coins.Length; i++) {
			if ( coins[i].Product.Name.Equals(msg.ProductID) ) {
				if (coins [i].Coins > 0) {
					GameObject animatedCoins = GameObject.Find ("AddCoinsAnimated");

					if (animatedCoins != null) {
						GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
						AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

						addCoins.AnimateCoinsAdding (coins [i].Coins);
					} else {
						GameManager.Instance.Player.AddCoins (coins [i].Coins);
						GameManager.Instance.Player.UpdatePlayerPrefs ();
					}
				}

				if ( coins[i].NoAds == 1 ) {
					PreferencesFactory.SetInt (Constants.KeyNoAds, 1);
				}

				PreferencesFactory.Save ();

				if (!Debug.isDebugBuild) {
					Flurry.Flurry.Instance.LogEvent (coins [i].EventCode);
					Fabric.Answers.Answers.LogPurchase (coins [i].Price, 
						coins [i].Currency, 
						true, 
						coins [i].Description, 
						coins [i].NoAds == 1 ? "Ads" : "Coins",
						msg.ProductID
					);
                    Branch.userCompletedAction("InApp");
				}

				if ( GameSparksManager.IsUserLoggedIn () ) {
					GSRequestData json = new GSRequestData ();
					json.Add ("package", msg.ProductID);
                    json.Add ("price", string.Format("{0} {1}", coins[i].Price, coins[i].Currency));
					json.Add ("date", DateTime.UtcNow.ToString (CultureInfo.InvariantCulture));

					new LogEventRequest ()
						.SetEventKey ("IAPPurchase")
						.SetEventAttribute ("data", json)
						.Send (((response) => {
							
						}));
				}

                if (coins[i].NoAds == 1)
                {
                    DialogManager.Instance.Show(prefabName: "GeneralMessageOkButton",
                        title: LocaliseText.Get("Text.Success"),
                        text: LocaliseText.Get("Payment.PurchaseSuccess"),
                        dialogButtons: DialogInstance.DialogButtonsType.Ok);
                }

				break;
			}
		}

		return true;
	}
}

[Serializable]
public class PaymentProductCoins
{
	public PaymentProduct Product;
	public decimal Price;
	public string Currency;
	public int Coins;
	public string Description;
    public string LanguageKey;
	public int NoAds;
	public string EventCode;
}

[Serializable]
public class PaymentProduct
{
    public ProductType ProductType;
    public string Name;
}