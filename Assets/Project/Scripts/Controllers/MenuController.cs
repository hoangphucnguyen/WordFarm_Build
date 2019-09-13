using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure;
using GameFramework.Preferences;
using GoogleMobileAds.Api;
using GameFramework.GameObjects;
using GameFramework.Messaging;
using System;
using GameFramework.Display.Placement;
using GameFramework.Display.Other;
using GameFramework.GameObjects.Components;
using UnityEngine.Advertisements;
using PaperPlaneTools;
using Flurry;
using System.Globalization;
using Fabric.Internal.Runtime;
using GameFramework.Localisation;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameStructure.Levels.ObjectModel;
using DG.Tweening;
using GameFramework.Facebook.Messages;

public class MenuController : Singleton<MenuController>
{

    private bool ColdStart;
    private BannerView banner;
    private Vector2 settingsOffsetMin;
    private GameObject container;
    private GameObject _rewardShareButton;

    [SerializeField]
    private GameObject _pigContainer;

    // Use this for initialization
    void Start()
    {
        Branch.initSession(CallbackWithBranchUniversalObject);

        int show = PreferencesFactory.GetInt(Constants.KeyShowBannerOnMainMenuScreen, 0);

        container = GameObject.Find("Container");

        if (PreferencesFactory.GetInt(Constants.KeyNoAds, 0) == 1)
        {
            show = 0; // disable ads
        }

        _rewardShareButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "RewardButton", true);

        if (_rewardShareButton != null && RewardsShareHelper.WillReward())
        {
            ShowRewardShareButton();
        }

        #if !UNITY_EDITOR
        // delay request until AdColony is configured
        StartCoroutine(CoRoutines.DelayedCallback(1.5f, () =>
        {
            AdColonyManager.Instance.RequestAd();
        }));
        #endif

		if ( show > 0 ) { // show banner only when return to main screen, not cold start
			AddBanner ();
			PreferencesFactory.DeleteKey (Constants.KeyShowBannerOnMainMenuScreen);

			int CountClicks = PreferencesFactory.GetInt (Constants.KeyCountMainMenuClicks);
			PreferencesFactory.SetInt (Constants.KeyCountMainMenuClicks, CountClicks + 1);

   //         if (CountClicks > 0 && CountClicks % Constants.ShowAdsPerTime == 0 && Reachability.Instance.IsReachable ()) {
			//	Loading.Instance.Show ();

			//	AdColonyManager.Instance.SetCallback (BannerMainMenuClosed);
   //             AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());
			//}
		} else { // show banner on cold start
			ColdStart = true;

			int CountMainMenuColdStart = PreferencesFactory.GetInt (Constants.KeyCountMainMenuColdStart);
			PreferencesFactory.SetInt (Constants.KeyCountMainMenuColdStart, CountMainMenuColdStart + 1);
		}
			
		GameManager.SafeAddListener<BannerLoadedMessage> (BannerLoadedHandler);
		GameManager.SafeAddListener<VideoAdShowingMessage> (VideoAdShowingHandler);

		((CustomGameManager)GameManager.Instance).ResetDefaultSound ();

		if (!Debug.isDebugBuild) {
			FlurryIOS.LogPageView ();
			FlurryAndroid.OnPageView ();

			Fabric.Answers.Answers.LogContentView ("Menu", "Screen");
		}

        StartCoroutine(CoRoutines.DelayedCallback(0.5f, () =>
        {
            CheckForPrize();
        }));
                       
        GameSparksManager.Instance.SetGameMode(GameMode.Single);

