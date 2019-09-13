using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects;
using FlipWebApps.BeautifulTransitions.Scripts.Transitions.Components.GameObject;
using GameFramework.Facebook.Components;
using iSDK.Messenger.Scripts;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameStructure;
using GameFramework.Messaging;
using GameFramework.Facebook.Messages;
using System;
using GameFramework.Preferences;
using System.Globalization;
using GameFramework.Localisation;
using GameFramework.GameObjects.Components;
using DG.Tweening;
using GameFramework.Display.Other;
using Facebook.Unity;
using System.IO;
using GameFramework.Localisation.Messages;
using UnityEngine.UI;

public class InviteController : Singleton<InviteController>
{
    [SerializeField]
    private string ImageURL = string.Empty;
    [SerializeField]
    private GameObject balloon;

    private bool active;

    private Action _closeCallback;
    private Texture2D _cachedImage;
    private string _sharedImageName = "share.png";

	protected override void GameSetup()
	{
        SetupLinks();
	}

    private void Start()
    {
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);

        string path = Path.Combine(Application.persistentDataPath, _sharedImageName);

        // TODO
        //if (File.Exists(path))
        //{
        //    byte[] bytes = File.ReadAllBytes(path);
        //    
        //    _cachedImage = bytes.
        //}

        GameSparksManager.Instance.DownloadAvatarUrl(ImageURL, null, (Texture2D tex) => {
            if (tex == null)
            {
                return;
            }

            _cachedImage = tex;

            byte[] bytes = tex.EncodeToPNG();

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllBytes(path, bytes);
        });
    }

    void SetupLinks() {
        if (string.IsNullOrEmpty(ImageURL))
        {
            ImageURL = Constants.ShareImageURL;
        }
    }

    protected override void GameDestroy()
    {
        base.GameDestroy();

        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
    }

    bool LocalisationHandler(BaseMessage message)
    {
        SetupLinks();

        return true;
    }

    public void Show(Action callback = null)
    {
        this.active = true;

        if ( callback != null ) {
            _closeCallback = callback;
        }

        GameObjectHelper.GetChildNamedGameObject(gameObject, "Dialog", true).SetActive(true);

        GameManager.SafeAddListener<FacebookAppRequestMessage>(FacebookMessageHandler);

        if (!Debug.isDebugBuild)
        {
            Fabric.Answers.Answers.LogContentView("Invite", "Dialog");
        }

        PreferencesFactory.SetString(Constants.KeyInviteBalloonShowedDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));

        HideBalloon();
    }

    public void Close()
    {
        Invoke("HideDialog", 0.5f);

        this.active = false;
    }

    private void HideDialog()
    {
        if ( _closeCallback != null ) {
            _closeCallback.Invoke();
        }

        GameObjectHelper.GetChildNamedGameObject(gameObject, "Dialog", true).SetActive(false);
        GameManager.SafeRemoveListener<FacebookAppRequestMessage>(FacebookMessageHandler);
    }

    void Update()
    {
        if (this.active && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void FacebookInvite()
    {
        FacebookManager.Instance.AppRequest();
    }

    bool FacebookMessageHandler(BaseMessage message)
    {
        FacebookAppRequestMessage m = message as FacebookAppRequestMessage;

        if (m.Result == FacebookAppRequestMessage.ResultType.OK && m.InvitedFriends.Count > 0)
        {
            InviteBonusCoins();

            if (!Debug.isDebugBuild)
            {
                Flurry.Flurry.Instance.LogEvent("Share_Facebook");
                Fabric.Answers.Answers.LogInvite("Facebook");
                Fabric.Answers.Answers.LogShare("Facebook", contentId: "Invite");
            }
        }

        return true;
    }

    public void MessengerInvite()
    {
        MessengerShare.Instance.ShareDialog(new Uri(Constants.ShareURLLink(Constants.ShareCodes.Facebook)), new Uri(ImageURL),
                                            string.Format("{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.Facebook), Constants.HashTagSocials), null,
            doneCallback: MessengerInviteCallback);
    }

    void MessengerInviteCallback()
    {
        InviteBonusCoins();

        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("Share_Messenger");
            Fabric.Answers.Answers.LogInvite("Messenger");
            Fabric.Answers.Answers.LogShare("Messenger", contentId: "Invite");
        }
    }

    public static bool WillRewardInvite()
    {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyInviteLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last invite was yesterday
            return true;
        }

        int totalInvites = PreferencesFactory.GetInt(Constants.KeyInvitesTotal);

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalInvites < Constants.InviteMaxPerDay)
        {
            return true;
        }

        return false;
    }

    public static bool InviteBonusCoins()
    {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyInviteLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last invite was yesterday, reset all data
            PreferencesFactory.SetString(Constants.KeyInviteLastDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));
            PreferencesFactory.SetInt(Constants.KeyInvitesTotal, 0);

            dateTime = UnbiasedTime.Instance.Now();
        }

        bool addedCoins = false;

        int totalInvites = PreferencesFactory.GetInt(Constants.KeyInvitesTotal);

