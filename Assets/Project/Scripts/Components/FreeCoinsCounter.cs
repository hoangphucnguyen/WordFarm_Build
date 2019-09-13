using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameFramework.GameObjects;
using GameFramework.Localisation;
using GameFramework.Preferences;
using GameFramework.UI.Dialogs.Components;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class FreeCoinsCounter : MonoBehaviour {

    private float timeSinceLastCalled;
    private Text freeCoinsCounterText;
    GameObject FreeCoinsObject;
    GameObject FreeCoinsCounterObject;
    bool showingAd = false;

    // Use this for initialization
    void Start()
    {
        FreeCoinsObject = GameObjectHelper.GetChildNamedGameObject(gameObject, "FreeCoins", true);
        FreeCoinsCounterObject = GameObjectHelper.GetChildNamedGameObject(gameObject, "FreeCoinsCounter", true);

        freeCoinsCounterText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(FreeCoinsCounterObject, "Text", true);

        DateTime nowDate = UnbiasedTime.Instance.Now();
        DateTime freeCoinsDate = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyFreeCoinsAvailable, nowDate.ToString(CultureInfo.InvariantCulture)));

        int TimesFreeCoins = PreferencesFactory.GetInt(Constants.KeyFreeCoinsCounter, 0);

        if (TimesFreeCoins == 0 || (TimesFreeCoins == Constants.TimesFreeCoins && freeCoinsDate <= nowDate))
        {
            ResetCounter();
        }
        else
        {
            if (TimesFreeCoins == Constants.TimesFreeCoins)
            {
                TimeSpan timeSpan = TimeForNextCoins();
                freeCoinsCounterText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                GameObjectHelper.SafeSetActive(FreeCoinsObject, false);
                GameObjectHelper.SafeSetActive(FreeCoinsCounterObject, true);
            }
        }

        if (!Reachability.Instance.IsReachable())
        { // hide if no internet
            GameObjectHelper.SafeSetActive(FreeCoinsObject, false);
        }

#if !UNITY_EDITOR
        AdColonyManager.Instance.RequestAd(Constants.AdColonyCoinsRewardZone);
        LoadAdmobRewarderVideo();
#endif
    }

    private void OnDestroy()
    {
        if (rewardBasedVideo != null)
        {
            rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
            rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
        }
    }

    void ResetCounter() {
        PreferencesFactory.SetInt(Constants.KeyFreeCoinsCounter, 0);

        GameObjectHelper.SafeSetActive(FreeCoinsObject, true);
        GameObjectHelper.SafeSetActive(FreeCoinsCounterObject, false);
    }
	
	// Update is called once per frame
	void Update () {
        int TimesFreeCoins = PreferencesFactory.GetInt(Constants.KeyFreeCoinsCounter, 0);

        if ( TimesFreeCoins >= Constants.TimesFreeCoins )
        {
            timeSinceLastCalled += Time.deltaTime;

            if (timeSinceLastCalled > 1f)
            {
                TimeSpan timeSpan = TimeForNextCoins();

                if (timeSpan.TotalSeconds > 0)
                {
                    freeCoinsCounterText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                }
                else
                {
                    ResetCounter();
                }

                timeSinceLastCalled = 0f;
            }
        }
	}

    TimeSpan TimeForNextCoins()
    {
        DateTime nowDate = UnbiasedTime.Instance.Now();
        DateTime freeCoinsDate = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyFreeCoinsAvailable, nowDate.ToString(CultureInfo.InvariantCulture)));

        TimeSpan timeSpan = freeCoinsDate.Subtract(nowDate);

        return timeSpan;
    }

    public void FreeCoins()
    {
        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("ExtraCoins_Shop");
            Fabric.Answers.Answers.LogCustom("ExtraCoins_Shop");
        }

        DialogManager.Instance.Show("FreeCoinsDialog", doneCallback: FreeCoinsDoneCallback);
    }

    public virtual void FreeCoinsDoneCallback(DialogInstance dialogInstance)
    {
        if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok)
        {
            if (!Reachability.Instance.IsReachable())
            {
                DialogManager.Instance.Show(titleKey: "GeneralMessage.Info.Title",
                    textKey: "GeneralMessage.NoInternet");
                return;
            }

            Loading.Instance.Show();

            if (rewardBasedVideo != null && rewardBasedVideo.IsLoaded())
            {
                showingAd = true;
                rewardBasedVideo.Show();
            }
            else
            {
                AdColonyManager.Instance.SetCallback(CloseFreeCoinsVideo);
                AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId(Constants.AdColonyCoinsRewardZone));
            }
        }
    }

    void CloseFreeCoinsVideo(string zoneId, int amount, bool success)
    {
        if (!success)
        {
            ShowRewardedAd();
            return;
        }

        Loading.Instance.Hide();

        if (zoneId == null || !zoneId.Equals(Constants.AdColonyCoinsRewardZone))
        {
            return;
        }

        CompleteReward();
    }


    public void ShowRewardedAd()
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions
            {
                resultCallback = (ShowResult result) => {
                    Loading.Instance.Hide();

                    if (result == ShowResult.Finished)
                    {
                        CompleteReward();
                    }
                }
            };
            Advertisement.Show("rewardedVideo", options);
        }
        else
        {
            Loading.Instance.Hide();
            DialogManager.Instance.ShowError(LocaliseText.Get("Advertising.UnityAds.UnableToShow"));
        }
    }

    void CompleteReward()
    {
        GameObject animatedCoins = GameObject.Find("AddCoinsAnimated");
        GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
        AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

        addCoins.AnimateCoinsAdding(Constants.FreeCoinsDaily);

        //

        PreferencesFactory.SetString(Constants.KeyFreeCoinsAvailable, UnbiasedTime.Instance.Now().AddMinutes(Constants.MinutesBetweenFreeCoins).ToString(CultureInfo.InvariantCulture));

        int TimesFreeCoins = PreferencesFactory.GetInt(Constants.KeyFreeCoinsCounter, 0);
        TimesFreeCoins += 1;

        PreferencesFactory.SetInt(Constants.KeyFreeCoinsCounter, TimesFreeCoins);

        if (TimesFreeCoins >= Constants.TimesFreeCoins)
        {
            GameObjectHelper.SafeSetActive(FreeCoinsObject, false);
            GameObjectHelper.SafeSetActive(FreeCoinsCounterObject, true);
        }
    }

    private RewardBasedVideoAd rewardBasedVideo;
    void LoadAdmobRewarderVideo()
    {
        if (rewardBasedVideo == null)
        {
            rewardBasedVideo = RewardBasedVideoAd.Instance;
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        }

        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(request, Constants.AdMobUnitIdRewardedVideo);
    }

    void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        Debug.Log("FreeCoins: Close admob: " + showingAd);
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
                Debug.Log("FreeCoins: args.Type: " + args.Type + "; args.Amount: " + args.Amount);

                CompleteReward();
                LoadAdmobRewarderVideo();
            }
        });
    }
}