        AnimatePig();
	}

    void CallbackWithBranchUniversalObject(BranchUniversalObject universalObject, BranchLinkProperties linkProperties, string error)
    {
        if (error != null)
        {
            
        }
        else if (linkProperties.controlParams.Count > 0)
        {
            
        }
    }

    public void Invite() {
        InviteController.Instance.Show();
    }

    private void AnimatePig() {
        Vector3 _startPosition = _pigContainer.transform.position;
        RectTransform _rectTransform = _pigContainer.transform.parent.transform as RectTransform;

        Vector3 _position;
        _position = new Vector3(1.2f, -3.0f, 0);

		_pigContainer.transform.DOMove(_position, 3).SetEase(Ease.Linear).OnComplete(() => {
            ReversePig(_startPosition);
		});
    }

    private void ReversePig(Vector3 _startPosition) {
        _pigContainer.GetComponent<Animator>().speed = 0f;
        _pigContainer.transform.DORotate(new Vector3(0, 180.0f, 0), 0.2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            _pigContainer.GetComponent<Animator>().speed = 1f;
            _pigContainer.transform.DOMove(_startPosition, 5).SetEase(Ease.Linear).OnComplete(() =>
            {
                _pigContainer.transform.DORotate(new Vector3(0, 0, 0), 0.0f);
                AnimatePig();
            });
        });
    }

    protected override void OnApplicationPause(bool pauseStatus)
	{
        base.OnApplicationPause(pauseStatus);

		if ( !pauseStatus ) {
			CheckForPrize ();
		}
	}

    static GameObject _prizeButtonObject;

	void CheckForPrize() {
		var isPrizeAvailable = CustomFreePrizeManager.Instance.IsPrizeAvailable();
		
		if ( isPrizeAvailable ) {
            GameObject _buttonObject = GameObjectHelper.GetChildNamedGameObject(gameObject, "PurchaseButton", true);

            if ( _buttonObject == null ) {
                return;
            }
            
    		GameObject originalParent = _buttonObject.transform.parent.gameObject;
    		Vector3 originalPosition = _buttonObject.transform.position;

            DialogInstance _prizeInstance = CustomFreePrizeManager.Instance.ShowFreePrizeDialog(doneCallback: (DialogInstance dialogInstance) =>
    		{
    			_prizeButtonObject = null;

                if (_buttonObject != null && originalParent != null)
                {
                    GameObjectUtils.MoveObjectTo(_buttonObject, originalParent, originalPosition);
                }
    		});

		    _prizeButtonObject = _prizeInstance.gameObject;

		    GameObjectUtils.MoveObjectTo(_buttonObject, _prizeInstance.Target);
		}
	}

	void BannerClosed(string zoneId, int amount, bool success) {
		if ( !success ) {
			ShowVideoAd (BannerClosedProcess);
			return;
		}

		Loading.Instance.Hide ();
		Play ();
	}

	void BannerMainMenuClosed(string zoneId, int amount, bool success) {
		if ( !success ) {
			ShowVideoAd (BannerMainMenuClosed);
			return;
		}

		Loading.Instance.Hide ();
	}

	void BannerMainMenuClosed(int amount, bool success) {
		Loading.Instance.Hide ();
		// noop
	}

	void BannerClosedProcess(int amount, bool success) {
		Loading.Instance.Hide ();
		Play ();
	}

	public void ShowVideoAd(Action<int, bool> action)
	{
		if (Advertisement.IsReady("video"))
		{
			var options = new ShowOptions { resultCallback = (ShowResult result) => {
					action(0, result == ShowResult.Finished);
				} };
			Advertisement.Show("video", options);
		} else {
			Loading.Instance.Hide ();
			Play ();
		}
	}

	protected override void GameDestroy()
	{
		GameManager.SafeRemoveListener<BannerLoadedMessage> (BannerLoadedHandler);
		GameManager.SafeRemoveListener<VideoAdShowingMessage> (VideoAdShowingHandler);

		if (banner != null) {
			banner.Destroy ();
		}
	}

	static GameObject _marketObject;

	public void Market() {
		if ( _marketObject != null ) {
			return;
		}

		GameObject purchaseButton = GameObjectHelper.GetChildNamedGameObject (gameObject, "PurchaseButton", true);

        if ( purchaseButton == null ) {
            return;
        }

		GameObject originalParent = purchaseButton.transform.parent.gameObject;
		Vector3 originalPosition = purchaseButton.transform.position;

		DialogInstance marketInstance = DialogManager.Instance.Show ("Market", doneCallback: (DialogInstance dialogInstance) => {
			_marketObject = null;
			GameObjectUtils.MoveObjectTo (purchaseButton, originalParent, originalPosition);
		});

		_marketObject = marketInstance.gameObject;

		GameObjectUtils.MoveObjectTo (purchaseButton, marketInstance.Target);
	}

	public void PlayOnline() {
		GameManager.LoadSceneWithTransitions ("Lobby");
	}

	public void Play() {
        if (ColdStart && PreferencesFactory.GetInt(Constants.KeyNoAds, 0) == 0)
        {
            ColdStart = false;

            if (PreferencesFactory.GetInt(Constants.KeyShowTutorial, 0) == 0)
            {
                PreferencesFactory.SetInt(Constants.KeyShowTutorial, 1);
            }
		}

		NextScene ();
	}

	void NextScene() {
        Level level = LevelController.FirstUnplayedLevel();

        if (level != null)
        {
            GameManager.Instance.Levels.Selected = level;

            GameManager.LoadSceneWithTransitions("Game");
        }
        else
        {
            GameManager.LoadSceneWithTransitions("Levels");
        }

		if (banner != null) {
			banner.Hide ();
		}
	}

	void RemoveBanner() {
		if (banner != null) {
			banner.Hide ();
			banner.Destroy ();
		}
	}

	void AddBanner() {
        if (Constants.KeyEnableAdmobAds == false) {
            return;
        }

		RemoveBanner ();

		// reset layout
		RectTransform rect = container.transform as RectTransform;
		rect.offsetMin = new Vector2 (rect.offsetMin.x, 0);

		//

		CustomGameManager manager = CustomGameManager.Instance as CustomGameManager;

		banner = manager.AddBanner (AdSize.SmartBanner);

		if (banner != null) {
			rect.offsetMin = new Vector2 (rect.offsetMin.x, (manager.CalculateBannerHeight() * DisplayMetrics.GetScale()));

			banner.Show ();

			banner.OnAdLoaded += onAdLoadedHandler;
		}
	}

	bool VideoAdShowingHandler(BaseMessage message) {
		VideoAdShowingMessage msg = message as VideoAdShowingMessage;

		if ( msg.Showing ) {
			RemoveBanner ();
		} else {
			if (banner != null) {
				AddBanner ();
			}
		}

		return true;
	}

	bool BannerLoadedHandler (BaseMessage message) {
		CustomGameManager manager = CustomGameManager.Instance as CustomGameManager;

		RectTransform rect = container.transform as RectTransform;
		rect.offsetMin = new Vector2 (rect.offsetMin.x, (manager.CalculateBannerHeight() * DisplayMetrics.GetScale()));

		return true;
	}

	void onAdLoadedHandler(object sender, EventArgs args)
	{
		GameManager.SafeQueueMessage (new BannerLoadedMessage());
	}

    void ShowRewardShareButton()
    {
        _rewardShareButton.SetActive(true);
    }

    public void RewardShare()
    {
#if UNITY_EDITOR
        RewardsShareHelper.RewardShareCoins();
        _rewardShareButton.SetActive(false);
        return;
#endif

        FacebookRequests.Instance.FeedShare(Constants.ShareURLLink(Constants.ShareCodes.FacebookFeed),
                                            LocaliseText.Get("GameName"),
                                            string.Format("{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.FacebookFeed), Constants.HashTagSocials),
                                            (FacebookShareLinkMessage.ResultType result) =>
                                            {
                                                if (result == FacebookShareLinkMessage.ResultType.OK)
                                                {
                                                    RewardsShareHelper.RewardShareCoins();
                                                    _rewardShareButton.SetActive(false);
                                                }

                                                if (!Debug.isDebugBuild)
                                                {
                                                    Flurry.Flurry.Instance.LogEvent("Share_Facebook_Feed");
                                                    Fabric.Answers.Answers.LogLevelStart("Share_Facebook_Feed");
                                                }
                                            });
    }

    public void TestPush() {
        int n_id = 55555;
        LocalNotifications.CancelNotification(n_id);
        LocalNotifications.RegisterNotification(n_id, UnbiasedTime.Instance.Now().AddSeconds(30), "Test push notification");

        DialogManager.Instance.ShowInfo("Notification was registered. It will appear in 30 seconds.");
    }
}
