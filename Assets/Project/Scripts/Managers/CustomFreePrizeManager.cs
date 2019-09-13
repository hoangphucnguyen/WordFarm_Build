using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.FreePrize.Components;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameStructure;
using GameFramework.Localisation;
using System;
using GameFramework.GameObjects.Components;
using GameFramework.Preferences;
using System.Globalization;
using System.Linq;
using GameFramework.GameObjects;
using UnityEngine.UI;
using GameFramework.UI.Other;
using UnityEngine.Advertisements;
using GameFramework.Localisation.Messages;
using GameFramework.Messaging;
using GoogleMobileAds.Api;

public class CustomFreePrizeManager : SingletonPersistant<CustomFreePrizeManager>
{
    /// <summary>
    /// The delay in seconds before starting the next countdown. 0 = no wait
    /// </summary>
    [Header("Time")]
    [Tooltip("The delay in seconds before starting the next countdown. 0 = no wait")]
    public MinMax DelayRangeToNextCountdown = new MinMax { Min = 0, Max = 0 };       // wait range before starting next countdown. 0 = no wait

    /// <summary>
    /// The time in seconds before another prize becomes available.
    /// </summary>
    [Tooltip("The time in seconds before another prize becomes available.")]
    public MinMax TimeRangeToNextPrize = new MinMax { Min = 600, Max = 1800 };    // countdown range to next prize. 10 minutes to 30 minutes

    /// <summary>
    /// Whether to save times for the next prize becoming available across game restarts.
    /// </summary>
    [Tooltip("Whether to save times for the next prize becoming available across game restarts.")]
    public bool SaveAcrossRestarts;


    /// <summary>
    /// A minimum and maxximum value for the prize.
    /// </summary>
    [Header("Prize")]
    [Tooltip("A minimum and maxximum value for the prize.")]
    public MinMax ValueRange = new MinMax { Min = 10, Max = 20 };                // value defaults


    /// <summary>
    /// An optional audio clip to play when the free prize window is closed.
    /// </summary>
    [Header("Free Prize Dialog")]
    [Tooltip("An optional audio clip to play when the free prize window is closed.")]
    public AudioClip PrizeDialogClosedAudioClip;

    /// <summary>
    /// An optional prefab to use for displaying custom content in the free prize window.
    /// </summary>
    [Tooltip("An optional prefab to use for displaying custom content in the free prize window.")]
    public GameObject ContentPrefab;

    /// <summary>
    /// An optional animation controller to animate the free prize window content.
    /// </summary>
    [Tooltip("An optional animation controller to animate the free prize window content.")]
    public RuntimeAnimatorController ContentAnimatorController;

    /// <summary>
    /// Whether the content shows the dialog buttons. Setting this hides the dialog buttons so that they can be displayed at the appropriate point e.g. after an animation has played.
    /// </summary>
    [Tooltip("Whether the content shows the dialog buttons. Setting this hides the dialog buttons so that they can be displayed at the appropriate point e.g. after an animation has played.")]
    public bool ContentShowsButtons;

    /// <summary>
    /// DateTime then the next free prize countdown should start
    /// </summary>
    public DateTime NextCountdownStart { get; set; }

    /// <summary>
    /// DateTime then the free prize will become available
    /// </summary>
    public DateTime NextFreePrizeAvailable { get; set; }

    /// <summary>
    /// True when teh Free Prize Dialog is shown otherwise false
    /// </summary>
    public bool IsShowingFreePrizeDialog { get; set; }

    /// <summary>
    /// The current prize amount.
    /// </summary>
    public int CurrentPrizeAmount { get; set; }

    private int PrizeItems = 0;
    private bool prizeIsCoins = true;
    private bool prizeIsPoints = false;
    static int[] _notificationDays = new int[5] { 1, 3, 7, 10, 30 };
    private bool _prizeIsProcessed = false;
    private bool showingAd = false;

    private DialogInstance dialogInstance;

    /// <summary>
    /// Called from singletong Awake() - Load saved prize times, or setup new if first run or not saving across restarts
    /// </summary>
    protected override void GameSetup()
    {
        base.GameSetup();

        if (SaveAcrossRestarts && PreferencesFactory.HasKey("FreePrize.NextCountdownStart"))
        {
            NextCountdownStart = DateTime.Parse(PreferencesFactory.GetString("FreePrize.NextCountdownStart", UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture))); // start countdown immediately if new game
            NextFreePrizeAvailable = DateTime.Parse(PreferencesFactory.GetString("FreePrize.NextPrize", NextFreePrizeAvailable.ToString(CultureInfo.InvariantCulture)));
            SetCurrentPrizeAmount();
        }
        else
        {
            StartNewCountdown();
            SetCurrentPrizeAmount();
        }