#if UNITY_EDITOR
        totalInvites = 0;
#endif

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalInvites < Constants.InviteMaxPerDay)
        {
			GameObject animatedCoins = GameObject.Find("AddCoinsAnimated");

			GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
			AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

            addCoins.AnimateCoinsAdding(Constants.InviteAwardCoins);

            addedCoins = true;
        }

        PreferencesFactory.SetInt(Constants.KeyInvitesTotal, totalInvites + 1);

        return addedCoins;
    }

    public void CopyLink()
    {
#if UNITY_IOS
        UniClipboard.SetText(Constants.ShareURLLink(Constants.ShareCodes.CopyLink));
#endif

#if UNITY_ANDROID
        UniClipboard.SetText (Constants.ShareURLLink(Constants.ShareCodes.CopyLink));
#endif

        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("Share_CopyLink");
            Fabric.Answers.Answers.LogInvite("CopyLink");
            Fabric.Answers.Answers.LogShare("CopyLink", contentId: "Invite");
        }

        DialogManager.Instance.Show(prefabName: "GeneralMessageOkButton",
            title: LocaliseText.Get("Invite.CopyLinkTitle"),
            text: LocaliseText.Get("Invite.CopyLinkText"),
                                     dialogButtons: DialogInstance.DialogButtonsType.Ok, doneCallback: (DialogInstance dialog) =>
                                      {
#if UNITY_EDITOR
            InviteBonusCoins ();
#endif
									  });
	}

    public void NativeInvite()
    {
        NativeInvite(_cachedImage);
    }

    public void NativeInvite(Texture2D tex) {
#if UNITY_IOS
        IOSSocialManager.OnMediaSharePostResult += HandleOnShareCallback;
        IOSSocialManager.Instance.ShareMedia(string.Format("{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.Native), Constants.HashTagSocials), tex);
#endif

#if UNITY_ANDROID
        AndroidSocialGate.OnShareIntentCallback += HandleOnShareIntentCallback;
        AndroidSocialGate.StartShareIntent(LocaliseText.Get("GameName"), string.Format("{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.Native), Constants.HashTagSocials), tex);
#endif
	}

    void HandleOnShareCallback(SA.Common.Models.Result result, string data)
    {
        IOSSocialManager.OnMediaSharePostResult -= HandleOnShareCallback;

        if (result.Error != null)
        {
            return;
        }

        InviteBonusCoins();

        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("Share_Native", new Dictionary<string, string>() { { "Share", data } });
            Fabric.Answers.Answers.LogInvite("Native", new Dictionary<string, object>() { { "Share", data } });
            Fabric.Answers.Answers.LogShare(data, contentId: "Invite");
        }
    }

    void HandleOnShareIntentCallback(bool status, string package)
    {
        AndroidSocialGate.OnShareIntentCallback -= HandleOnShareIntentCallback;

        InviteBonusCoins();

        if (!Debug.isDebugBuild)
        {
            Flurry.Flurry.Instance.LogEvent("Share_Native", new Dictionary<string, string>() { { "Share", package } });
            Fabric.Answers.Answers.LogInvite("Native", new Dictionary<string, object>() { { "Share", package } });
            Fabric.Answers.Answers.LogShare(package, contentId: "Invite");
        }
    }

	public void PrepareAndShowInviteBalloon() {
		DateTime now = UnbiasedTime.Instance.Now ();
		DateTime datetime;

		if (PreferencesFactory.HasKey (Constants.KeyInviteBalloonShowedDate)) {
			datetime = DateTime.Parse(PreferencesFactory.GetString (Constants.KeyInviteBalloonShowedDate, now.ToString(CultureInfo.InvariantCulture)));
		} else {
			datetime = now.AddDays (-1); // first launch, there is no date
		}

		if (datetime.Date < now.Date) {
			ShowBalloon ();
		}
	}

	public void ShowBalloon() {
		if ( balloon == null ) {
			return;
		}

		Vector3 currentScale = balloon.transform.localScale;

		balloon.transform.DOScale (new Vector3 (0, 0, 0), 0.0f);
		balloon.SetActive (true);

		balloon.transform.DOScale (currentScale, 1.0f).SetEase (Ease.OutElastic);
	}

	public void HideBalloon() {
		if ( balloon == null || balloon.activeInHierarchy == false ) {
			return;
		}

		balloon.transform.DOScale (new Vector3 (0, 0, 0), 1f).SetEase (Ease.OutElastic);
		StartCoroutine (CoRoutines.DelayedCallback (1.5f, DisableBalloon));
	}

	void DisableBalloon() {
		balloon.SetActive (false);
	}
}
