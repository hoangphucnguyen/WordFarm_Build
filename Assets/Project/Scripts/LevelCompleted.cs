using System;
using System.Collections;
using GameFramework.GameObjects;
using GameFramework.GameObjects.Components;
using GameFramework.GameStructure;
using GameFramework.GameStructure.Levels;
using GameFramework.GameStructure.Levels.ObjectModel;
using GameFramework.Localisation;
using GameFramework.Social;
using GameFramework.UI.Other;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using GameFramework.GameStructure.Game;
using GameFramework.Preferences;
using GameFramework.UI.Dialogs.Components;
using UnityEngine.UI;
using Facebook.Unity;
using GameFramework.Facebook.Messages;
using GameFramework.Messaging;
using System.Collections.Generic;
using GameFramework.Facebook.Components;
using GameFramework.GameStructure.GameItems.ObjectModel;
using GameFramework.Display.Other;
using DG.Tweening;
using UnityEngine.Advertisements;

#if FACEBOOK_SDK
using GameFramework.Facebook.Components;
#endif

#if UNITY_ANALYTICS
using System.Collections.Generic;
using UnityEngine.Analytics;
using System.Security.Cryptography;
using System.Globalization;
using iSDK.Messenger.Scripts;
using GameFramework.Helper;
using GoogleMobileAds.Api;
#endif

/// <summary>
/// Base class for a game over dialog.
/// </summary>
[RequireComponent(typeof(DialogInstance))]
public class LevelCompleted : Singleton<LevelCompleted>
{
    public enum CopyType
    {
        None,
        Always,
        OnWin
    };

    [Header("General")]
    public string LocalisationBase = "GameOver";
    public int TimesPlayedBeforeRatingPrompt = -1;
    public bool ShowStars = false;
    public bool ShowTime = true;
    public bool ShowCoins = true;
    public bool ShowScore = true;
    public string ContinueScene = "Levels";

    [Header("Reward Handling")]
    [Tooltip("Specifies how the players overall score should be updated with the score obtained for the level.")]
    public CopyType UpdatePlayerScore = CopyType.None;
    [Tooltip("Specifies how the players overall coins should be updated with the coins obtained for the level.")]
    public CopyType UpdatePlayerCoins = CopyType.None;

    [Header("Tuning")]
    public float PeriodicUpdateDelay = 1f;

    protected DialogInstance DialogInstance;
    private GameObject shareView;
    private GameObject pointsView;
    private GameObject multiplayerView;
    bool _multiplayer;
    private string _shareImageUrl;
    private string _shareImageName;
    private bool _uploadingScreenshot;

    InterstitialAd interstitial;
    bool showingAd = false;
    private int Coins;

    protected override void GameSetup()
    {
        DialogInstance = GetComponent<DialogInstance>();

        Assert.IsNotNull(DialogInstance.Target, "Ensure that you have set the script execution order of dialog instance in settings (see help for details).");
    }

    protected override void GameDestroy()
    {
        base.GameDestroy();

        if (interstitial != null)
        {
            interstitial.Destroy();
        }

        if ( rewardBasedVideo != null ) {
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
        }
    }

    void Update()
    {
        if (DialogInstance.Target.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Continue();
        }
    }

    private void UnlockNextLevel()
    {
        var nextLevel = GameManager.Instance.Levels.GetNextItem();

        if (nextLevel == null)
        {
            return;
        }

        if (GameManager.Instance.LevelUnlockMode == GameItem.UnlockModeType.Custom &&
            nextLevel != null)
        {
            nextLevel.IsUnlocked = true;
            nextLevel.UpdatePlayerPrefs();

            // save any settings and preferences.
            PreferencesFactory.Save();
        }
    }

    public void ShowMultiplayer(ChallengeManager.GameStates gameState, ChallengeManager.GameStateMessage message, float time)
    {
        Show(gameState == ChallengeManager.GameStates.Won, time, message.Points, gameState, message);
    }

