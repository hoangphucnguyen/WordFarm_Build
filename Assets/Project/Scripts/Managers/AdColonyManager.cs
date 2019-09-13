using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameFramework.GameObjects.Components;
using GameFramework.Debugging;

public class AdColonyManager : Singleton<AdColonyManager> {
	private Action<string, int, bool> callback = null; // Note - if callback(null, 0, ...) is passed, then ad was closed
    private Dictionary<string, AdColony.InterstitialAd> _cachedAds = new Dictionary<string, AdColony.InterstitialAd>();

	protected override void GameSetup() {
		#if !UNITY_EDITOR
		Init ();
		#endif
	}

	public void SetCallback (Action<string, int, bool> action) {
		this.callback = action;
	}

	public void Init() {
		AdColony.Ads.OnRequestInterstitial += (AdColony.InterstitialAd ad_) => {
			MyDebug.Log("AdColony.Ads.OnRequestInterstitial called");

            if (_cachedAds.ContainsKey(ad_.ZoneId))
            {
                _cachedAds[ad_.ZoneId] = ad_;
            }
            else
            {
                _cachedAds.Add(ad_.ZoneId, ad_);
            }
		};

		AdColony.Ads.OnClosed += (AdColony.InterstitialAd ad_) => {
			MyDebug.Log("AdColony.Ads.OnClosed called, expired: " + ad_.Expired);

			if (_cachedAds.ContainsKey(ad_.ZoneId))
			{
				_cachedAds.Remove(ad_.ZoneId);
			}

            RequestAd(ad_.ZoneId); // ad expired, call another to be cached

			CustomGameManager manager = GameFramework.GameStructure.GameManager.Instance as CustomGameManager;
			manager.UnmuteSound();

            if ( this.callback != null ) {
				this.callback (null, 0, true);
			}
		};

		AdColony.Ads.OnExpiring += (AdColony.InterstitialAd ad_) => {
            if (_cachedAds.ContainsKey(ad_.ZoneId))
            {
                _cachedAds.Remove(ad_.ZoneId);
            }

            RequestAd(ad_.ZoneId); // ad expired, call another to be cached

			MyDebug.Log("AdColony.Ads.OnExpiring called");
		};

		AdColony.Ads.OnRequestInterstitialFailed += () => {
			MyDebug.Log("AdColony.Ads.OnRequestInterstitialFailed called");
		};

		AdColony.Ads.OnRewardGranted += (string zoneId, bool success, string name, int amount) => {
			MyDebug.Log(string.Format("AdColony.Ads.OnRewardGranted called\n\tzoneId: {0}\n\tsuccess: {1}\n\tname: {2}\n\tamount: {3}", zoneId, success, name, amount));

			if (_cachedAds.ContainsKey(zoneId))
			{
				_cachedAds.Remove(zoneId);
			}

            RequestAd(zoneId); // ad expired, call another to be cached

			CustomGameManager manager = GameFramework.GameStructure.GameManager.Instance as CustomGameManager;
            manager.UnmuteSound();

			if ( this.callback != null ) {
				this.callback(zoneId, amount, success);
			}
		};

		ConfigureAds ();
	}

	void ConfigureAds() {
		// Configure the AdColony SDK
		MyDebug.Log("**** Configure ADC SDK ****");

		// Set some test app options with metadata.
		AdColony.AppOptions appOptions = new AdColony.AppOptions();
		appOptions.AdOrientation = AdColony.AdOrientationType.AdColonyOrientationPortrait;

		// AdColony zone ids
		string[] zoneIDs = new string[] { 
			Constants.AdColonyInterstitialZoneID, 
			Constants.AdColonyCoinsRewardZone, 
			Constants.AdColonyDoubleDailyBonus,
			Constants.AdColonyFreeLife,
			Constants.AdColonyDoubleCoins
		};

		AdColony.Ads.Configure(Constants.AdColonyAppID, appOptions, zoneIDs);
	}

	public void RequestAd(string zoneId = null) {
		if ( zoneId == null ) {
			zoneId = Constants.AdColonyInterstitialZoneID;
		}

        AdColony.InterstitialAd _ad = AdForZoneId(zoneId);

        if ( _ad != null && _ad.Expired == false ) { // we have an active ad for this zone, do not ask for another
            return;
        }

		// Request an ad.
        MyDebug.Log("Request Ad zone: " + zoneId);

		AdColony.AdOptions adOptions = new AdColony.AdOptions();
		adOptions.ShowPrePopup = false;
		adOptions.ShowPostPopup = false;

		AdColony.Ads.RequestInterstitialAd(zoneId, adOptions);
	}

    public AdColony.InterstitialAd AdForZoneId(string zoneId = null) {
		if (zoneId == null)
		{
			zoneId = Constants.AdColonyInterstitialZoneID;
		}

		if (_cachedAds.ContainsKey(zoneId))
		{
            return _cachedAds[zoneId];
		}

        return null;
    }

	public void PlayAd(AdColony.InterstitialAd ad = null) {
        if (ad != null && ad.Expired == false) {
            CustomGameManager manager = GameFramework.GameStructure.GameManager.Instance as CustomGameManager;
            manager.MuteSound();

			AdColony.Ads.ShowAd(ad);
		} else {
			if ( this.callback != null ) {
				this.callback (null, 0, false);
			}
		}
	}
}
