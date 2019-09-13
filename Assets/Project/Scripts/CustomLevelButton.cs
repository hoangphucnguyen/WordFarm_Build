using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.Levels.Components;
using GameFramework.UI.Other;
using GameFramework.GameObjects;
using GameFramework.GameStructure;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Preferences;
using GoogleMobileAds.Api;
using System;
using UnityEngine.Advertisements;
using GameFramework.GameStructure.Levels.ObjectModel;

public class CustomLevelButton : LevelButton
{
    public override void SetupDisplay()
    {
        base.SetupDisplay();

        GameObjectHelper.SafeSetActive(StarsWonGameObject, true);
    }

    public override void ClickLocked()
    {
        if (PreferencesFactory.GetInt(Constants.KeyNoAds, 0) == 1)
        { // no ads
            ClickLockedProcess();
            return;
        }

        int CountSelectLevelClicks = PreferencesFactory.GetInt(Constants.KeyCountSelectLevelClicks);
        PreferencesFactory.SetInt(Constants.KeyCountSelectLevelClicks, CountSelectLevelClicks + 1);

        //if (CountSelectLevelClicks > 0 && CountSelectLevelClicks % Constants.ShowAdsPerTime == 0 && Reachability.Instance.IsReachable())
        //{
        //    Loading.Instance.Show();

        //    AdColonyManager.Instance.SetCallback(BannerLockedClosed);
        //    AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());
        //}
        //else
        //{
            ClickLockedProcess();
        //}
    }

    void ClickLockedProcess()
    {
        DialogManager.Instance.ShowInfo(textKey: GameItem.IdentifierBase + ".Buy.NotEnabled");
    }

    void ClickUnlockedProcess()
    {
        base.ClickUnlocked();
    }

    public override void ClickUnlocked()
    {
        if (PreferencesFactory.GetInt(Constants.KeyNoAds, 0) == 1)
        { // no ads
            ClickUnlockedProcess();
            return;
        }

        int CountSelectLevelClicks = PreferencesFactory.GetInt(Constants.KeyCountSelectLevelClicks);
        PreferencesFactory.SetInt(Constants.KeyCountSelectLevelClicks, CountSelectLevelClicks + 1);

        //if (CountSelectLevelClicks > 0 && CountSelectLevelClicks % Constants.ShowAdsPerTime == 0 && Reachability.Instance.IsReachable())
        //{
        //    Loading.Instance.Show();

        //    AdColonyManager.Instance.SetCallback(BannerUnlockedClosed);
        //    AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());
        //}
        //else
        //{
            ClickUnlockedProcess();
        //}
    }

    void BannerUnlockedClosed(string zoneId, int amount, bool success)
    {
        if (!success)
        {
            ShowVideoAd(BannerClosed);
            return;
        }

        Loading.Instance.Hide();

        ClickUnlockedProcess();
    }

    void BannerLockedClosed(string zoneId, int amount, bool success)
    {
        if (!success)
        {
            ShowVideoAd(BannerLockedClosed);
            return;
        }

        Loading.Instance.Hide();

        ClickLockedProcess();
    }

    public void ShowVideoAd(Action<int, bool> action)
    {
        if (Advertisement.IsReady("video"))
        {
            var options = new ShowOptions
            {
                resultCallback = (ShowResult result) =>
                {
                    Loading.Instance.Hide();
                    action(0, result == ShowResult.Finished);
                }
            };
            Advertisement.Show("video", options);
        }
        else
        {
            Loading.Instance.Hide();
            action(0, false);
        }
    }

    void BannerClosed(int amount, bool success)
    {
        Loading.Instance.Hide();
        ClickUnlockedProcess();
    }

    void BannerLockedClosed(int amount, bool success)
    {
        Loading.Instance.Hide();
        ClickLockedProcess();
    }
}