    static GameObject _inviteButtonObject;
	static GameObject _inviteOriginalParent;
	static Vector3 _inviteOriginalPosition;
    static GameObject _buttonPurchaseObject;
    public virtual void Show(bool isWon, float time, int points = 0, ChallengeManager.GameStates gameState = ChallengeManager.GameStates.Leaved, ChallengeManager.GameStateMessage message = null)
	{
		_multiplayer = GameSparksManager.Instance.GetGameMode() == GameMode.Multi;

		_buttonPurchaseObject = GameObject.Find("PurchaseButton");

		if (_buttonPurchaseObject != null && !_multiplayer)
		{
			_inviteOriginalParent = _buttonPurchaseObject.transform.parent.gameObject;
			_inviteOriginalPosition = _buttonPurchaseObject.transform.position;

            _inviteButtonObject = gameObject;
			DialogInstance _inviteDialogInstance = gameObject.GetComponent<DialogInstance>();

            GameObjectUtils.MoveObjectToAtIndex(_buttonPurchaseObject, _inviteDialogInstance.Target, 1);
		}

        pointsView = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "PointsView", true);
        shareView = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "SharingView", true);
        multiplayerView = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "MultiplayerPointsView", true);

        if (_multiplayer)
        {
            pointsView.SetActive(false);
            multiplayerView.SetActive(true);

            if ( gameState == ChallengeManager.GameStates.Won || gameState == ChallengeManager.GameStates.Draw ) {
                ButtonUtils.PlayWinSound();
            } else {
                ButtonUtils.PlayLoseSound();
            }
        }
        else
        {
            pointsView.SetActive(true);
            multiplayerView.SetActive(false);

            ButtonUtils.PlayWinSound();
        }

        var currentLevel = GameManager.Instance.Levels.Selected;

        int coins = 0;
        bool firstTimePlayed = _multiplayer ? true : currentLevel.ProgressBest < 0.9f;

        LevelManager.Instance.EndLevel();

        this.Coins = coins;

        if (firstTimePlayed && !_multiplayer)
        {
            currentLevel.AddPoints(points);
            currentLevel.ProgressBest = 1.0f;
        }

        if (_multiplayer && gameState == ChallengeManager.GameStates.Lost)
        {
            points *= -1;
        }

        GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Dialog", true), false);

        Assert.IsTrue(LevelManager.IsActive, "Ensure that you have a LevelManager component attached to your scene.");

        if (coins > 0 && !_multiplayer)
        { // add coins for this level
            currentLevel.AddCoins(coins);
            GameManager.Instance.Player.AddCoins(coins);
        }

        GameManager.Instance.Player.AddPoints(points);

        // update the player coins if necessary
        if (((UpdatePlayerCoins == CopyType.Always) || (UpdatePlayerCoins == CopyType.OnWin && isWon)) && !_multiplayer)
        {
            GameManager.Instance.Player.AddCoins(currentLevel.Coins);
        }

        // show won / lost game objects as appropriate
        GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Lost", true), !isWon);

        // see if the world or game is won and also if we should unlock the next world / level
        GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "GameWon", true), false);
        GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "WorldWon", true), false);
        GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "LevelWon", true), false);
        GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Won", true), false);

        GameObject levelName = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "LevelName", true);

        if (_multiplayer)
        {
            levelName.GetComponent<Text>().text = LocaliseText.Get("LevelCompleted.Match");
        }
        else
        {
            levelName.GetComponent<Text>().text = LocaliseText.Format("LevelCompleted.LevelName", currentLevel.Number);
        }

        GameObjectHelper.SafeSetActive(levelName, true);

        if (!_multiplayer && isWon)
        {
            GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Won", true), true);

            // process and update game state - do this last so we can check some bits above.
            GameHelper.ProcessCurrentLevelComplete();
        }

        // set some text based upon the result
        UIHelper.SetTextOnChildGameObject(DialogInstance.gameObject, "AchievementText", LocaliseText.Format(LocalisationBase + ".Achievement", currentLevel.Score, currentLevel.Name));

        // setup stars
        var starsGameObject = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Stars", true);
        GameObjectHelper.SafeSetActive(starsGameObject, ShowStars);
        if (ShowStars && !_multiplayer)
        {
            Assert.IsNotNull(starsGameObject, "GameOver->ShowStars is enabled, but could not find a 'Stars' gameobject. Disable the option or fix the structure.");
            starsGameObject.SetActive(ShowStars);
            var newStarsWon = GetNewStarsWon();
            currentLevel.StarsWon |= newStarsWon;
            var star1WonGameObject = GameObjectHelper.GetChildNamedGameObject(starsGameObject, "Star1", true);
            var star2WonGameObject = GameObjectHelper.GetChildNamedGameObject(starsGameObject, "Star2", true);
            var star3WonGameObject = GameObjectHelper.GetChildNamedGameObject(starsGameObject, "Star3", true);
            StarWon(currentLevel.StarsWon, newStarsWon, star1WonGameObject, 1, coins);
            StarWon(currentLevel.StarsWon, newStarsWon, star2WonGameObject, 2, coins);
            StarWon(currentLevel.StarsWon, newStarsWon, star3WonGameObject, 4, coins);
            GameObjectHelper.SafeSetActive(GameObjectHelper.GetChildNamedGameObject(starsGameObject, "StarWon", true), newStarsWon != 0);
        }

        // set time

        var timeGameObject = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Time", true);
        GameObjectHelper.SafeSetActive(timeGameObject, ShowTime);
        if (!_multiplayer && ShowTime)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);

            string timeText = LocaliseText.Format("LevelCompleted.Time", timeSpan.Minutes, timeSpan.Seconds);

            Assert.IsNotNull(timeGameObject, "GameOver->ShowTime is enabled, but could not find a 'Time' gameobject. Disable the option or fix the structure.");

            UIHelper.SetTextOnChildGameObject(timeGameObject, "TimeResult", timeText, true);
        }

        if (!_multiplayer && currentLevel.TimeBest < 0.05)
        { // save only first time played
            currentLevel.TimeBest = time;
        }

        // set coins
        if (ShowCoins && coins > 0)
        {
            var coinsGameObject = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Coins", true);
            GameObjectHelper.SafeSetActive(coinsGameObject, ShowCoins);

            Assert.IsNotNull(coinsGameObject, "GameOver->ShowCoins is enabled, but could not find a 'Coins' gameobject. Disable the option or fix the structure.");
            UIHelper.SetTextOnChildGameObject(coinsGameObject, "CoinsResult", coins.ToString(), true);
        }

        if (!_multiplayer)
        {
            if (firstTimePlayed)
            {
                GameObject DoubleButton = GameObjectHelper.GetChildNamedGameObject(pointsView, "DoubleButton", true);
                DoubleButton.SetActive(Reachability.Instance.IsReachable());
            }
            else
            {
                GameObject ShareButton = GameObjectHelper.GetChildNamedGameObject(pointsView, "ShareButton", true);
                ShareButton.SetActive(true);
            }
        }

        // set score
        var scoreGameObject = GameObjectHelper.GetChildNamedGameObject(_multiplayer ? multiplayerView : pointsView, "Score", true);
        GameObjectHelper.SafeSetActive(scoreGameObject, ShowScore);

        if (!firstTimePlayed)
        {
            GameObjectHelper.SafeSetActive(scoreGameObject, false);
        }

        if (!_multiplayer && firstTimePlayed)
        {
            GameObject adsObject = GameObjectHelper.GetChildNamedGameObject(pointsView, "Ads", true);
            Text unlockText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(pointsView, "UnlockText", true);

            if (LevelController.Instance.LastLevelInPack(currentLevel))
            {
                Pack pack = LevelController.Instance.PackForLevel(currentLevel);
                Pack nextPack = LevelController.Packs().GetItem(pack.Number + 1);

                if (nextPack)
                {
                    nextPack.LoadData();

                    adsObject.SetActive(false);
                    unlockText.gameObject.SetActive(true);

                    unlockText.text = LocaliseText.Format("LevelCompleted.LastLevelInPack", LocaliseText.Get(nextPack.JsonData.GetString("name")));
                }
            }

            if (LevelController.Instance.LastLevelInRank(currentLevel))
            {
                Rank rank = LevelController.Instance.RankForLevel(currentLevel);
                Rank nextRank = LevelController.Ranks().GetItem(rank.Number + 1);

                if (nextRank)
                {
                    nextRank.LoadData();

                    adsObject.SetActive(false);
                    unlockText.gameObject.SetActive(true);

                    unlockText.text = LocaliseText.Format("LevelCompleted.LastLevelInRank", LocaliseText.Get(nextRank.JsonData.GetString("name")));
                }
            }
        }

        if (ShowScore)
        {
            Assert.IsNotNull(scoreGameObject, "GameOver->ShowScore is enabled, but could not find a 'Score' gameobject. Disable the option or fix the structure.");

            UIHelper.SetTextOnChildGameObject(scoreGameObject, "ScoreResult", LocaliseText.Format("LevelCompleted.Score", "0"), true);

            AnimateScoreText(points);
        }

        if (_multiplayer)
        {
            var resultStateObject = GameObjectHelper.GetChildNamedGameObject(multiplayerView, "ResultState", true);
            GameObjectHelper.SafeSetActive(resultStateObject, true);

            string text = "";

            switch (gameState)
            {
                case ChallengeManager.GameStates.Won:
                    text = LocaliseText.Get("Game.YouWon");
                    break;
                case ChallengeManager.GameStates.Lost:
                    text = LocaliseText.Get("Game.YouLost");
                    break;
                case ChallengeManager.GameStates.Draw:
                    text = LocaliseText.Get("Game.ItsDrawn");
                    break;
            }

            resultStateObject.GetComponent<Text>().text = text;
        }

        if (!_multiplayer)
        {
            UpdateNeededCoins();

            LevelController.Instance.PackProgressCompleted(currentLevel);
            UnlockNextLevel();

            //

            int StartupLevels = ((CustomGameManager)CustomGameManager.Instance).StartupLevels;

            if (StartupLevels == currentLevel.Number)
            {
                GameObject adsObject = GameObjectHelper.GetChildNamedGameObject(pointsView, "Ads", true);

                Text adsText = adsObject.GetComponent<Text>();

                adsText.text = LocaliseText.Get("Text.PlayedAllLevels");
                adsText.fontSize = 39;
                adsText.resizeTextForBestFit = false;

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("Game_LastLevel", new Dictionary<string, string>() { { "Level", currentLevel.Number.ToString() } });
                    Fabric.Answers.Answers.LogCustom("Game_LastLevel", new Dictionary<string, object>() { { "Level", currentLevel.Number.ToString() } });
                }
            }
        }

        if (!_multiplayer && firstTimePlayed)
        {
            JSONObject pointsData = new JSONObject();
            pointsData.Add("Level", currentLevel.Number.ToString());
            pointsData.Add("Language", LocaliseText.Language);
            pointsData.Add("RealLanguage", LanguageUtils.RealLanguage(LocaliseText.Language));
            pointsData.Add("Time", time.ToString());
            pointsData.Add("UsedHints", GameController.Instance.usedHintsCount);
            pointsData.Add("UserCoins", GameManager.Instance.Player.Coins);
            pointsData.Add("Words", JsonUtils.ListToArray(GameController.Instance.GetFoundWords()));
            pointsData.Add("Date", DateTimeUtils.DateTimeToISO8601(UnbiasedTime.Instance.UTCNow()));
            
            GameSparksManager.Instance.SendPoints(points, "LevelComplete", pointsData);
        }

        // 

        GameObject completeTextGameObject = GameObjectHelper.GetChildNamedGameObject(_multiplayer ? multiplayerView : pointsView, "CompleteText", true);

        // save game state.
        GameManager.Instance.Player.UpdatePlayerPrefs();

        if (!_multiplayer)
        {
            currentLevel.UpdatePlayerPrefs();
        }

        PreferencesFactory.Save();

        GameObject NameContainer = GameObjectHelper.GetChildNamedGameObject(DialogInstance.Content, "NameContainer", true);
        GameObject LevelName = GameObjectHelper.GetChildNamedGameObject(DialogInstance.Content, "LevelName", true);
        GameObject Results = GameObjectHelper.GetChildNamedGameObject(DialogInstance.Content, "Results", true);
        GameObject Buttons = GameObjectHelper.GetChildNamedGameObject(DialogInstance.Content, "Buttons", true);
        GameObject CloseButton = GameObjectHelper.GetChildNamedGameObject(DialogInstance.Content, "Close", true);

        GameObject parent = DialogInstance.Content.transform.parent.gameObject;

        Vector3 currentScale = parent.transform.localScale;

        parent.transform.DOScale(new Vector3(0, 0, 0), 0.0f);
        NameContainer.transform.localScale = new Vector3(0, 0, 0);
        LevelName.transform.localScale = new Vector3(0, 0, 0);
        Results.transform.localScale = new Vector3(0, 0, 0);
        Buttons.transform.localScale = new Vector3(0, 0, 0);
        completeTextGameObject.transform.localScale = new Vector3(0, 0, 0);

        CloseButton.GetComponent<Image>().color = new Color(1, 1, 1, 0);

        //show dialog
        DialogInstance.Show();

        parent.transform.DOScale(currentScale, 1.0f).SetEase(Ease.OutElastic);
        NameContainer.transform.DOScale(new Vector3(1, 1, 1), 1.5f).SetDelay(0.1f).SetEase(Ease.OutElastic);
        LevelName.transform.DOScale(new Vector3(1, 1, 1), 0.8f).SetDelay(0.2f).SetEase(Ease.OutElastic);
        Results.transform.DOScale(new Vector3(1, 1, 1), 0.8f).SetDelay(0.2f).SetEase(Ease.OutElastic);
        Buttons.transform.DOScale(new Vector3(1, 1, 1), 0.8f).SetDelay(0.2f).SetEase(Ease.OutElastic);
        completeTextGameObject.transform.DOScale(new Vector3(1, 1, 1), 0.8f).SetDelay(0.35f).SetEase(Ease.OutElastic);

        CloseButton.GetComponent<Image>().DOFade(1, 0.5f).SetDelay(0.7f);

        GameObject Light = GameObjectHelper.GetChildNamedGameObject(completeTextGameObject, "Light", true);

        Light.transform.DOLocalRotate(new Vector3(0, 0, -360), 10, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear);

        //TODO bug - as we increase TimesPlayedForRatingPrompt on both game start (GameManager) and level finish we can miss this comparison.
        if (GameManager.Instance.TimesPlayedForRatingPrompt == TimesPlayedBeforeRatingPrompt)
        {
            GameFeedback gameFeedback = new GameFeedback();
            gameFeedback.GameFeedbackAssumeTheyLikeOptional();
        }