        GameManager.SafeAddListener<UserNotificationsChangedMessage>(UserNotificationsChangedHandler);
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);

        //

#if !UNITY_EDITOR
		CancelLocalNotifications ();
		RegisterLocalNotifications ();
#endif
    }

    protected override void GameDestroy()
    {
        base.GameDestroy();

        GameManager.SafeRemoveListener<UserNotificationsChangedMessage>(UserNotificationsChangedHandler);
        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);

        if ( rewardBasedVideo!= null ) {
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
        }
    }

    bool LocalisationHandler(BaseMessage message)
    {
#if !UNITY_EDITOR
        CancelLocalNotifications ();
        RegisterLocalNotifications ();
#endif
		return true;
    }

    /// <summary>
    /// Save the current state including free prize times
    /// </summary>
    public override void SaveState()
    {
        PreferencesFactory.SetString("FreePrize.NextCountdownStart", NextCountdownStart.ToString(CultureInfo.InvariantCulture));
        PreferencesFactory.SetString("FreePrize.NextPrize", NextFreePrizeAvailable.ToString(CultureInfo.InvariantCulture));
        PreferencesFactory.Save();
    }

    /// <summary>
    /// Make the free prize immediately available
    /// </summary>
    public void MakePrizeAvailable()
    {
        NextCountdownStart = UnbiasedTime.Instance.Now();
        NextFreePrizeAvailable = UnbiasedTime.Instance.Now();

        SaveState();
    }

    /// <summary>
    /// Recalculate new times for a new countdown
    /// </summary>
    public void StartNewCountdown()
    {
        float seconds = DateTimeUtils.SecondsTillMidnight();

        NextCountdownStart = UnbiasedTime.Instance.Now().AddSeconds(UnityEngine.Random.Range(DelayRangeToNextCountdown.Min, DelayRangeToNextCountdown.Max + 1));
        NextFreePrizeAvailable = NextCountdownStart.AddSeconds(seconds);

        SaveState();
    }

    /// <summary>
    /// Set the current prize amount
    /// </summary>
    void SetCurrentPrizeAmount()
    {
        
    }

    /// <summary>
    /// Returns whether a countdown is taking place. If waiting for the countdown to begin or a Free Prize is available then this will return false
    /// </summary>
    /// <returns></returns>
    public bool IsCountingDown()
    {
        return GetTimeToPrize().TotalSeconds > 0 && GetTimeToCountdown().TotalSeconds <= 0;
    }

    /// <summary>
    /// Returns whether a prize is available
    /// </summary>
    /// <returns></returns>
    public bool IsPrizeAvailable()
    {
        return GetTimeToPrize().TotalSeconds <= 0;
    }

    /// <summary>
    /// Returns the time until the next countdown will start.
    /// </summary>
    /// <returns></returns>
    TimeSpan GetTimeToCountdown()
    {
        return NextCountdownStart.Subtract(UnbiasedTime.Instance.Now());
    }

    /// <summary>
    /// Returns the time until the next free prize will be available.
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetTimeToPrize()
    {
        return NextFreePrizeAvailable.Subtract(UnbiasedTime.Instance.Now());
    }

    public void DoubleCoins()
    {
        if (!Reachability.Instance.IsReachable())
        {
            DialogManager.Instance.Show(titleKey: "GeneralMessage.Info.Title",
                textKey: "GeneralMessage.NoInternet");
            return;
        }

#if UNITY_EDITOR
        CompleteReward();
        return;
#endif

        Loading.Instance.Show();

        if (rewardBasedVideo != null && rewardBasedVideo.IsLoaded())
        {
            showingAd = true;
            rewardBasedVideo.Show();
        }
        else
        {
            AdColonyManager.Instance.SetCallback(CloseDoubleCoins);
            AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId(Constants.AdColonyDoubleDailyBonus));
        }

        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("ExtraCoins_DailyBonusDouble");
            Fabric.Answers.Answers.LogCustom("ExtraCoins_DailyBonusDouble");
        }
    }

    private RewardBasedVideoAd rewardBasedVideo;
    void LoadAdmobRewarderVideo()
    {
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

    void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        QueueActionsManager.Instance.AddAction(() => {
            if (showingAd)
            {
                showingAd = false;
                Loading.Instance.Hide();
            }
        });
    }

    void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        QueueActionsManager.Instance.AddAction(() => {
            if (showingAd)
            {
                Debug.Log("CustomFreePrize: args.Type: " + args.Type + "; args.Amount: " + args.Amount);
                RewardedAdComplete(0, true);
            }
        });
    }

    void CloseDoubleCoins(string zoneId, int amount, bool success)
    {
        if (!success)
        {
            ShowRewardedAd();

            return;
        }

        Loading.Instance.Hide();

        if (zoneId == null || !zoneId.Equals(Constants.AdColonyDoubleDailyBonus))
        {
            return;
        }

        CompleteReward();
    }

    void CompleteReward()
    {
        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("ExtraCoins_DailyBonus");
            Fabric.Answers.Answers.LogCustom("ExtraCoins_DailyBonus");
        }

        if (dialogInstance != null && dialogInstance.gameObject != null && this.PrizeItems > 0)
        {
            GameObject part2 = GameObjectHelper.GetChildNamedGameObject(dialogInstance.Content, "Part2", true);

            Text rewardsCoinsText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(part2, "RewardsCoins", true);

            if (prizeIsPoints)
            {
                rewardsCoinsText.text = LocaliseText.Format("FreePrize.NumberMorePoints", this.PrizeItems);
            }

            if ( prizeIsCoins ) 
            {
                rewardsCoinsText.text = LocaliseText.Format("FreePrize.NumberMoreCoins", this.PrizeItems);
            }

            GameObject WatchAds = GameObjectHelper.GetChildNamedGameObject(part2, "WatchAds", true);
            WatchAds.SetActive(false);

            GameObject DoubleButton = GameObjectHelper.GetChildNamedGameObject(part2, "DoubleButton", true);
            DoubleButton.SetActive(false);

            if (prizeIsCoins && this.PrizeItems > 0)
            {
                GameObject animatedCoins = GameObject.Find("AddCoinsAnimated");

                GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
                AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

                addCoins.AnimateCoinsAdding(this.PrizeItems);
            }

            if (prizeIsPoints)
            {
                GameObject animatedPoints = GameObject.Find("AddPointsAnimated");

                GameObject _clone = Instantiate(animatedPoints, animatedPoints.transform.parent);
                AddPointsAnimated _add = _clone.GetComponent<AddPointsAnimated>();

				_add.AnimateAdding(this.PrizeItems);

                GameSparksManager.Instance.SendPoints(this.PrizeItems, "FreePrize-Double");
            }
        }
    }

    public void ShowRewardedAd()
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions
            {
                resultCallback = (ShowResult result) =>
                {
                    RewardedAdComplete(0, result == ShowResult.Finished);
                }
            };
            Advertisement.Show("rewardedVideo", options);
        }
        else
        {
            RewardedAdComplete(0, false);
        }
    }

    void RewardedAdComplete(int amount, bool success)
    {
        Loading.Instance.Hide();

        if (success)
        {
            CompleteReward();
        }
    }

    string BonusText()
    {
        string text = LocaliseText.Format("FreePrize.Text1", this.PrizeItems);

        return text;
    }

    /// <summary>
    /// Show a free prize dialog that gives the user coins. We default to the standard General Message window, adding any additional
    /// content as setup in the FreePrizeManager configuration.
    /// </summary>
    public DialogInstance ShowFreePrizeDialog(Action <DialogInstance> doneCallback = null)
    {
        // only allow the free prize dialog to be shown once.
        if (IsShowingFreePrizeDialog) return null;

        IsShowingFreePrizeDialog = true;
        _prizeIsProcessed = false;
        dialogInstance = DialogManager.Instance.Create(ContentPrefab, null, null, null,
            runtimeAnimatorController: ContentAnimatorController);

        Sprite sprite = null;
        string text = BonusText();

        if (prizeIsCoins && this.PrizeItems > 0)
        {
            sprite = Resources.Load<Sprite>("Images/coins");
        }

        string DateLastFreePrizeTakeString = PreferencesFactory.GetString(Constants.KeyDateLastFreePrizeTake);
        DateTime DateLastFreePrizeTake = UnbiasedTime.Instance.Now();

        if (!DateLastFreePrizeTakeString.Equals(""))
        {
            DateLastFreePrizeTake = DateTime.Parse(DateLastFreePrizeTakeString);
        }

        int DaysInRow = PreferencesFactory.GetInt(Constants.KeyFreePrizeTakeDaysInRow, 1);

        if (DateLastFreePrizeTake.AddDays(1).Date == UnbiasedTime.Instance.Now().Date)
        {
            DaysInRow += 1;
            PreferencesFactory.SetInt(Constants.KeyFreePrizeTakeDaysInRow, DaysInRow);
        }
        else
        { // reset
            DaysInRow = 1;
            PreferencesFactory.SetInt(Constants.KeyFreePrizeTakeDaysInRow, DaysInRow);
        }

        GameObject Days = GameObjectHelper.GetChildNamedGameObject(dialogInstance.gameObject, "Days", true);

        for (int i = 0; i < Constants.DailyBonusItems.Length; i++)
        {
            int prizeValue = Constants.DailyBonusItems[i];

            string dayText = string.Format("Day{0}", (i + 1));
            GameObject day = GameObjectHelper.GetChildNamedGameObject(Days, dayText, true);

            if (!day)
            {
                continue;
            }

            Text t = GameObjectHelper.GetChildNamedGameObject(day, "Text", true).GetComponent<Text>();
            t.text = prizeValue.ToString();

            if (DaysInRow - 1 > i)
            {
                GameObject image = GameObjectHelper.GetChildNamedGameObject(day, "Image", true);
                GameObjectHelper.SafeSetActive(image, true);
            }

            //			if ( DaysInRow-1 == i) {
            //				GameObject today = GameObjectHelper.GetChildNamedGameObject(day, "Today", true);
            //				GameObjectHelper.SafeSetActive (today, true);
            //
            //				GameObject dayNumber = GameObjectHelper.GetChildNamedGameObject(day, "DayNumber", true);
            //				GameObjectHelper.SafeSetActive (dayNumber, false);
            //			}

            if (DaysInRow == (i + 1))
            { // add daily bonus
                this.PrizeItems += prizeValue;

                GameObject claimButton = GameObjectHelper.GetChildNamedGameObject(dialogInstance.Content, "ClaimButton", true);

                Text claimText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(claimButton, "Text", true);

                if (prizeIsPoints)
                {
                    claimText.text = LocaliseText.Format("FreePrize.ClaimPoints", this.PrizeItems);
                }

                if ( prizeIsCoins ) 
                {
                    claimText.text = LocaliseText.Format("FreePrize.ClaimButton", this.PrizeItems);
                }
            }
        }

        dialogInstance.Show(title: LocaliseText.Get("FreePrize.Title"), text: text, text2Key: "FreePrize.Text2",
        doneCallback:(DialogInstance _dialogInstance) => {
            if (doneCallback != null)
            {
                doneCallback(_dialogInstance);
            }

            ShowFreePrizeDone(_dialogInstance);       
        },
            dialogButtons:
            ContentShowsButtons
            ? DialogInstance.DialogButtonsType.Custom
            : DialogInstance.DialogButtonsType.Ok);

        GameObject ImageCoins = GameObjectHelper.GetChildNamedGameObject(dialogInstance.gameObject, "ph_Image", true);
        ImageCoins.SetActive(false);

        if (this.PrizeItems > 0 && prizeIsCoins && sprite != null)
        {
            ImageCoins.SetActive(true);
            ImageCoins.GetComponent<Image>().sprite = sprite;
        }

        StartNewCountdown();

        if (!Debug.isDebugBuild)
        {
            Fabric.Answers.Answers.LogContentView("FreePrize", "Dialog");
        }
#if !UNITY_EDITOR
        AdColonyManager.Instance.RequestAd(Constants.AdColonyDoubleDailyBonus);
        LoadAdmobRewarderVideo();
#endif

        return dialogInstance;
	}

	public void ClaimButton() {
		ProcessDailyBonus ();
	}

    public void CloseButton() {
        if (!_prizeIsProcessed)
        {
            ProcessDailyBonus(doublePrize: false);
        }

        dialogInstance.Done();
    }

    void Update()
    {
        if (IsShowingFreePrizeDialog && Input.GetKeyDown(KeyCode.Escape))
        {
            ClaimButton();
        }
    }

	private void ProcessDailyBonus(bool doublePrize = true) {
        _prizeIsProcessed = true;
		int DaysInRow = PreferencesFactory.GetInt(Constants.KeyFreePrizeTakeDaysInRow, 0);
		int extraCoins = 0;

		if ( DaysInRow == 7 ) { // reset
            extraCoins = Constants.DialyBonusCoins; // bonus coins at last day

			PreferencesFactory.SetInt (Constants.KeyFreePrizeTakeDaysInRow, 1);
		}

		PreferencesFactory.SetString (Constants.KeyDateLastFreePrizeTake, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));

		if (PrizeDialogClosedAudioClip != null) {
			GameManager.Instance.PlayEffect (PrizeDialogClosedAudioClip);
		}

		if ( (prizeIsCoins && this.PrizeItems > 0) || extraCoins > 0) {
			// add extra coins to reward coins only of prize is coins
			if ( prizeIsCoins ) {
				extraCoins += this.PrizeItems;
			}

			GameObject animatedCoins = GameObject.Find ("AddCoinsAnimated");
			GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
			AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

			addCoins.AnimateCoinsAdding (extraCoins);
		}

		if ( prizeIsPoints ) {
			GameObject animatedPoints = GameObject.Find("AddPointsAnimated");

			GameObject _clone = Instantiate(animatedPoints, animatedPoints.transform.parent);
			AddPointsAnimated _add = _clone.GetComponent<AddPointsAnimated>();

			_add.AnimateAdding(this.PrizeItems);

            GameSparksManager.Instance.SendPoints(this.PrizeItems, "FreePrize");
		}

        SetCurrentPrizeAmount();

        // Local notifications
