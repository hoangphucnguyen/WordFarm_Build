using UnityEngine;
using GameFramework.GameStructure;
using GameFramework.Preferences;
using GameFramework.GameStructure.Levels.ObjectModel;
using System;
using System.Globalization;
using GoogleMobileAds.Api;
using GameFramework.GameStructure.Players.ObjectModel;
using GameFramework.GameStructure.GameItems.ObjectModel;
using GameFramework.GameStructure.Worlds.ObjectModel;
using GameFramework.GameStructure.Characters.ObjectModel;
using UnityEngine.Advertisements;
using GameFramework.Localisation;
using GameFramework.Localisation.Messages;
using GameFramework.Messaging;
using GameFramework.Debugging;
using GameFramework.GameStructure.Players.Messages;
using PrefsEditor;

public class CustomGameManager : GameManager
{

    public AudioClip defaultAudio;
    public int StartupLevels = 0;

    public PackGameItemManager Packs { get; set; }
    public int NumberOfAutoCreatedPacks = 10;

    public RankGameItemManager Ranks { get; set; }
    public int NumberOfAutoCreatedRanks = 1;

    static int[] _notificationDays = new int[4] { 2, 5, 8, 30 };
    private float _currentBackgroundSoundVolume = 1.0f;

    bool _paused = false;
    public bool IsPaused { get { return _paused; } }

    protected override void GameSetup()
    {
        // secure preferences
        PreferencesFactory.UseSecurePrefs = SecurePreferences;
        if (SecurePreferences)
        {
            if (string.IsNullOrEmpty(PreferencesPassPhrase))
                Debug.LogWarning("You have not set a custom pass phrase in GameManager | Player Preferences. Please correct for improved security.");
            else
                PreferencesFactory.PassPhrase = PreferencesPassPhrase;
            PreferencesFactory.AutoConvertUnsecurePrefs = AutoConvertUnsecurePrefs;
        }

        StartupLevels = NumberOfAutoCreatedLevels;

        int NumberOfAdditionalCreatedLevels = PreferencesFactory.GetInt(Constants.KeyNumberOfAdditionalCreatedLevels);

        NumberOfAutoCreatedLevels += NumberOfAdditionalCreatedLevels;

        string FirstAppStartDate = PreferencesFactory.GetString(Constants.KeyFirstAppStartDate, null);

        if (FirstAppStartDate == null)
        {
            PreferencesFactory.SetString(Constants.KeyFirstAppStartDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));
        }

        PreferencesFactory.SetString(Constants.KeyLastAppStartDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));

        int TimesAppStarted = PreferencesFactory.GetInt(Constants.KeyTimesAppStarted, 0);
        PreferencesFactory.SetInt(Constants.KeyTimesAppStarted, TimesAppStarted + 1);

        base.GameSetup();

        if (PlayerPrefs.GetInt("ManualChangedLanguage", 0) == 0)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            string savedLanguage = PreferencesFactory.GetString("Language", useSecurePrefs: false);
            string systemLanguage = DeviceLanguage();

            // user does not changed his language manual
            // and system language is different from previous auto-detected
            if (systemLanguage != savedLanguage)
            {
                LanguageController.ChangeLanguage(systemLanguage);
            }
#endif

#if UNITY_IOS
            IOSNativeUtility.OnLocaleLoaded += GetLocale;
            IOSNativeUtility.Instance.GetLocale();