#if UNITY_ANALYTICS
        // record some analytics on the level played
        if (!_multiplayer)
        {
            var values = new Dictionary<string, object>
            {
            { "score", currentLevel.Score },
            { "Coins", coins },
            { "time", time },
            { "level", currentLevel.Number }
            };
            Analytics.CustomEvent("LevelCompleted", values);
        }
#endif

#if UNITY_EDITOR
        if (!_multiplayer)
        {
            GameSparksManager.Instance.SyncProgressCoroutine();
        }
#endif

#if !UNITY_EDITOR
        AdColonyManager.Instance.RequestAd();
        AdColonyManager.Instance.RequestAd(Constants.AdColonyDoubleCoins);

        LoadInterstitialAd();
        LoadAdmobRewarderVideo();
#endif

		// co routine to periodic updates of display (don't need to do this every frame)
		if (!Mathf.Approximately(PeriodicUpdateDelay, 0))
			StartCoroutine(PeriodicUpdate());
	}

	IEnumerator StartCoinsParticle(GameObject particle, Vector3 position, float delay) {
		yield return new WaitForSeconds (delay);

		particle.transform.position = new Vector3(position.x, position.y, particle.transform.position.z);
		ParticleSystem p = particle.GetComponent<ParticleSystem> ();
		p.Play ();

		AudioClip audio = Resources.Load<AudioClip> ("Audio/Coins");
		GameManager.Instance.PlayEffect (audio);
	}

	void StarWon(int starsWon, int newStarsWon, GameObject starGameObject, int bitMask, int coins = 0)
	{
//		bool giveCoins = LevelController.StarGiveCoins (GameManager.Instance.Levels.Selected, bitMask);
//
//		if ( coins == 0 ) { // when no one coin is won (ex: first time level with 1 star, user play again, win 2 stars, then no one coin is won)
//			giveCoins = false;
//		}

		bool giveCoins = true;

		// default state
		GameObjectHelper.GetChildNamedGameObject(starGameObject, "NotWon", true).SetActive(true);
		GameObject Won = GameObjectHelper.GetChildNamedGameObject (starGameObject, "Won", true);
		Won.SetActive ((starsWon & bitMask) == bitMask);

		// if just won then animate
		if ((newStarsWon & bitMask) == bitMask)
		{
			var animation = starGameObject.GetComponent<UnityEngine.Animation>();
			if (animation != null)
				animation.Play();

			Won.GetComponent <Image>().color = new Color(1, 1, 1, 0);

			StartCoroutine (DelayStarWonAnimation(Won, bitMask, giveCoins));
		}
	}

	IEnumerator DelayStarWonAnimation(GameObject Won, int bitMask, bool giveCoins) {
		float delay = 1.0f;

		if ( bitMask == 2) {
			delay = 1.5f;
		}

		if ( bitMask == 4 ) {
			delay = 2.0f;
		}

		yield return new WaitForSeconds (delay);

		var starsGameObject = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "Stars", true);
		var particle = GameObjectHelper.GetChildNamedGameObject (starsGameObject, "CoinsParticle", true);

		Animation anim = Won.GetComponent <Animation> ();
		anim.Play ();

		AudioClip audio = Resources.Load<AudioClip> ("Audio/Star");
		GameManager.Instance.PlayEffect (audio);

		if (giveCoins) {
			StartCoroutine (StartCoinsParticle (particle, Won.transform.parent.position, 0.25f));
		}
	}

	public virtual IEnumerator PeriodicUpdate()
	{
		while (true)
		{
			UpdateNeededCoins();

			yield return new WaitForSeconds(PeriodicUpdateDelay);
		}
	}

	/// <summary>
	/// If LevelManager is in use then we return the difference between stars that were recorded at the start and those that are recorded now.
	/// 
	/// You may also override this function if you wish to provide your own handling such as allocating stars only on completion.
	/// </summary>
	/// <returns></returns>
	public virtual int GetNewStarsWon()
	{
		if (LevelManager.IsActive && LevelManager.Instance.Level != null)
		{
			return LevelManager.Instance.Level.StarsWon - LevelManager.Instance.StartStarsWon;
		}

		return 0;
	}

	public void UpdateNeededCoins()
	{
		int minimumCoins = GameManager.Instance.Levels.ExtraValueNeededToUnlock(GameManager.Instance.Player.Coins);
		var targetCoinsGameobject = GameObjectHelper.GetChildNamedGameObject(DialogInstance.gameObject, "TargetCoins", true);
		if (targetCoinsGameobject != null)
		{
			if (minimumCoins == 0)
				UIHelper.SetTextOnChildGameObject(DialogInstance.gameObject, "TargetCoins",
					LocaliseText.Format(LocalisationBase + ".TargetCoinsGot", minimumCoins), true);
			else if (minimumCoins > 0)
				UIHelper.SetTextOnChildGameObject(DialogInstance.gameObject, "TargetCoins",
					LocaliseText.Format(LocalisationBase + ".TargetCoins", minimumCoins), true);
			else
				targetCoinsGameobject.SetActive(false);
		}
	}

	public void ShowPointsScreen() {
		Text nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text> (DialogInstance.gameObject, "LevelName", true);

		if (_multiplayer) {
            nameText.text = LocaliseText.Get("LevelCompleted.Match");
			multiplayerView.SetActive (true);
		} else {
			Level currentLevel = GameManager.Instance.Levels.Selected;

			nameText.text = LocaliseText.Format ("LevelCompleted.LevelName", currentLevel.Number);
			pointsView.SetActive (true);
		}

		shareView.SetActive (false);
	}

	public void ShowShareScreen() {
		Text nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text> (DialogInstance.gameObject, "LevelName", true);
        nameText.text = LocaliseText.Get("LevelCompleted.TellYourFriends");

		multiplayerView.SetActive (false);
		pointsView.SetActive (false);
		shareView.SetActive (true);
	}

	public void Rematch() {
		GameController.Instance.Rematch ();

		DialogInstance.DoneFinished ();
	}

	public void CopyLinkShareButtonHandler() {
        CopyLinkShare();
	}

	public void CopyLinkShare()
	{
        InviteController.Instance.CopyLink();
	}

	public void MessengerShareButtonHandler() {
        MessengerShareButton();
	}

	public void MessengerShareButton()
	{
        InviteController.Instance.MessengerInvite();
	}

    public void NativeShareButtonHandler() {
        NativeShare();
    }

    void NativeShare() {
        InviteController.Instance.NativeInvite();
    }

	public void FacebookShareButtonHandler() {
        FacebookShare();
	}

	public void FacebookShare()
	{
#if FACEBOOK_SDK
		GameManager.SafeAddListener<FacebookShareLinkMessage> (OnShareLinkHandler);

        InviteController.Instance.FacebookInvite();
#endif
	}

    public void FacebookLogin()
    {
        SettingsContainer.Instance.FacebookLogin();
    }

