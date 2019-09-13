using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure;
using UnityEngine.UI.Extensions;
using GameFramework.GameStructure.Levels.ObjectModel;
using System;
using FlipWebApps.BeautifulTransitions.Scripts.Transitions.Components;
using GameFramework.GameObjects;
using GameFramework.Preferences;
using GameFramework.GameObjects.Components;
using GoogleMobileAds.Api;
using GameFramework.Messaging;
using PaperPlaneTools;
using GameFramework.Display.Other;
using UnityEngine.UI;
using DG.Tweening;
using Flurry;
using GameFramework.Localisation;

public class LevelsListController : Singleton<LevelsListController>
{

    private BannerView banner;
    private GameObject container;

    [SerializeField]
    private GameObject packsContainer;
    [SerializeField]
    private GameObject levelsContainer;
    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private Text signText;

    private Pack _currentPack;
    private Rank _currentRank;
    private bool _showingLevels;

    private HorizontalScrollSnap _scrollSnap;

    private GameObject _rankSignGameObject;
    private Text _rankSignText;
    private GameObject _nextButton;
    private CustomVerticalScrollSnap _ranksVerticalScroll;

    // Use this for initialization
    void Start()
    {
        _scrollSnap = GameObjectHelper.GetChildComponentOnNamedGameObject<HorizontalScrollSnap>(gameObject, "ScrollView", true);
        _nextButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "NextButton", true);

        container = gameObject;

        //

        AddBanner();

        GameManager.SafeAddListener<BannerLoadedMessage>(BannerLoadedHandler);
        GameManager.SafeAddListener<VideoAdShowingMessage>(VideoAdShowingHandler);

        StartCoroutine(CoRoutines.DelayedCallback(0.5f, BonusCoins));

        ((CustomGameManager)GameManager.Instance).ResetDefaultSound();

        if (!Debug.isDebugBuild)
        {
            FlurryIOS.LogPageView();
            FlurryAndroid.OnPageView();

            Fabric.Answers.Answers.LogContentView("Levels", "Screen");
        }

        _rankSignGameObject = GameObjectHelper.GetChildNamedGameObject(packsContainer, "Sign", true);
        _rankSignText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(_rankSignGameObject, "RankName", true);
        _ranksVerticalScroll = GameObjectHelper.GetChildComponentOnNamedGameObject<CustomVerticalScrollSnap>(packsContainer, "ScrollView", true);

        PrepareRankForLevel(GameManager.Instance.Levels.Selected);

        _ranksVerticalScroll.StartingScreen = LevelController.Ranks().Items.Length - (_currentRank.Number - 1) - 1;

