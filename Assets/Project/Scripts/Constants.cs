using System.Collections;
using System.Collections.Generic;
using GameFramework.Localisation;
using UnityEngine;

public class Constants {
	// AdColony App ID and Zone IDs.
#if UNITY_IPHONE
	public static string AdColonyAppID = "appa3e5ace4f7714d88b5";
	public static string AdColonyInterstitialZoneID = "vz7f257e7f27c841689f";
    //public static string AdColonyInterstitialZoneID = "vz823e1cec0dec4e0ab6"; // !!!TEST ZONE!!!!

	public static string AdColonyCoinsRewardZone = "vz722096643c4a495985"; // 20 coins reward zone
	public static string AdColonyDoubleDailyBonus = "vz5321c51a44d14c1f91"; // double daily bonus
	public static string AdColonyFreeLife = "vz72cc650071c2411f84"; // free life
	public static string AdColonyDoubleCoins = "vz3cf3529613124e0b95"; // double coins end level

	public static string AdMobUnitIdBanner = "ca-app-pub-5651352110994359/8533584622";
    public static string AdMobUnitIdInterstitial = "ca-app-pub-3940256099942544/4411468910";
    public static string AdMobUnitIdRewardedVideo = "ca-app-pub-3940256099942544/1712485313";

	public static string UnityAdsGameId = "1419296";
#elif UNITY_ANDROID
	public static string AdColonyAppID = "appe1153781a8944ba398";
	public static string AdColonyInterstitialZoneID = "vz1af61b4fd74b4774ba";
	public static string AdColonyCoinsRewardZone = "vzb366f08877694fe895"; // 20 coins reward zone
	public static string AdColonyDoubleDailyBonus = "vz5a18c39cd69e4ac2a3"; // double daily bonus
	public static string AdColonyFreeLife = "vzdaee484453824edc98"; // free life
	public static string AdColonyDoubleCoins = "vz8aad9e3be74b472084"; // double coins end level

	public static string AdMobUnitIdBanner = "ca-app-pub-5651352110994359/5440517423";
    public static string AdMobUnitIdInterstitial = "ca-app-pub-3940256099942544/1033173712";
    public static string AdMobUnitIdRewardedVideo = "ca-app-pub-3940256099942544/5224354917";

	public static string UnityAdsGameId = "1419295";
#else
	public static string AdColonyAppID = "dummyAppID";
	public static string AdColonyInterstitialZoneID = "dummyCurrencyZoneID";
	public static string AdColonyCoinsRewardZone = "dummyCurrencyZoneID";
	public static string AdColonyDoubleDailyBonus = "dummyCurrencyZoneID";
	public static string AdColonyFreeLife = "dummyCurrencyZoneID";
	public static string AdColonyDoubleCoins = "dummyCurrencyZoneID";

	public static string AdMobUnitId = "unexpected_platform";
    public static string AdMobUnitIdRewardedVideo = "unexpected_platform";

	public static string UnityAdsGameId = "";
#endif