#if FACEBOOK_SDK
	void PublishPermissionCallback(ILoginResult result)
	{
		foreach (string permission in AccessToken.CurrentAccessToken.Permissions)
		{
			FacebookManager.Instance.PermissionsGranted.Add(permission);
		}

		FacebookShare ();
	}

	bool OnShareLinkHandler(BaseMessage message) {
		GameManager.SafeRemoveListener<FacebookShareLinkMessage> (OnShareLinkHandler);

		FacebookShareLinkMessage msg = message as FacebookShareLinkMessage;

		if (msg.Result == FacebookShareLinkMessage.ResultType.OK) {
			if (InviteController.InviteBonusCoins ()) {
				StartCoroutine (CoRoutines.DelayedCallback (2f, Continue));
			} else {
				Continue ();
			}
		} else {
			Continue ();
		}

		return true;
	}

	bool FacebookShareLoginHandler(BaseMessage message) {
		GameManager.SafeRemoveListener<FacebookLoginMessage>(FacebookShareLoginHandler);

		FacebookShare ();

		return true;
	}
#endif

	public void CloseButtonHandler() {
        Close();
	}

	public void Close() {
		if ( shareView != null && shareView.activeSelf ) {
			ShowPointsScreen ();
			return;
		}

		if (PreferencesFactory.GetInt(Constants.KeyNoAds, 0) == 1)
		{ // no ads
			CloseProcess();
			return;
		}

		int CountClicks = PreferencesFactory.GetInt(Constants.KeyCountCloseLevelClicks);
		PreferencesFactory.SetInt(Constants.KeyCountCloseLevelClicks, CountClicks + 1);

  //      if (CountClicks > 0 && CountClicks % Constants.ShowAdsPerTime == 0 && Reachability.Instance.IsReachable())
		//{
		//	Loading.Instance.Show();

		//	AdColonyManager.Instance.SetCallback(BannerCloseClosed);
		//	AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());
		//}
		//else
		//{
			CloseProcess();
		//}
	}

    void CloseProcess()
    {
        // move back Purchase button
        if (_inviteButtonObject != null)
        {
            _inviteButtonObject = null;
            GameObjectUtils.MoveObjectTo(_buttonPurchaseObject, _inviteOriginalParent, _inviteOriginalPosition);
        }

		DialogInstance.DoneFinished();

		if (_multiplayer)
		{
			GameManager.LoadSceneWithTransitions("Lobby");
		}
		else
		{
			BackToLevels();
		}
    }

	public void HandleOnAdClosed(object sender, EventArgs args) {
		ContinueProcess ();
	}

	public void ContinueButtonHandler () {
        Continue();
	}

	public void Continue ()
	{
        // move back Purchase button
        if (_inviteButtonObject != null)
        {
            _inviteButtonObject = null;
            GameObjectUtils.MoveObjectTo(_buttonPurchaseObject, _inviteOriginalParent, _inviteOriginalPosition);
        }

		DialogInstance.DoneFinished ();

		if ( PreferencesFactory.GetInt (Constants.KeyNoAds, 0) == 1 ) { // no ads
			ContinueProcess ();
			return;
		}

#if UNITY_EDITOR
		ContinueProcess ();
		return;
#endif

		int CountClicks = PreferencesFactory.GetInt(Constants.KeyCountNextLevelClicks);
		PreferencesFactory.SetInt(Constants.KeyCountNextLevelClicks, CountClicks + 1);

        Level level = GameManager.Instance.Levels.Selected;

        if (level.Number > 10 && level.Number % Constants.ShowAdsPerTime == 0 && Reachability.Instance.IsReachable())
		{
            int rnd = UnityEngine.Random.Range(1, 10);

            if (rnd < 6 && interstitial != null && interstitial.IsLoaded())
            {
                Loading.Instance.Show();
                interstitial.Show();
                return;
            }

			Loading.Instance.Show();

			AdColonyManager.Instance.SetCallback(BannerClosed);
			AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());
		}
		else
		{
			ContinueProcess();
		}
	}

	void BannerClosedProcess(int amount, bool success) {
		Loading.Instance.Hide ();
		ContinueProcess ();
	}

	void BannerCloseClosedProcess(int amount, bool success)
	{
		Loading.Instance.Hide();
		CloseProcess();
	}

	void BannerClosed(string zoneId, int amount, bool success) {
		if ( !success ) {
			ShowVideoAd (BannerClosedProcess);
			return;
		}

		Loading.Instance.Hide ();
		ContinueProcess ();
	}

	void BannerCloseClosed(string zoneId, int amount, bool success)
	{
		if (!success)
		{
			ShowVideoAd(BannerCloseClosedProcess);
			return;
		}

		Loading.Instance.Hide();
		CloseProcess();
	}

	public void ShowVideoAd(Action<int, bool> action)
	{
		if (Advertisement.IsReady("video"))
		{
			var options = new ShowOptions
			{
				resultCallback = (ShowResult result) =>
				{
					action(0, result == ShowResult.Finished);
				}
			};
			Advertisement.Show("video", options);
		}
		else
		{
			action(0, false);
		}
	}

    void BackToLevels() {
        GameManager.LoadSceneWithTransitions(ContinueScene);
    }

	void ContinueProcess() {
		System.GC.Collect ();

		if ( GameManager.Instance.Levels != null ) {
			Level nextLevel = GameManager.Instance.Levels.GetNextItem ();

			// no more levels
			if ( nextLevel == null ) {
				BackToLevels ();
				return;
			}

			Pack nextLevelPack = LevelController.Instance.PackForLevel (nextLevel);

			if ( nextLevelPack != null && !nextLevelPack.IsUnlocked ) {
				BackToLevels();
				return;
			}

			GameManager.Instance.Levels.Selected = nextLevel;
			PreferencesFactory.Save();

			GameController.Instance.NextLevel ();

			return;
		}

		BackToLevels();
	}

	public void Retry()
	{
		DialogInstance.DoneFinished ();

		if (GameManager.Instance.Player.Lives <= 0) {
			BackToLevels();
			return;
		}

		var sceneName = !string.IsNullOrEmpty(GameManager.Instance.IdentifierBase) && SceneManager.GetActiveScene().name.StartsWith(GameManager.Instance.IdentifierBase + "-") ? SceneManager.GetActiveScene().name.Substring((GameManager.Instance.IdentifierBase + "-").Length) : SceneManager.GetActiveScene().name;
		GameManager.LoadSceneWithTransitions(sceneName);
	}

	public void DoubleCoinsButtonHandler() {
        DoubleCoins();
	}

	public void DoubleCoins() {
		if ( !Reachability.Instance.IsReachable () ) {
			DialogManager.Instance.Show(titleKey: "GeneralMessage.Info.Title",
				textKey: "GeneralMessage.NoInternet");
			return;
		}

#if UNITY_EDITOR
		CompleteReward ();
		return;
#endif

		Loading.Instance.Show ();

        if (rewardBasedVideo != null && rewardBasedVideo.IsLoaded())
        {
            showingAd = true;
            rewardBasedVideo.Show();
        }
        else
        {
            AdColonyManager.Instance.SetCallback(CloseDoubleCoins);
            AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId(Constants.AdColonyDoubleCoins));
        }

		if (!Debug.isDebugBuild) {
			Flurry.Flurry.Instance.LogEvent ("ExtraCoins_EndLevel");
			Fabric.Answers.Answers.LogCustom ("ExtraCoins_EndLevel");
		}
	}

	void CloseDoubleCoins(string zoneId, int amount, bool success) {
		if ( !success ) {
			ShowRewardedAd ();
			return;
		}

		Loading.Instance.Hide ();

		if ( zoneId == null ) {
			return;
		}

		CompleteReward ();
	}

	public void ShowRewardedAd()
	{
		if (Advertisement.IsReady("rewardedVideo"))
		{
			var options = new ShowOptions
			{
				resultCallback = (ShowResult result) =>
				{
					RewardedAdComplete(2, result == ShowResult.Finished);
				}
			};
			Advertisement.Show("rewardedVideo", options);
		}
		else
		{
			RewardedAdComplete(0, false);
		}
	}

    void LoadInterstitialAd() {
        interstitial = new InterstitialAd(Constants.AdMobUnitIdInterstitial);
        interstitial.OnAdFailedToLoad += (object sender, AdFailedToLoadEventArgs e) => {

        };

        interstitial.OnAdClosed += (object sender, EventArgs e) => {
            QueueActionsManager.Instance.AddAction(() => {
                Loading.Instance.Hide();
                ContinueProcess();
            });
        };

        AdRequest request = new AdRequest.Builder().Build();

        interstitial.LoadAd(request);
    }

    private RewardBasedVideoAd rewardBasedVideo;
    void LoadAdmobRewarderVideo() {
        if (rewardBasedVideo == null)
        {
            rewardBasedVideo = RewardBasedVideoAd.Instance;

            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        }

        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(request, Constants.AdMobUnitIdRewardedVideo);
    }

    void HandleRewardBasedVideoClosed(object sender, EventArgs args) {
        QueueActionsManager.Instance.AddAction(() => {
            if (showingAd)
            {
                showingAd = false;

                Loading.Instance.Hide();
                ContinueProcess();
            }
        });
    }

    void HandleRewardBasedVideoRewarded(object sender, Reward args) {
        string type = args.Type;
        double amount = args.Amount;

        QueueActionsManager.Instance.AddAction(() => {
            if (showingAd)
            {
                Debug.Log("LevelCompleted: args.Type: " + args.Type + "; args.Amount: " + args.Amount);

                RewardedAdComplete(2, true);
            }
        });
    }

	void RewardedAdComplete(int amount, bool success) {
		Loading.Instance.Hide ();

		if ( success ) {
			CompleteReward ();
		}
	}

	void CompleteReward() {
		var currentLevel = GameManager.Instance.Levels.Selected;
		var points = currentLevel.Score;

		currentLevel.AddPoints (points);

		GameManager.Instance.Player.AddPoints (points);

		GameManager.Instance.Player.UpdatePlayerPrefs();
		currentLevel.UpdatePlayerPrefs();
		PreferencesFactory.Save();

		UIHelper.SetTextOnChildGameObject(pointsView, "ScoreResult", LocaliseText.Format("LevelCompleted.ScoreDouble", points), true);

		GameObject DoubleButton = GameObjectHelper.GetChildNamedGameObject(pointsView, "DoubleButton", true);
		DoubleButton.SetActive (false);

		GameObject AdsObject = GameObjectHelper.GetChildNamedGameObject(pointsView, "Ads", true);
		AdsObject.SetActive (false);

        GameObject unlockTextObject = GameObjectHelper.GetChildNamedGameObject(pointsView, "UnlockText", true);
        unlockTextObject.SetActive(false);

		GameObject ShareButton = GameObjectHelper.GetChildNamedGameObject(pointsView, "ShareButton", true);
		ShareButton.SetActive (true);

		if ( !_multiplayer ) {
            JSONObject pointsData = new JSONObject();
            pointsData.Add("Level", currentLevel.Number.ToString());
            pointsData.Add("Language", LocaliseText.Language);
            pointsData.Add("RealLanguage", LanguageUtils.RealLanguage(LocaliseText.Language));
            pointsData.Add("Words", JsonUtils.ListToArray(GameController.Instance.GetFoundWords()));
            pointsData.Add("Date", DateTimeUtils.DateTimeToISO8601(UnbiasedTime.Instance.UTCNow()));
            
            GameSparksManager.Instance.SendPoints (points, "LevelComplete-Double", pointsData);
		}
	}

    void AnimateScoreText(int _points) {
		Text _scoreText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(_multiplayer ? multiplayerView : pointsView, "ScoreResult", true);

        if (!_scoreText)
        {
            return;
        }

        int points = 0;
        DOTween.To(() => points, x => points = x, _points, 1.25f).SetEase(Ease.Linear).OnUpdate(() => {
            if ( !_scoreText ) {
                return;
            }

            string distanceText;
			if (points == 1 || points == -1)
			{
				distanceText = LocaliseText.Format("LevelCompleted.ScoreOne", points.ToString());
			}
			else
			{
				distanceText = LocaliseText.Format("LevelCompleted.Score", points.ToString());
			}

			_scoreText.text = distanceText;
        });
    }
}