        if (PreferencesFactory.GetInt(Constants.KeyShowSelectedPack) > 0
            && GameManager.Instance.Levels.Selected != null)
        {
            PreferencesFactory.DeleteKey(Constants.KeyShowSelectedPack);

            GoToLevel(GameManager.Instance.Levels.Selected);
        }

#if !UNITY_EDITOR
        AdColonyManager.Instance.RequestAd(); // request ads to cache for CustomLevelButton.cs
#endif
    }

    public void Close() {
		StartCoroutine (CoRoutines.DelayedCallback (Constants.DelayButtonClickAction, () => {
			GameManager.LoadSceneWithTransitions ("Menu");
		}));
	}

	public void CloseButton() {
        CloseProcess();
	}

	void CloseProcess() {
		if ( _showingLevels ) {
			ClosePackProcess ();
			return;
		}
	}

	void ChangeSignText(string text) {
		signText.DOFade (0f, 0.15f).SetEase (Ease.Linear).OnComplete (()=>{
			signText.text = text;

			signText.DOFade (1f, 0.25f).SetEase (Ease.Linear);
		});
	}

	// Ranks

	public void OpenRankButton(Rank rank) {
		
	}

	// Packs

    void SetPackData(Pack pack, GameObject newObject) {
		if (pack.IsUnlocked)
		{
			CanvasGroup canvasGroup = GameObjectHelper.GetChildComponentOnNamedGameObject<CanvasGroup>(newObject, "PlayButton", true);
			canvasGroup.alpha = 1f;
		}

		Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(newObject, "Name", true);
        text.text = LocaliseText.Get(pack.JsonData.GetString("name"));

		GameObject iconObject = GameObjectHelper.GetChildNamedGameObject(newObject, "Icon", true);

		if (pack.Progress > 0.9f)
		{
			iconObject.SetActive(true);
		}

		int points = LevelController.Instance.PointsForPack(pack);

		if (points > 0)
		{
			GameObject pointsObject = GameObjectHelper.GetChildNamedGameObject(newObject, "Points", true);
			pointsObject.SetActive(true);

			Text pointsNumber = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(pointsObject, "PointsNumber", true);
			pointsNumber.text = points.ToString();
		}
    }

    public void PackWillChange(VerticalScrollSnap scrollSnap) {
        return;
        // TODO
        ScrollRect scrollRect = scrollSnap.GetComponent<ScrollRect>();

        if ( scrollRect == null ) {
            return;
        }

        Transform child = scrollRect.content.GetChild(scrollSnap.CurrentPage);

        if ( child == null ) {
            return;
        }

        GameObject pageContainer = child.gameObject;
        Rank rank = LevelController.Ranks().GetItem(scrollSnap.CurrentPage + 1);

        ItemsRange range = LevelController.RankPacksRange(rank);

        int index = 0;
        for (int i = range.From; i < range.To + 1; i++)
        {
            GameObject newObject = pageContainer.transform.GetChild(index).gameObject;

			Pack pack = LevelController.Packs().GetItem(i);

            if (pack.JsonData == null)
            {
                pack.LoadData();
            }

            SetPackData(pack, newObject);

            index++;
        }
    }

    public void PackChanged(VerticalScrollSnap scrollSnap) {
        Rank rank = LevelController.Ranks().GetItem(scrollSnap.CurrentPage+1);
        _currentRank = rank;
    }

	public void OpenPackButton(Pack pack) {
		if ( _showingLevels ) {
			return;
		}

		if ( pack == null ) {
			return;
		}

		if ( pack.IsUnlocked == false ) {
			return;
		}

        ChangeSignText (LocaliseText.Get(pack.JsonData.GetString("name")));

		_showingLevels = true;
		_currentPack = pack;

        OpenPack();
	}

	void OpenPack() {
		((CustomGameManager)CustomGameManager.Instance).Packs.Selected = _currentPack;

        _nextButton.SetActive(false);
		levelsContainer.SetActive (true);
		
		// show close button
		backButton.SetActive (true);

		_scrollSnap.NextScreen ();
	}

	public void ClosePack() {
        ClosePackProcess();
	}

	void ClosePackProcess() {
        ChangeSignText (LocaliseText.Get("Text.Ranks"));

        _nextButton.SetActive(true);
		_scrollSnap.PreviousScreen ();
		levelsContainer.SetActive (false);
		_showingLevels = false;

		// hide close button
		backButton.GetComponent<Animator>().SetTrigger("CloseDisable");
		backButton.SetActive(false);
	}

    void PrepareRankForLevel(Level level) {
		Pack pack = LevelController.Instance.PackForLevel(level);

        // user played all levels, this level is random generated
        if ( pack == null ) {
            Level lastLevel = GameManager.Instance.Levels.GetItem(((CustomGameManager)CustomGameManager.Instance).StartupLevels);
            pack = LevelController.Instance.PackForLevel(lastLevel);
        }

		// get rank for this pack
		Rank rank = LevelController.Instance.RankForPack(pack);

		// select Packs for this level
		_currentPack = pack;
		((CustomGameManager)CustomGameManager.Instance).Packs.Selected = _currentPack;

		// select Ranks for this level pack
		_currentRank = rank;
		((CustomGameManager)CustomGameManager.Instance).Ranks.Selected = _currentRank;
    }

    // Levels

    void GoToLevel(Level level) {
		// show close button
		backButton.SetActive(true);
        _nextButton.SetActive(false);

        PrepareRankForLevel(level);

        ChangeSignText(LocaliseText.Get(_currentPack.JsonData.GetString("name")));

        _showingLevels = true;

        _scrollSnap.GoToScreen(1);
    }

	//

	void BonusCoins() {
		int coins = PreferencesFactory.GetInt (Constants.KeyDailyLoginBonusCoins, 0);

		if ( coins > 0 ) {
			PreferencesFactory.DeleteKey (Constants.KeyDailyLoginBonusCoins);

			GameObject animatedCoins = GameObject.Find ("AddCoinsAnimated");
			GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
			AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

			addCoins.AnimateCoinsAdding (coins);
		}
	}

	void ShowInviteBalloon() {
		InviteController.Instance.PrepareAndShowInviteBalloon ();
	}

	protected override void GameDestroy()
	{
		GameManager.SafeRemoveListener<BannerLoadedMessage> (BannerLoadedHandler);
		GameManager.SafeRemoveListener<VideoAdShowingMessage> (VideoAdShowingHandler);

		if (banner != null) {
			banner.Destroy ();
		}
	}

	public void StartGame() {
		TransitionManager.Instance.TransitionOutAndLoadScene ("Game");

		if ( banner != null ) {
			banner.Hide ();
		}
	}

	bool DeviceOrientationHandler (BaseMessage message) {
		if (banner != null) {
			AddBanner ();
		}

		return true;
	}

	void RemoveBanner() {
		if (banner != null) {
			banner.Hide ();
			banner.Destroy ();
		}
	}

	void AddBanner() {
		if (Constants.KeyEnableAdmobAds == false)
		{
			return;
		}

		RemoveBanner ();

		// reset layout
		//RectTransform rect = container.transform as RectTransform;
		//rect.offsetMin = new Vector2 (rect.offsetMin.x, 0);

		//

		CustomGameManager manager = CustomGameManager.Instance as CustomGameManager;
		banner = manager.AddBanner (AdSize.SmartBanner);

		if (banner != null) {
			//rect.offsetMin = new Vector2 (rect.offsetMin.x, (manager.CalculateBannerHeight() * DisplayMetrics.GetScale()));

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

		//RectTransform rect = container.transform as RectTransform;
		//rect.offsetMin = new Vector2 (rect.offsetMin.x, (manager.CalculateBannerHeight() * DisplayMetrics.GetScale()));

		return true;
	}

	void onAdLoadedHandler(object sender, EventArgs args)
	{
		GameManager.SafeQueueMessage (new BannerLoadedMessage());
	}
}