	// PreferencesFactory keys
	public static string KeyFirstAppStartDate = "FirstAppStartDate";
	public static string KeyLastAppStartDate = "LastAppStartDate";
	public static string KeyTimesAppStarted = "TimesAppStarted";
	public static string KeyNotificationPrizeAvailable = "NotificationPrizeAvailable";
	public static string KeyNotificationPrizeAvailable3Days = "NotificationPrizeAvailable3Days";
	public static string KeyNotificationAppStart = "NotificationAppStart";
	public static string KeyNotificationAppStart10Days = "NotificationAppStart10Days";
	public static string KeyNotificationAppStart30Days = "NotificationAppStart30Days";
	public static string KeyShowBannerOnMainMenuScreen = "ShowBannerOnMainMenuScreen";
	public static string KeyCountNextLevelClicks = "CountNextLevelClicks";
    public static string KeyCountCloseLevelClicks = "CountCloseLevelClicks";
	public static string KeyCountMainMenuClicks = "CountMainMenuClicks";
	public static string KeyCountSelectLevelClicks = "CountSelectLevelClicks";
	public static string KeyCountMainMenuColdStart = "CountMainMenuColdStart";
	public static string KeyCountLeaderboardClicks = "CountLeaderboardClicks";
	public static string KeyDateLastFreePrizeTake = "DateLastFreePrizeTake";
	public static string KeyFreePrizeTakeDaysInRow = "FreePrizeTakeDaysInRow";
	public static string KeyUnlockedPacks = "UnlockedPacks";
	public static string KeyFreeCoinsAvailable = "FreeCoinsAvailable";
    public static string KeyFreeCoinsCounter = "FreeCoinsCounter";
	public static string KeyDailyLoginBonusCoins = "DailyLoginBonusCoins";
	public static string KeyFreeLivesDateTime = "FreeLivesDateTime";
	public static string KeyFreeLivesTodayCount = "FreeLivesTodayCount";
	public static string KeyLifesDateOfLoose = "Lifes.DateOfLoose";
	public static string KeyTutorialMenuShow = "TutorialMenuShow";
	public static string KeyInviteLastDate = "InviteLastDate";
	public static string KeyInvitesTotal = "InvitesTotal";
	public static string KeyNumberOfAdditionalCreatedLevels = "NumberOfAdditionalCreatedLevels";
	public static string KeyNoAds = "NoAds";
	public static string KeyFacebookConnected = "FacebookConnected";
	public static string KeyInviteBalloonShowedDate = "InviteBalloonShowedDate";
	public static string KeyNoNotificationPermissionDate = "NoNotificationPermissionDate";
	public static string KeyNoNotificationPermissionDeniedDate = "NoNotificationPermissionDeniedDate";
	public static string KeyNotificationsAllowed = "NotificationsAllowed";
    public static string KeyShowSelectedPack = "ShowSelectedPack";
    public static string KeyFoundWords = "FoundWords";
    public static string KeyFoundWordsLevel = "FoundWordsLevel";
    public static string KeyOfflinePoints = "OfflinePoints";
    public static string KeyOfflinePointsData = "OfflinePointsData";
    public static string KeyShowTutorial = "ShowTutorial";
    public static string KeyAskFriendsLastDate = "AskFriendsLastDate";
    public static string KeyAskFriendsTotal = "AskFriendsTotal";
    public static string KeyAskFriendsHintHidden = "AskFriendsHintHidden";
    public static bool KeyEnableAdmobAds = false;
    public static string KeyRandomLevelWords = "RandomLevelWords";
    public static string KeyRewardShareLastDate = "RewardShareLastDate";
    public static string KeyRewardShareTotal = "RewardShareTotal";
    public static string KeyRateRewardLastDate = "RateRewardLastDate";
    public static string KeyRateRewardTotal = "RateRewardTotal";
    public static string KeyRateMaxRewardsTime = "RateMaxRewardsTime";

	public static int[] DailyBonusItems = new int[7]{10, 20, 30, 40, 50, 60, 70};
	public static int InviteAwardCoins = 50;
	public static int InviteMaxPerDay = 1;
	public static int FreeCoinsDaily = 50;
	public static int PointsPerLevel = 30;
    public static int HintPrice = 25;
    public static int DialyBonusCoins = 50;
    public static int MinutesBetweenFreeCoins = 60;
    public static int TimesFreeCoins = 4;
    public static int AskFriendsCoins = 25;
    public static int AskFriendsPerDay = 4;
    public static int ShowAdsPerTime = 1;
    public static string HashTagSocials = "WordFarmWithFriends";
    public static int RewardSharePerDay = 1;
    public static int RewardShareCoins = 200;
    public static int RateRewardCoins = 100;
    public static int RateMaxRewardsTime = 1;

	public static string KeyRedirectAfterSignIn = "RedirectAfterSignIn";
	public static string KeyRedirectAfterSignUp = "RedirectAfterSignUp";

	// Redirects
	public static int KeyRedirectLeaderboard = 1;

	// Notifications 
	public static int NotificationPrizeAvailable = 1;
	public static int NotificationPrizeAvailableAfter3Days = 2;
	public static int NotificationAppStart = 3;
	public static int NotificationAppStart10Days = 4;
	public static int NotificationAppStart30Days = 5;

