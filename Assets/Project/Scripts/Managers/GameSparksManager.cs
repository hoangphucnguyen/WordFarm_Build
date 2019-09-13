using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components;
using GameSparks.Core;
using GameSparks.Api;
using GameSparks.Api.Messages;
using GameFramework.Preferences;
using GameFramework.Preferences.PrefsEditorIntegration;
using GameFramework.GameStructure.GameItems.ObjectModel;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;
using System.Globalization;
using GameFramework.GameStructure;
using UnityEngine.SceneManagement;
using System;
using GameFramework.Messaging;
using GameFramework.GameStructure.Levels.ObjectModel;
using GameFramework.Helper;
using GameFramework.GameObjects;
using GameFramework.Facebook.Components;
using Facebook.Unity;
using GameFramework.Debugging;
using GameFramework.Facebook;
using System.Linq;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Localisation;
using GameFramework.Localisation.Messages;

public class UserLevelInfo {
	public int level;
	public float time;
	public int score;
	public string displayName;
	public string id;
	public string FBUserId;
	public bool me;
}

public enum GameMode {
	Single, 
	Multi
};

public class GameSparksManager : Singleton<GameSparksManager>
{
	private bool coldStart;
	private string _pushToken = null;
	private GSData _awards = null;

	private string _currentChallengeId;
	private bool _isMyturn;
	private GameMode _gameMode;
	private List <GSData> _words;
	public Dictionary<string, Texture2D> ProfileImages = new Dictionary<string, Texture2D>();

	public ChallengeManager.Challenge Challenger;
	public ChallengeManager.Challenge Challenged;
	public ChallengeManager.Challenge Opponent;

	public static string UserID() {
        if ( PreferencesFactory.HasKey(Constants.ProfileAnonymousUserId) ) {
            return PreferencesFactory.GetString(Constants.ProfileAnonymousUserId, "");
        }

		return PreferencesFactory.GetString (Constants.ProfileUserId, "");
	}

	protected override void GameSetup ()
	{
		coldStart = true;

		GS.GameSparksAuthenticated = (playerId) => {
			MyDebug.Log("GS Auth: " + playerId);

			if ( coldStart ) {
				SyncRestore();
				SyncLevels ();
			}

			if ( _pushToken != null ) {
				SetupPushToken();
			}

			if ( !coldStart ) { // if it is not an app cold start, then preferences are not synced and is safe to get awards
				GetAwards();
			}

            SetLanguage(LocaliseText.Language);

			coldStart = false;

            SendPoints(0, "UserAuth"); // to send all offline points if any
		};

		GSMessageHandler._AllMessages += HandleGameSparksMessageReceived;

		GameManager.SafeAddListener<UserLogoutMessage> (UserLogoutHandler);
		GameManager.SafeAddListener<UserLoginMessage> (UserLoginHandler);
		GameManager.SafeAddListener<UserRegisterMessage> (UserRegisterHandler);
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);

		UploadCompleteMessage.Listener += GetUploadMessage;