#endif
        }

        Packs = new PackGameItemManager();

        if (LevelSetupMode == GameItemSetupMode.FromResources)
            Packs.Load(1, NumberOfAutoCreatedPacks);

        Ranks = new RankGameItemManager();

        if (LevelSetupMode == GameItemSetupMode.FromResources)
            Ranks.Load(1, NumberOfAutoCreatedRanks);

        Rank rank = Ranks.GetItem(1);

        if (!rank.IsUnlocked)
        {
            rank.IsUnlocked = true; // first rank is unlocked by default
            rank.UpdatePlayerPrefs();
        }

        Pack pack = Packs.GetItem(1);

        if (!pack.IsUnlocked)
        {
            pack.IsUnlocked = true; // first pack is unlocked by default
            pack.UpdatePlayerPrefs();
        }

        Level level = Levels.GetItem(1);

        if (!level.IsUnlocked)
        {
            level.IsUnlocked = true; // first level is unlocked by default
            level.UpdatePlayerPrefs();
        }

        GameManager.SafeAddListener<UserNotificationsChangedMessage>(UserNotificationsChangedHandler);
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);

        // bug fix in gameframework when user loose all his lives and start game again and Lives back to full
        if (FirstAppStartDate != null)
        { // but only after first start ever
            Player.Lives = Player.GetSettingInt("Lives", 0);
        }

        if (FirstAppStartDate == null)
        { // first start
            PreferencesFactory.SetInt(Constants.KeyNotificationsAllowed, 1);
        }

        //

        Advertisement.Initialize(Constants.UnityAdsGameId);

        if (BackGroundAudioVolume > Constants.DefaultAudioVolume)
        {
            BackGroundAudioVolume = Constants.DefaultAudioVolume;
        }

        if (EffectAudioVolume > Constants.DefaultAudioVolume)
        {
            EffectAudioVolume = Constants.DefaultAudioVolume;
        }

        _currentBackgroundSoundVolume = BackGroundAudioVolume;

        if (!SoundEnabled())
        {
            BackGroundAudioSource.Stop();
        }

#if UNITY_IPHONE
		SA.IOSNative.Core.AppController.Subscribe();

		SA.IOSNative.Core.AppController.OnApplicationDidReceiveMemoryWarning += OnApplicationDidReceiveMemoryWarning;
#endif

#if !UNITY_EDITOR
        CancelLocalNotifications ();
        RegisterLocalNotifications ();