	public static int NotificationDailyStart = 1000;
	public static int NotificationPrizeAvailableStart = 2000;

	// Profile
	public static string ProfileAvatar = "ProfileAvatar";
	public static string ProfileAvatarUploadId = "ProfileAvatarUploadId";
	public static string ProfileUsername = "ProfileUsername";
	public static string ProfileUserId = "ProfileUserId";
	public static string ProfileEmail = "ProfileEmail";
	public static string ProfileRating = "ProfileRating";
    public static string ProfileFBUserId = "ProfileFBUserId";
    public static string ProfileAnonymousUserId = "ProfileAnonymousUserId";

	public static float DelayButtonClickAction = 0.0f;

	//

	public static float DefaultAudioVolume = 0.25f;
	public static string ChallengeShortCode = "WFChallenge";
	public static float ChallengeTurnDuration = 30f;

    public static string ShareURL = "https://wordfarm.supersocial.games/tkTY/rEBD6iWuMH";
    public static string ShareImageURL = "https://supersocial.games/wordfarmstatic/images/icon.png";

    public enum ShareCodes { 
        Facebook,
        FacebookFeed,
        Native,
        CopyLink,
        AskFriends
    };

    public static string ShareURLLink(ShareCodes code) {
        switch (LocaliseText.Language) {
            case "Bulgarian":
                switch ( code ) {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/a2e05CUm1H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/MrRAFPvn1H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/hSyQOCzn1H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/elCtzkFo1H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/Qeuzx4kuMH";
                }

            case "Macedonian":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/Pohnbfin2H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/DQsCIxjn2H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/sEKGqEkn2H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/6V43mRln2H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/ypWUTSgn2H";
                }

            case "Serbian":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/L7MAQwFn1H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/uo0kesHn1H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/jN565lJn1H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/zzf6WcKo1H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/YY2Cp9fuMH";
                }

            case "Croatian":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/nzfjXttm2H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/C9TqbBum2H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/Xo5ESEwm2H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/aFlylCym2H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/YRGSf7pm2H";
                }

            case "Montenegro":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/JPt36uHm2H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/xMzEq6Im2H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/DzsLuSKm2H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/xu3TA9Lm2H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/9LRjd3Fm2H";
                }

            case "Slovak":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/8HZQpPRm2H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/KmeFfxTm2H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/aDpBdQUm2H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/vRSfErXm2H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/m4m1xNNm2H";
                }

            case "Bosnian":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/hYJhF20m2H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/cXyeOv2m2H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/hQ5q2i4m2H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/eVm7oR7m2H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/TVDMXhZm2H";
                }

            case "Brazil":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/bmYqjWbn2H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/V9N8WYcn2H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/uDo4f4dn2H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/kdL1pffn2H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/mM8BKjan2H";
                }

            case "Portuguese":
                switch (code)
                {
                    case ShareCodes.FacebookFeed:
                        return "https://wordfarm.supersocial.games/tkTY/L0fI5JTn1H";
                    case ShareCodes.Native:
                        return "https://wordfarm.supersocial.games/tkTY/XP0XmXWn1H";
                    case ShareCodes.CopyLink:
                        return "https://wordfarm.supersocial.games/tkTY/4Eiyk7Xn1H";
                    case ShareCodes.AskFriends:
                        return "https://wordfarm.supersocial.games/tkTY/5ZXF6CLo1H";
                    default:
                        return "https://wordfarm.supersocial.games/tkTY/YdndCypuMH";
                }
        }

        switch (code)
        {
            case ShareCodes.FacebookFeed:
                return "https://wordfarm.supersocial.games/tkTY/Leuil03n1H";
            case ShareCodes.Native:
                return "https://wordfarm.supersocial.games/tkTY/vvlWs94n1H";
            case ShareCodes.CopyLink:
                return "https://wordfarm.supersocial.games/tkTY/j3UWrC6n1H";
            case ShareCodes.AskFriends:
                return "https://wordfarm.supersocial.games/tkTY/bGa050Mo1H";
            default:
                return ShareURL;
        }
    }
}