		//Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
	}

	public void OnTokenReceived(object sender/*, Firebase.Messaging.TokenReceivedEventArgs token*/) {
		/*MyDebug.Log("Received Registration Token: " + token.Token);

		_pushToken = token.Token;

		if ( IsUserLoggedIn() ) {
			SetupPushToken ();
		}*/
	}

	public void ClearMemory() {
		MyDebug.Log ("ClearMemory: " + ProfileImages.Count + " items");

		foreach ( KeyValuePair<string, Texture2D> entry in ProfileImages) {
			Texture2D tex = entry.Value;
			Destroy (tex);
		}

		ProfileImages.Clear ();
	}

	public void AnonymousLogin() {
		if ( !GS.Available ) {
			GS.Reconnect ();
		}

		new DeviceAuthenticationRequest ()
			.Send ((response) => {
                if (!response.HasErrors)
                {
                    if (string.IsNullOrEmpty(response.UserId) == false)
                    {
                        PreferencesFactory.SetString(Constants.ProfileAnonymousUserId, response.UserId);
                    }
                }
		});
	}

	void SetupPushToken() {
		new PushRegistrationRequest()
			.SetDeviceOS("FCM")
			.SetPushId(_pushToken)
			.Send((response) => {
				string registrationId = response.RegistrationId; 
				GSData scriptData = response.ScriptData; 

				MyDebug.Log("Received registrationId: " + registrationId);

				PlayerPrefs.SetString ("PushRegistrationId", registrationId);
				PlayerPrefs.Save ();
			});
	}

	void GetAwards() {
		new LogEventRequest ()
			.SetEventKey ("Awards")
			.Send ((response) => {
			if (!response.HasErrors && response.ScriptData != null) {
				_awards = response.ScriptData.GetGSData ("data");

				if ( _awards != null ) {
					JSONObject json = JSONObject.Parse(_awards.JSON);

					DialogInstance _awardsDialog = DialogManager.Instance.Show ("AwardDialog");

					AwardsController controller = _awardsDialog.GetComponent <AwardsController>();

					controller.SetAward (json);
				}
			}
		});
	}

	public void SearchOnlineUser(string username, Action <List<ChallengeManager.Challenge>> action) {
		new LogEventRequest ()
			.SetEventKey ("SearchOnlineUser")
			.SetEventAttribute ("displayName", username)
			.Send ((response) => {
				if (!response.HasErrors && response.ScriptData != null) {
					var results = response.ScriptData.GetGSDataList ("results");

					if ( results != null && action != null ) {
						List <ChallengeManager.Challenge> _challenges = new List<ChallengeManager.Challenge>();

						foreach ( GSData res in results ) {
							ListChallengeResponse._Challenge _ch = new ListChallengeResponse._Challenge(res);

							var ch = new ChallengeManager.Challenge
							{
								ChallengeId = _ch.ChallengeId,
								UserName = _ch.Challenger.Name,
								UserId = _ch.Challenger.Id,
								AvatarUploadId = _ch.ScriptData.GetString ("avatarUploadId"),
								ExternalIds = _ch.Challenger.ExternalIds
							};

							_challenges.Add (ch);
						}

						action(_challenges);
					}
				}
			});
	}

	protected override void GameDestroy()
	{
		GameManager.SafeRemoveListener<UserLogoutMessage> (UserLogoutHandler);
		GameManager.SafeRemoveListener<UserLoginMessage> (UserLoginHandler);
		GameManager.SafeRemoveListener<UserRegisterMessage> (UserRegisterHandler);
        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
	}

	void HandleGameSparksMessageReceived (GSMessage e)
	{
		MyDebug.Log ("MSG" + e.JSONString);
	}

	public void Logout() {
		string registrationId = PlayerPrefs.GetString ("PushRegistrationId", "");

		new LogEventRequest ()
			.SetEventKey ("Logout")
			.SetEventAttribute ("PushRegistrationId", registrationId)
			.Send (((response) => {
				GS.Disconnect ();
				GS.Reset ();
			}));

		PlayerPrefs.DeleteKey ("PushRegistrationId");

		if ( PlayerPrefs.HasKey ("LastProgressSyncDate") ) {
			PlayerPrefs.DeleteKey ("LastProgressSyncDate");
		}

		PlayerPrefs.Save ();
	}

	bool UserLogoutHandler(BaseMessage message) {
		Logout ();

		return true;
	}

	bool UserLoginHandler(BaseMessage message) {
		SyncRestore ();
		SyncLevels ();

        SendPoints(0, "UserLogin"); // to send all offline points if any

		return true;
	}

	bool UserRegisterHandler(BaseMessage message) {
		SyncProgress ();
		SyncLevels ();

        SendPoints(0, "UserRegister"); // to send all offline points if any

		return true;
	}

	bool LocalisationHandler(BaseMessage message)
	{
		SetLanguage(LocaliseText.Language);

		return true;
	}

	public static bool IsTokenAvailable() {
		var platform = GS.GSPlatform;
		var token = platform != null ? platform.AuthToken : null;
		return token != null && token != "0";
	}

	public static bool IsUserLoggedIn ()
	{
		return IsTokenAvailable () && PreferencesFactory.HasKey (Constants.ProfileUserId);
	}

    public void Log(GSRequestData json) {
        if (!GameSparksManager.IsTokenAvailable())
        {
            GameSparksManager.Instance.AnonymousLogin();
        }

        json.AddString("userId", UserID());
        json.AddDate("user_date", UnbiasedTime.Instance.Now());
        json.AddDate("utc_date", UnbiasedTime.Instance.UTCNow());

        new LogEventRequest()
            .SetEventKey("AppLog")
            .SetEventAttribute("data", json)
            .Send(((response) => {
                if (!response.HasErrors)
                {
                    
                }
            }));
    }

	public void SyncProgressCoroutine() {
		StartCoroutine (SyncCoroutine());
	}

	IEnumerator SyncCoroutine() {
		yield return new WaitForEndOfFrame ();

		SyncProgress ();
	}

	public void SyncProgress ()
	{
		if (!IsUserLoggedIn ()) {
			return;
		}

		DateTime now = UnbiasedTime.Instance.Now ();

		// when implement void SaveState(), add it here to save state before sync
		CustomFreePrizeManager.Instance.SaveState ();
		LevelController.Instance.SaveState ();

		PreferencesFactory.SetString ("LastProgressSyncDate", now.ToString (CultureInfo.InvariantCulture), false);
		PreferencesFactory.Save ();

		string json = JSONPrefs.String ();

		GSRequestData parsedJson = new GSRequestData (json);

		new LogEventRequest ()
			.SetEventKey ("PlayerProgress")
			.SetEventAttribute ("data", parsedJson)
			.Send (((response) => {
			if (!response.HasErrors) {
				PlayerPrefs.SetString ("LastProgressSyncDate", now.ToString (CultureInfo.InvariantCulture));
				PlayerPrefs.Save ();
			}
		}));
	}

	public void SyncRestore ()
	{
		if (!IsUserLoggedIn ()) {
			return;
		}

		new LogEventRequest ()
			.SetEventKey ("LoadPlayerProgress")
			.Send ((response) => {
			if (!response.HasErrors && response.ScriptData != null ) {
				GSData data = response.ScriptData.GetGSData ("data");

				if ( data == null ) {
					return;
				}
				
				DateTime now = UnbiasedTime.Instance.Now ();
				string json = data.JSON;

				// date from remote json
				DateTime syncDate = now;

				if ( data.ContainsKey ("LastProgressSyncDate") ) {
					syncDate = DateTime.Parse (data.GetString ("LastProgressSyncDate"));
				}

				// date in device when was last sync
				DateTime lastUpdateDate = DateTime.Parse (PlayerPrefs.GetString ("LastProgressSyncDate", now.ToString (CultureInfo.InvariantCulture)));
				MyDebug.Log ("Should update prefs; lastUpdateDate: " + lastUpdateDate + "; syncDate: " + syncDate);

				if ( syncDate.CompareTo (lastUpdateDate) != 0 ) {
                    MyDebug.Log ("Diff hash, update prefs");

					JSONPrefs.Replace (json);

					// updating device date to sync date, because on next start shouldn't update again
					PlayerPrefs.SetString ("LastProgressSyncDate", syncDate.ToString (CultureInfo.InvariantCulture));
					PlayerPrefs.Save ();

					CustomGameManager manager = CustomGameManager.Instance as CustomGameManager;
					manager.ResetGame();

					// reload data from sync
					LevelController.Instance.Reload ();

                    //

                    GSRequestData logData = new GSRequestData();
                    logData.AddString("key", "ProgressSync");
                    logData.AddString("message", "User synched progress from server");

                    GSData _d = new GSData(new Dictionary<string, object>(){
                        {"Date in JSON", syncDate}, 
                        {"Date in device", lastUpdateDate}
                    });

                    logData.AddObject("data", _d);

                    Log(logData);
				}

				GetAwards(); // safe place to get awards
			}
		});
	}

	public void SyncLevels() {
		if (!IsUserLoggedIn ()) {
			return;
		}

		JSONArray levels = new JSONArray ();
		int maxLevel = 1;

		foreach ( Level level in GameManager.Instance.Levels.Items ) {
			if ( level.IsUnlocked && level.TimeBest > 0.0f ) {
				JSONObject l = new JSONObject ();
				l.Add ("level", new JSONValue(level.Number));
				l.Add ("time", new JSONValue(level.TimeBest));

				levels.Add (l);

				maxLevel = level.Number;
			}
		}

		JSONObject d = new JSONObject ();
		d.Add ("levels", levels);
		d.Add ("max", maxLevel);

		string json = d.ToString ();

		GSRequestData parsedJson = new GSRequestData (json);

		new LogEventRequest ()
			.SetEventKey ("UserPlayedLevels")
			.SetEventAttribute ("levels", parsedJson)
			.Send (((response) => {
				if (!response.HasErrors) {
					
				}
			}));
	}

	public static void FriendsForLevel(int level, float time, Action<List<UserLevelInfo>> callback = null) {
		new LogEventRequest ()
			.SetEventKey ("FriendsPlayedLevel")
			.SetEventAttribute ("level", level)
			.Send (((response) => {
				if (!response.HasErrors) {
					List<UserLevelInfo> list = new List<UserLevelInfo>();

					var scriptData = response.ScriptData;

					// {"level":1,"time":34,"score":234,"displayName":"Maria Alafgeifbdfce Dingleescu","id":"590054ce08987a04bcd444f4","externalIds":{"FB":"102890916946229"}}
					foreach ( GSData r in scriptData.GetGSDataList ("friends") ) {
						UserLevelInfo info = new UserLevelInfo();

						info.level = level;
						info.time = (float)r.GetFloat ("time");
						info.displayName = r.GetString ("displayName");
						info.id = r.GetString ("id");
						info.FBUserId = r.GetGSData ("externalIds").GetString ("FB");
						info.me = false;
						info.score = (int)r.GetFloat ("score");

						list.Add (info);
					}

                    {
                        UserLevelInfo myInfo = new UserLevelInfo();

                        myInfo.level = level;
                        myInfo.time = time;
                        myInfo.displayName = LocaliseText.Get("LevelCompleted.Me");
                        myInfo.id = null;
                        myInfo.FBUserId = null;
                        myInfo.me = true;
                        myInfo.score = GameManager.Instance.Player.HighScore;

                        list.Add(myInfo);
                    }

                    list = list.OrderByDescending(o => o.score).ToList();

					if ( callback != null ) {
						callback(list);
					}
				}
			}));
	}

    public void UploadAvatar(Texture2D image, Action <Texture2D> action = null) {
		GSRequestData data = new GSRequestData ();
		data.AddString ("type", "avatar");

        Loading.Instance.Show();

		new GetUploadUrlRequest()
			.SetUploadData(data)
			.Send((response) => {
                Loading.Instance.Hide();

				if ( response.HasErrors ) {
					return;
				}

				string url = response.Url; 

                Loading.Instance.Show();
				StartCoroutine (AvatarUploading (url, image, action));
			});
	}

	public void DownloadAvatar(string uploadId, Action <Texture2D> action) {
		if (ProfileImages.ContainsKey (uploadId)) {
			action (ProfileImages [uploadId]);
			return;
		}

		new GetUploadedRequest()
			.SetUploadId(uploadId)
			.Send((response) => {
				if ( response.HasErrors ) {
					return;
				}

				var size = response.Size; 
				string url = response.Url; 

				DownloadAvatarUrl(url, uploadId, action);
			});
	}

	public void DownloadAvatarUrl(string url, string uploadId = null, Action <Texture2D> action = null) {
		if ( uploadId != null && ProfileImages.ContainsKey (uploadId)) {
			action (ProfileImages [uploadId]);
			return;
		}

		if ( uploadId == null && ProfileImages.ContainsKey (url)) {
			action (ProfileImages [url]);
			return;
		}

		StartCoroutine (DownloadImage(url, uploadId, action));
	}

	IEnumerator AvatarUploading(string uploadUrl, Texture2D tex, Action<Texture2D> action)
	{
		byte[] bytes = tex.EncodeToJPG();

		// Create a Web Form, this will be our POST method's data
		var form = new WWWForm();
		form.AddBinaryData("file", bytes, "avatar.jpg", "image/jpeg");

		WWW w = new WWW(uploadUrl, form); 

		yield return w;

        Loading.Instance.Hide();

        if (w.error != null && w.error != "")
		{
            DialogManager.Instance.ShowError (LocaliseText.Get("Profile.AvatarNotUploaded"));
		}
		else
		{
			DialogManager.Instance.ShowInfo(LocaliseText.Get("Profile.AvatarUploaded"));

            if ( action != null ) {
                action(tex);
            }
		}
	}

	void GetUploadMessage(GSMessage message)
	{
		var uploadId = message.BaseData.GetString("uploadId");

		new GetUploadedRequest()
			.SetUploadId(uploadId)
			.Send((response) => {
				GSData scriptData = response.ScriptData; 
				var size = response.Size; 
				string url = response.Url; 

				PreferencesFactory.SetString (Constants.ProfileAvatarUploadId, uploadId);
			});
	}

	IEnumerator DownloadImage(string url, string uploadId = null, Action <Texture2D> action = null)
	{
		var www = new WWW(url);

		yield return www;

		Texture2D downloadedImage = new Texture2D(256, 256);

		www.LoadImageIntoTexture(downloadedImage);

		if (uploadId != null && !ProfileImages.ContainsKey(uploadId))
			ProfileImages.Add(uploadId, downloadedImage);

		if (uploadId == null && !ProfileImages.ContainsKey(url))
			ProfileImages.Add(url, downloadedImage);

		if ( action != null ) {
			action (downloadedImage);
		}
	}

	public void LeaderboardRating(Action <int> action) {
		new GetLeaderboardEntriesRequest()
			.SetLeaderboards(new List <string>(){"allTime"})
			.Send((response) => {
				if ( response.HasErrors ) {
					return;
				}

				JSONObject results = JSONObject.Parse (response.JSONString);

				if ( action != null ) {
					action((int)results.GetObject ("allTime").GetNumber ("rank"));
				}
			});
	}


    public void SendPoints(int points, string source = null, JSONObject pointsJsonData = null) {
        int offlinePoints = PreferencesFactory.GetInt(Constants.KeyOfflinePoints, 0);
        string _pointsDataString = PreferencesFactory.GetString(Constants.KeyOfflinePointsData, null);

        if ( pointsJsonData != null && source != null ) {
            pointsJsonData.Add("source", source);
        }

        JSONArray _pointsData = new JSONArray();

        if ( !string.IsNullOrEmpty(_pointsDataString) )
        {
            try
            {
                _pointsData = JSONArray.Parse(_pointsDataString);
            } catch (Exception e) {}
        }

        if ( pointsJsonData != null ) { // add new record
            _pointsData.Add(pointsJsonData);
        }

        if (Reachability.Instance.IsReachable() == false || IsUserLoggedIn() == false) {
            PreferencesFactory.SetInt(Constants.KeyOfflinePoints, points + offlinePoints);
            PreferencesFactory.SetString(Constants.KeyOfflinePointsData, _pointsData.ToString());

            return;
        }

        if ( points + offlinePoints <= 0 ) {
            return;
        }

        int levelNumber = -1;
        try
        {
            Level level = LevelController.FirstUnplayedLevel();

            if (level != null)
            {
                levelNumber = level.Number - 1;
            }
        } catch(Exception e) {}

        GSRequestData requestData = new GSRequestData();
        requestData.AddNumber("offlinePoints", offlinePoints);
        requestData.AddNumber("score", points);
        requestData.AddNumber("lastPlayedLevel", levelNumber);

        if (_pointsData != null)
        {
            requestData.AddJSONStringAsObject("pointsData", _pointsData.ToString());
        }

        if ( source != null ) {
            requestData.AddString("source", source);
        }

		new LogEventRequest ()
			.SetEventKey("SubmitScoreV2")
			.SetEventAttribute("score", points + offlinePoints)
            .SetEventAttribute("data", requestData)
			.Send ((response) => {
            if ( !response.HasErrors ) {
                PreferencesFactory.DeleteKey(Constants.KeyOfflinePoints);
                PreferencesFactory.DeleteKey(Constants.KeyOfflinePointsData);
            }
        });
	}

	public void GetPoints(Action <int> action) {
		new LogEventRequest ()
			.SetEventKey ("UserPoints")
			.Send ((response) => {
				if ( response.HasErrors ) {
					return;
				}

				int points = (int)response.ScriptData.GetInt ("score");

				if ( action != null ) {
					action(points);
				}
			});
	}

    public void UserStats(Action <GSData> action) {
		new LogEventRequest()
			.SetEventKey("UserStats")
			.Send((response) =>
			{
				if (response.HasErrors)
				{
					return;
				}

                GSData stats = response.ScriptData.GetGSData("stats");

				if (action != null)
				{
					action(stats);
				}
			});
    }

	public void SetLanguage(string language)
	{
		if (!IsUserLoggedIn())
		{
			return;
		}

		new LogEventRequest()
			.SetEventKey("Language")
			.SetEventAttribute("language", language)
			.Send(((response) => {
				if (!response.HasErrors)
				{

				}
			}));
	}

    public void GetWords(Action<JSONArray> callback)
    {
        if (!IsTokenAvailable())
        {
            AnonymousLogin();
        }

        new LogEventRequest()
            .SetEventKey("GenerateWords")
            .SetEventAttribute("WordLength", "7")
            .SetEventAttribute("Language", LanguageUtils.RealLanguage(LocaliseText.Language))
            .Send((response) => {
                if (!response.HasErrors && response.ScriptData != null)
                {
                    _words = response.ScriptData.GetGSDataList("words");
                    
                    if (_words != null && callback != null)
                    {
                        JSONArray _arr = new JSONArray();

                        foreach (GSData _data in _words)
                        {
                            JSONObject _w = JSONObject.Parse(_data.JSON);
                            _arr.Add(_w);
                        }

                        callback(_arr);
                    }
                }
            });
    }

	//

	public List <GSData> GetWords()
	{
		return _words;
	}

	public void SetWords(List <GSData> words)
	{
		_words = words;
	}

	public GameMode GetGameMode()
	{
		return _gameMode;
	}

	public void SetGameMode(GameMode gameMode)
	{
		_gameMode = gameMode;
	}

	public string GetCurrentChallengeId()
	{
		return _currentChallengeId;
	}

	public void SetCurrentChallengeId(string challengeId)
	{
		_currentChallengeId = challengeId;
	}

	public void SetIsMyTurn(bool isMyturn)
	{
		_isMyturn = isMyturn;
	}

	public bool GetIsMyTurn()
	{
		return _isMyturn;
	}

	public void Reset()
	{
		_currentChallengeId = null;
		_isMyturn = false;
	}
}
