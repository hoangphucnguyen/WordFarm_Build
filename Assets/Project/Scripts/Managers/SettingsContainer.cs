using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components;
using GameFramework.GameStructure;
using GameFramework.Messaging;
using GameFramework.GameObjects;
using UnityEngine.UI;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Facebook.Components;
using GameFramework.Facebook.Messages;
using Facebook.Unity;
using GameFramework.Preferences;
using UnityEngine.EventSystems;
using GameFramework.Display.Other;
using GameSparks.Core;
using DG.Tweening;
using GameFramework.Localisation;
using System;
using FlipWebApps.BeautifulTransitions.Scripts.Transitions.Components.GameObject;
using UnityEngine.Advertisements;
using GameFramework.Localisation.Messages;
using PaperPlaneTools;

public class SettingsContainer : Singleton <SettingsContainer> {

	private bool _isSettingsOpened;
	public bool isSettingsOpened { get { return _isSettingsOpened; }}

	public GameObject mainContainer;
	private bool manualFacebookLogin;

	public string TosUrl;
	public string PrivacyUrl;

	// Use this for initialization
	void Start () {
		GameManager.SafeAddListener<SettingsOpenMessage> (SettingsOpenHandler);
		GameManager.SafeAddListener<SettingsCloseMessage> (SettingsCloseHandler);

		GameManager.SafeAddListener<FacebookLoginMessage>(OnFacebookLoginMessage);
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);
	}

	protected override void GameDestroy()
	{
		base.GameDestroy ();

		GameManager.SafeRemoveListener<SettingsOpenMessage> (SettingsOpenHandler);
		GameManager.SafeRemoveListener<SettingsCloseMessage> (SettingsCloseHandler);

		GameManager.SafeRemoveListener<FacebookLoginMessage>(OnFacebookLoginMessage);
        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
	}

	public void MainMenu() {
		PreferencesFactory.SetInt (Constants.KeyShowBannerOnMainMenuScreen, 1);
		GameManager.LoadSceneWithTransitions ("Menu");
	}

	public void SignUp() {
		DialogManager.Instance.Show ("SignUpDialog");
	}

	public void SignIn() {
		DialogManager.Instance.Show ("SignInDialog");
	}

	public void Logout() {
		UserHandler.Instance.Logout ();
		GameSparksManager.Instance.SyncProgress ();
	}

	public void ToggleBackgroundAudio() {
		ButtonUtils.PlayClickSound ();

		if (GameManager.Instance.BackGroundAudioVolume < 0.1f) {
            GameManager.Instance.BackGroundAudioSource.Play();
			GameManager.Instance.BackGroundAudioVolume = Constants.DefaultAudioVolume;
		} else {
            GameManager.Instance.BackGroundAudioSource.Stop();
			GameManager.Instance.BackGroundAudioVolume = 0f;

			if (!Debug.isDebugBuild) {
				Flurry.Flurry.Instance.LogEvent ("Sound_MusicOff");
				Fabric.Answers.Answers.LogCustom ("Sound_MusicOff");
			}
		}

		GameManager.Instance.SaveState ();
	}

	public void ToggleEffectsAudio() {
		ButtonUtils.PlayClickSound ();

		if (GameManager.Instance.EffectAudioVolume < 0.1f) {
			GameManager.Instance.EffectAudioVolume = Constants.DefaultAudioVolume;
		} else {
			GameManager.Instance.EffectAudioVolume = 0f;

			if (!Debug.isDebugBuild) {
				Flurry.Flurry.Instance.LogEvent ("Sound_SFXOff");
				Fabric.Answers.Answers.LogCustom ("Sound_MusicOff");
			}
		}

		GameManager.Instance.SaveState ();
	}

	public void ToggleNotifications() {
		ButtonUtils.PlayClickSound ();

		if ( PreferencesFactory.GetInt (Constants.KeyNotificationsAllowed, 1) == 1 ) {
			PreferencesFactory.SetInt (Constants.KeyNotificationsAllowed, 0);

			if (!Debug.isDebugBuild) {
				Flurry.Flurry.Instance.LogEvent ("Notifications_Off");
				Fabric.Answers.Answers.LogCustom ("Notifications_Off");
			}

            GameManager.SafeQueueMessage(new UserNotificationsChangedMessage(false));
		} else {
			PreferencesFactory.SetInt (Constants.KeyNotificationsAllowed, 1);

			if ( !LocalNotifications.Allowed () ) {
				LocalNotifications.AllowDialog (() => {
					PreferencesFactory.SetInt (Constants.KeyNotificationsAllowed, 0);

					GameObject _n = GameObjectHelper.GetChildNamedGameObject (gameObject, "Notifications", true);
					Switch _switch = GameObjectHelper.GetChildComponentOnNamedGameObject<Switch> (_n, "Switch", true);

					_switch.SetOn (false);
				});
            } else {
                GameManager.SafeQueueMessage(new UserNotificationsChangedMessage(true));
            }
		}
	}

	public void PrivacyPolicy() {
		if (PrivacyUrl != null) {
			Application.OpenURL (PrivacyUrl);
		}
	}

	public void Tos() {
		if (TosUrl != null) {
			Application.OpenURL (TosUrl);
		}
	}

	public void Contact() {
		DialogManager.Instance.Show ("ContactDialog");
	}

	void Update() {
		if (this.isSettingsOpened && Input.GetKeyDown(KeyCode.Escape)) {
			SettingsClose ();
		}
	}

	public void CloseButton() {
		StartCoroutine (CoRoutines.DelayedCallback (Constants.DelayButtonClickAction, SettingsClose));
	}

	bool SettingsOpenHandler(BaseMessage message) {
		if (!this.isSettingsOpened) {
			SettingsOpen ();
		} else {
			SettingsClose ();
		}

		return true;
	}

	bool SettingsCloseHandler(BaseMessage message) {
		if (this.isSettingsOpened) {
			SettingsClose ();
		}

		return true;
	}

	private void SettingsOpen() {
		if ( this.isSettingsOpened ) {
			SettingsClose ();
			return;
		}

        SetInviteButtonText();
        SetRateButtonText();

		GameManager.SafeTriggerMessage (new SettingsStateChanged(true));

		_isSettingsOpened = true;

		SettingsTransform (0.3);
	}

	private void SettingsTransform(double time) {
		if (this.isSettingsOpened) {
			GameObjectHelper.GetChildNamedGameObject (gameObject, "Dialog", true).SetActive (true);
		} else {
			Invoke ("HideDialog", 0.5f);
		}
	}

	private void HideDialog () {
		GameObjectHelper.GetChildNamedGameObject (gameObject, "Dialog", true).SetActive (false);
	}

	private void SettingsClose() {
		SettingsCloseComplete ();
	}

	public void SettingsCloseComplete() {
		_isSettingsOpened = false;

		GameManager.SafeTriggerMessage (new SettingsStateChanged(false));
	}

	public void FacebookLogin() {
		this.manualFacebookLogin = true;

		Loading.Instance.Show ();
		FacebookManager.Instance.Login ();
	}

	public void FacebookLogout() {
		DialogManager.Instance.Show (prefabName: "GameQuitMessage", 
			title: LocaliseText.Get ("Text.Logout"), 
			text: LocaliseText.Get ("Text.Sure"), 
			doneCallback: FacebookLogoutCallback,
			dialogButtons: DialogInstance.DialogButtonsType.OkCancel);

		GameSparksManager.Instance.SyncProgress ();
	}

	public void FacebookInvite() {
		FacebookManager.Instance.AppRequest();
	}

	void FacebookLogoutCallback(DialogInstance dialogInstance) {
		if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok) {
			UserHandler.Instance.LogoutProcess ();
		}
	}

	bool OnFacebookLoginMessage(BaseMessage message)
	{
		Loading.Instance.Hide ();

		var facebookLoginMessage = message as FacebookLoginMessage;

		if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.OK)
		{
			if (this.manualFacebookLogin) {
				PreferencesFactory.SetFlag (Constants.KeyFacebookConnected);

				UserHandler.Instance.ConnectedWithFacebook ();

				FacebookManager.Instance.AutoConnectOnStartup = true;

				DialogManager.Instance.Show (prefabName: "GeneralMessageOkButton", 
					title: "Facebook", 
					text: LocaliseText.Get ("Text.FacebookConnectedSuccess"), 
					dialogButtons: DialogInstance.DialogButtonsType.Ok);
			}
		}

		if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.CANCELLED) {

		}

		if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.ERROR) {
			DialogManager.Instance.Show (prefabName: "GeneralMessageOkButton", 
				title: "Facebook", 
				text: LocaliseText.Get ("Text.FacebookConnectedFails"), 
				dialogButtons: DialogInstance.DialogButtonsType.Ok);
		}

		this.manualFacebookLogin = false;

		return true;
	}

	bool LocalisationHandler(BaseMessage message)
	{
        SetInviteButtonText();
        SetRateButtonText();

		return true;
	}

	public void Leaderboard() {
		int CountClicks = PreferencesFactory.GetInt (Constants.KeyCountLeaderboardClicks);
		PreferencesFactory.SetInt (Constants.KeyCountLeaderboardClicks, CountClicks + 1);

  //      if (CountClicks > 0 && CountClicks % Constants.ShowAdsPerTime == 0 && Reachability.Instance.IsReachable ()) {
		//	Loading.Instance.Show ();

		//	AdColonyManager.Instance.SetCallback(BannerLeaderboardClosed);
		//	AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());
		//} else {
			LeaderboardProcess ();
		//}
	}

	void BannerLeaderboardClosed(string zoneId, int amount, bool success) {
		if ( !success ) {
			ShowVideoAd (BannerLeaderboardClosedChartboost);
			return;
		}

		Loading.Instance.Hide ();
		LeaderboardProcess ();
	}

	void BannerLeaderboardClosedChartboost(int amount, bool success) {
		Loading.Instance.Hide ();
		LeaderboardProcess ();
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
			Loading.Instance.Hide();
			action(0, false);
		}
	}

	void LeaderboardProcess() {
		DialogManager.Instance.Show ("LeaderBoard");
	}

	public void Redirect(int redirect) {
		if ( redirect == Constants.KeyRedirectLeaderboard ) {
			Leaderboard ();
		}
	}

	public void ProfileButton() {
		if ( !GameSparksManager.IsUserLoggedIn () ) {
			DialogManager.Instance.Show ("SignInDialog");
			return;
		}

		DialogManager.Instance.Show ("Profile");
	}

	public void HowToPlayButton() {
		DialogManager.Instance.Show ("HowToPlay");
	}

	public void Language()
	{
		DialogManager.Instance.Show("Language");
	}

    static GameObject _inviteButtonObject;

	public void Invite()
	{
        GameObject _buttonObject = GameObject.Find("PurchaseButton");

		if (_buttonObject == null)
		{
			return;
		}

		GameObject originalParent = _buttonObject.transform.parent.gameObject;
		Vector3 originalPosition = _buttonObject.transform.position;

        InviteController.Instance.Show(() => {
			_inviteButtonObject = null;
			GameObjectUtils.MoveObjectTo(_buttonObject, originalParent, originalPosition);
        });

		_inviteButtonObject = InviteController.Instance.gameObject;
        DialogInstance _inviteDialogInstance = InviteController.Instance.GetComponent<DialogInstance>();

		GameObjectUtils.MoveObjectTo(_buttonObject, _inviteDialogInstance.Target);
	}

    void SetInviteButtonText()
    {
        GameObject _inviteButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "InviteButton", true);
        GameObject _awardObject = GameObjectHelper.GetChildNamedGameObject(_inviteButton, "InviteAward", true);

        if (InviteController.WillRewardInvite())
        {
            _awardObject.SetActive(true);

            _awardObject.GetComponent<Text>().text = string.Format("+{0}", Constants.InviteAwardCoins);
        }
        else
        {
            _awardObject.SetActive(false);
        }
    }

    public void ShowRate() {
        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("AppRate_Settings");
            Fabric.Answers.Answers.LogCustom("AppRate_Settings");
        }

        RateBox.Instance.GoToRateUrl();
    }

    void SetRateButtonText() {
        GameObject _rateAward = GameObjectHelper.GetChildNamedGameObject(gameObject, "RateAward", true);
        Text _rateText = _rateAward.GetComponent<Text>();

        _rateText.text = string.Format("+{0}", Constants.RateRewardCoins);

        _rateText.font = GeneralUtils.FontForCurrentLanguage(_rateText.fontStyle);

        if (CustomRateWindow.WillReward())
        {
            _rateAward.SetActive(true);
        } else {
            _rateAward.SetActive(false);
        }
    }
}