#endif
    }

    protected override void OnApplicationPause(bool pauseStatus)
    {
        base.OnApplicationPause(pauseStatus);

        _paused = pauseStatus;
    }

    string DeviceLanguage()
    {
#if UNITY_EDITOR
        return Application.systemLanguage.ToString();
#endif

#if UNITY_ANDROID
        // Locale class
        var locale = new AndroidJavaClass("java.util.Locale");
        // Get instance of Locale class
        var localeObject = locale.CallStatic<AndroidJavaObject>("getDefault");

        // Returns the country/region code for this locale, which should either be the empty string, an uppercase ISO 3166 2-letter code, or a UN M.49 3-digit code.
        var country = localeObject.Call<string>("getCountry").ToLower();

        MyDebug.Log("country:" + country);

        // Returns a name for the locale's country that is appropriate for display to the user.
        var displayCountry = localeObject.Call<string>("getDisplayCountry");

        MyDebug.Log("displayCountry:" + displayCountry);

        // Returns a name for the locale that is appropriate for display to the user.
        var displayName = localeObject.Call<string>("getDisplayName");

        MyDebug.Log("displayName:" + displayName);

        // Returns the language code of this Locale.
        var language = localeObject.Call<string>("getLanguage");

        MyDebug.Log("language:" + language);

        return LanguageUtils.CountryCodeToLanguage(country);
#endif

        return Application.systemLanguage.ToString();
    }

    private void GetLocale(ISN_Locale locale)
    {
        IOSNativeUtility.OnLocaleLoaded -= GetLocale;

        string savedLanguage = PreferencesFactory.GetString("Language", useSecurePrefs: false);
        string systemLanguage = LanguageUtils.CountryCodeToLanguage(locale.CountryCode.ToLower());

        // user does not changed his language manual
        // and system language is different from previous auto-detected
        if (systemLanguage != savedLanguage)
        {
            LanguageController.ChangeLanguage(systemLanguage);
        }
    }

	protected override void GameDestroy()
	{
		base.GameDestroy();

		GameManager.SafeRemoveListener<UserNotificationsChangedMessage>(UserNotificationsChangedHandler);
        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
	}

	public void ResetGame() {
        int oldCoins = Player.Coins;
        int oldPoints = Player.Score;

		GameSetup ();

        Player.SendCoinsChangedMessage(Player.Coins, oldCoins);
        Player.SendScoreChangedMessage(Player.Score, oldPoints);

        string newLanguage = PreferencesFactory.GetString("Language", useSecurePrefs: false);

        if ( newLanguage != null ) {
            LocaliseText.Language = newLanguage;
        }

		if (SoundEnabled())
		{
			BackGroundAudioSource.Play();
		}

        GameManager.SafeQueueMessage(new GameResetedMessage());
	}

    public void MuteSound() {
        _currentBackgroundSoundVolume = BackGroundAudioVolume;

        BackGroundAudioVolume = 0.0f;
    }

    public bool SoundEnabled() {
        return BackGroundAudioVolume > 0.1f;
    }

    public void UnmuteSound() {
        BackGroundAudioVolume = _currentBackgroundSoundVolume;
    }

	private void OnApplicationDidReceiveMemoryWarning() {
		GameSparksManager.Instance.ClearMemory ();
	}

	public void ReloadPlayer() {
		BackGroundAudioVolume = PreferencesFactory.GetFloat("BackGroundAudioVolume", BackGroundAudioVolume, false);
		EffectAudioVolume = PreferencesFactory.GetFloat("EffectAudioVolume", EffectAudioVolume, false);

		Players = new PlayerGameItemManager();
		Players.Load(0, PlayerCount-1);

		// handle auto setup of worlds and levels
		if (AutoCreateWorlds)
		{
			var coinsToUnlockWorlds = WorldUnlockMode == GameItem.UnlockModeType.Coins ? CoinsToUnlockWorlds : -1;
			Worlds = new WorldGameItemManager();
			Worlds.Load(1, NumberOfAutoCreatedWorlds, coinsToUnlockWorlds, LoadWorldDatafromResources);

			// if we have worlds then autocreate levels for each world.
			if (AutoCreateLevels)
			{
				for (var i = 0; i < NumberOfAutoCreatedWorlds; i++)
				{
					var coinsToUnlock = LevelUnlockMode == GameItem.UnlockModeType.Coins ? CoinsToUnlockLevels : -1;
					Worlds.Items[i].Levels = new LevelGameItemManager();
					Worlds.Items[i].Levels.Load(WorldLevelNumbers[i].Min, WorldLevelNumbers[i].Max, coinsToUnlock, LoadLevelDatafromResources);
				}

				// and assign the selected set of levels
				Levels = Worlds.Selected.Levels;
			}
		}
		else
		{
			// otherwise not automatically setting up worlds so if auto setup of levels then create at root level.
			if (AutoCreateLevels)
			{
				var coinsToUnlock = LevelUnlockMode == GameItem.UnlockModeType.Coins ? CoinsToUnlockLevels : -1;
				Levels = new LevelGameItemManager();
				Levels.Load(1, NumberOfAutoCreatedLevels, coinsToUnlock, LoadLevelDatafromResources);
			}
		}

		// handle auto setup of characters
		if (AutoCreateCharacters)
		{
			Characters = new CharacterGameItemManager();
			if (CharacterUnlockMode == GameItem.UnlockModeType.Coins)
				Characters.Load(1, NumberOfAutoCreatedCharacters, CoinsToUnlockCharacters, LoadCharacterDatafromResources);
			else
				Characters.Load(1, NumberOfAutoCreatedCharacters, loadFromResources: LoadCharacterDatafromResources);
		}
	}

	public void ResetDefaultSound() {
        if (BackGroundAudioSource != null && defaultAudio != null && BackGroundAudioSource.clip != defaultAudio) {
			BackGroundAudioSource.clip = defaultAudio;

            if (SoundEnabled())
            {
                BackGroundAudioSource.Play();
            }
        }
    }

    void ChangeLevelsByLanguage() {
        foreach ( Level level in Levels ) {
            level.JsonData = null;
        }
    }

    bool LocalisationHandler(BaseMessage message)
    {
        LocalisationChangedMessage msg = (LocalisationChangedMessage)message;

        ChangeLevelsByLanguage();

#if !UNITY_EDITOR
        CancelLocalNotifications();
        RegisterLocalNotifications();
#endif
        return true;
    }

	bool UserNotificationsChangedHandler(GameFramework.Messaging.BaseMessage message)
	{
		UserNotificationsChangedMessage msg = message as UserNotificationsChangedMessage;

		if (msg.Enabled)
		{
			CancelLocalNotifications();
			RegisterLocalNotifications();
		}
		else
		{
			CancelLocalNotifications();
		}

		return true;
	}

	void CancelLocalNotifications() {
		for (int i = 0; i < _notificationDays.Length; i++) {
			int day = _notificationDays [i];

			LocalNotifications.CancelNotification (Constants.NotificationDailyStart + day + 1);
		}
	}

	void RegisterLocalNotifications() {
		float seconds = DateTimeUtils.SecondsTillMidnight ();

		// between 19:00 & 20:00
		int min = 19 * 60 * 60; // h * min * sec
		int max = 20 * 60 * 60;

		DateTime midnight = UnbiasedTime.Instance.Now().AddSeconds (seconds);
		DateTime fireDate;
		string text;

		// register local notification
		for (int i = 0; i < _notificationDays.Length; i++) {
			int day = _notificationDays [i];

			text = RandomNotificationText ();
			fireDate = midnight.AddDays(day).AddSeconds(UnityEngine.Random.Range (min, max));

			LocalNotifications.RegisterNotification (Constants.NotificationDailyStart + day + 1, fireDate, text);
		}
	}

	string RandomNotificationText() {
		string[] texts;

#if UNITY_ANDROID
		if (iSDK.Utils.AndroidSDKVersion () < 23) {
			texts = new string[3] {
				LocaliseText.Get ("Notifications.NotPlayingRecentlyNoEmoji"),
				LocaliseText.Get ("Notifications.HighIQNoEmoji"),
				LocaliseText.Get ("Notifications.MultiplayerNoEmoji")
			};
		} else {
			texts = new string[3] {
				LocaliseText.Get ("Notifications.NotPlayingRecently"),
				LocaliseText.Get ("Notifications.HighIQ"),
				LocaliseText.Get ("Notifications.Multiplayer")
			};
		}
#else
		texts = new string[3] {
			LocaliseText.Get ("Notifications.NotPlayingRecently"),
			LocaliseText.Get ("Notifications.HighIQ"),
			LocaliseText.Get ("Notifications.Multiplayer")
		};
#endif

		int index = UnityEngine.Random.Range (0, texts.Length);

		return texts[index];
	}

	public BannerView AddBanner(AdSize adSize) {
		if ( PreferencesFactory.GetInt (Constants.KeyNoAds, 0) == 1 ) {
			return null;
		}

		BannerView bannerView = new BannerView(Constants.AdMobUnitIdBanner, adSize, AdPosition.Bottom);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);

		return bannerView;
	}

	public InterstitialAd AddInterstitialAd() {
		if ( PreferencesFactory.GetInt (Constants.KeyNoAds, 0) == 1 ) {
			return null;
		}

		// Initialize an InterstitialAd.
		InterstitialAd interstitial = new InterstitialAd (Constants.AdMobUnitIdInterstitial);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder ().Build ();
		// Load the interstitial with the request.
		interstitial.LoadAd (request);

		return interstitial;
	}

	public int CalculateBannerHeight() {
		if (Screen.height <= 400*Mathf.RoundToInt(Screen.dpi/160)) {
			return 32*Mathf.RoundToInt(Screen.dpi/160);
		} else if (Screen.height <= 720*Mathf.RoundToInt(Screen.dpi/160)) {
			return 50*Mathf.RoundToInt(Screen.dpi/160);
		} else {
			return 90*Mathf.RoundToInt(Screen.dpi/160);
		}
	}

	public override void SaveState()
	{
		base.SaveState ();

		Player.UpdatePlayerPrefs ();

		GameSparksManager.Instance.SyncProgress ();
		GameSparksManager.Instance.SyncLevels ();
	}
}