#if !UNITY_EDITOR
		CancelLocalNotifications ();
		RegisterLocalNotifications ();
#endif

        if (doublePrize)
        {
            ShowDoubleScreen();
        }
	}

	void ShowDoubleScreen() {
		GameObject part1 = GameObjectHelper.GetChildNamedGameObject (dialogInstance.Content, "Part1", true);
		GameObjectHelper.SafeSetActive (part1, false);

		GameObject part2 = GameObjectHelper.GetChildNamedGameObject (dialogInstance.Content, "Part2", true);
		GameObjectHelper.SafeSetActive (part2, true);

		Text rewardsCoinsText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text> (part2, "RewardsCoins", true);

        if (prizeIsPoints)
        {
            rewardsCoinsText.text = LocaliseText.Format("FreePrize.NumberPoints", this.PrizeItems);
        }

        if (prizeIsCoins)
		{
			rewardsCoinsText.text = LocaliseText.Format("FreePrize.NumberCoins", this.PrizeItems);
		}
	}

	/// <summary>
	///  Callback when the free prize dialog is closed. You may override this in your own subclasses, but be sure to call this base class instance.
	/// </summary>
	/// <param name="dialogInstance"></param>
	public virtual void ShowFreePrizeDone(DialogInstance dialogInstance)
	{
		IsShowingFreePrizeDialog = false;
	}

    bool UserNotificationsChangedHandler(GameFramework.Messaging.BaseMessage message) {

        UserNotificationsChangedMessage msg = message as UserNotificationsChangedMessage;

        if ( msg.Enabled ) {
            CancelLocalNotifications();
            RegisterLocalNotifications();
        } else {
            CancelLocalNotifications();
        }

        return true;
    }

	void CancelLocalNotifications() {
		for (int i = 0; i < _notificationDays.Length; i++) {
			int day = _notificationDays [i];

			LocalNotifications.CancelNotification (Constants.NotificationPrizeAvailableStart + day + 1);
		}
	}

	void RegisterLocalNotifications() {
		// NextFreePrizeAvailable is 00:00 every day
		// between 13:00 & 16:00
		int min = 13 * 60 * 60; // h * min * sec
		int max = 16 * 60 * 60;

		DateTime fireDate;
		string text;

		// register local notification for everyday for first 10 days
		for (int i = 0; i < _notificationDays.Length; i++) {
			int day = _notificationDays [i];

			text = RandomNotificationText ();
			fireDate = NextFreePrizeAvailable.AddDays (day).AddSeconds (UnityEngine.Random.Range (min, max));

			LocalNotifications.RegisterNotification (Constants.NotificationPrizeAvailableStart + day + 1, fireDate, text);
		}
	}

	string RandomNotificationText() {
		string[] texts;

#if UNITY_ANDROID
		if (iSDK.Utils.AndroidSDKVersion () < 23) {
			texts = new string[2] {
				LocaliseText.Get ("FreePrize.PushNotificationGuessWhatNoEmoji"),
				LocaliseText.Get ("FreePrize.PushNotificationDailyBonusNoEmoji")
			};
		} else {
			texts = new string[2] {
				LocaliseText.Get ("FreePrize.PushNotificationGuessWhat"),
				LocaliseText.Get ("FreePrize.PushNotificationDailyBonus")
			};
		}
#else
		texts = new string[2] {
			LocaliseText.Get ("FreePrize.PushNotificationGuessWhat"),
			LocaliseText.Get ("FreePrize.PushNotificationDailyBonus")
		};
#endif

		int index = UnityEngine.Random.Range (0, texts.Length);

		return texts[index];
	}

	[Serializable]
	public class MinMax
	{
		public int Min;
		public int Max;
		public float Difference { get { return Max - Min; }}
	}
}
