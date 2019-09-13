using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Requests;
using UnityEngine.UI;
using GameFramework.Preferences;
using GameFramework.GameObjects;
using GameFramework.GameStructure;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Display.Other;
using GameSparks.Core;
using GameSparks.Api.Responses;
using GameFramework.Facebook.Components;
using GameFramework.Messaging;
using GameFramework.Facebook.Messages;
using GameFramework.Debugging;
using GameFramework.Localisation;

public class LobbyController : MonoBehaviour, ChallengeManager.IOnChallengeDetect {

	[SerializeField]
	private GameObject _scrollView;
	[SerializeField]
	private GameObject _usersPrefab;
	[SerializeField]
	private InputField _searchField;
    [SerializeField]
    private Animation _loadingAnimation;
    [SerializeField]
    private GameObject _activityIndicator;

	private bool _isUserLoggedIn { get{ return GameSparksManager.IsUserLoggedIn (); }}
	private Dictionary <string, GameObject> _mapUsersAvatar = new Dictionary<string, GameObject>();

	void Start () {
		if (!GameSparksManager.IsTokenAvailable ()) {
			GameSparksManager.Instance.AnonymousLogin ();
		}

		GameManager.SafeAddListener<FacebookProfilePictureMessage> (FacebookProfilePictureHandler);

		ChallengeManager.Instance.SetOnChallengeDetected(this);

		StartGame ();
	}

	void OnDestroy() {
		GameManager.SafeRemoveListener<FacebookProfilePictureMessage> (FacebookProfilePictureHandler);
	}

	void StartGame() {
		if (_isUserLoggedIn)
		{
			ChallengeManager.Instance.CreateMatch();
		}
	}

	public void CloseButton() {
		ChallengeManager.Instance.SetOnChallengeDetected(null);
		LeaveLoby ();

		StartCoroutine (CoRoutines.DelayedCallback (Constants.DelayButtonClickAction, () => {
			GameManager.LoadSceneWithTransitions ("Menu");
		}));
	}

	public void LeaderboardButton() {
		DialogManager.Instance.Show ("LeaderBoard");
	}

	public void Searching() {
		var username = _searchField.text;

		if ( username.Length == 0 ) {
			ChallengeManager.Instance.RefreshChallengesList ();

			return;
		}

        _activityIndicator.SetActive(true);

		GameSparksManager.Instance.SearchOnlineUser (username, (List <ChallengeManager.Challenge> challenges) => {
			_mapUsersAvatar.Clear ();

			foreach (Transform child in _scrollView.transform)
			{
				Destroy(child.gameObject);
			}

			foreach (var challenge in challenges) {
				var prefabButton = Instantiate(_usersPrefab);
				prefabButton.transform.SetParent(_scrollView.transform, false);

				var nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text> (prefabButton, "Name", true);
				nameText.text = challenge.UserName;

				if ( challenge.AvatarUploadId != null ) {
					var image = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage> (prefabButton, "AvatarImage", true);

					GameSparksManager.Instance.DownloadAvatar (challenge.AvatarUploadId, (Texture2D tex) => {
						if ( tex != null && image != null ) {
							image.texture = tex;
						}
					});
				} else {
					string FBUserId = challenge.ExternalIds == null ? null : challenge.ExternalIds.GetString ("FB");

					if ( FBUserId != null && !_mapUsersAvatar.ContainsKey (FBUserId) ) {
						_mapUsersAvatar.Add (FBUserId, prefabButton);

						FacebookRequests.Instance.LoadProfileImages (FBUserId);
					}
				}

				var inside = prefabButton.GetComponent<Button>();
				var challenge1 = challenge;
				inside.onClick.AddListener(delegate
					{
						ButtonUtils.PlayClickSound ();

						if (_isUserLoggedIn)
						{
							if (!Reachability.Instance.IsReachable ())
							{
								DialogManager.Instance.ShowInfo (LocaliseText.Get("GeneralMessage.NoInternet"));
							}
							else
							{
								LeaveLoby();
								ChallengeManager.Instance.SendJoinChallengeRequest(challenge1.ChallengeId);
							}
						}
						else
						{
							DialogManager.Instance.Show ("SignInDialog");
						}
					});
			}

            _activityIndicator.SetActive(false);
		});
	}

    public void RefreshButton() {
        if ( _loadingAnimation.isPlaying ) {
            return;
        }

        _loadingAnimation.wrapMode = WrapMode.Loop;
        _loadingAnimation.Play();

        Refresh();
    }

	public void Refresh() {
		ChallengeManager.Instance.RefreshChallengesList ();
	}

	bool FacebookProfilePictureHandler(BaseMessage message) {
		FacebookProfilePictureMessage m = message as FacebookProfilePictureMessage;

		if ( _mapUsersAvatar.ContainsKey (m.UserId) ) {
			GameObject item = _mapUsersAvatar [m.UserId];

			var image = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage> (item, "AvatarImage", true);
			image.texture = m.Texture;
		}

		return true;
	}

	public void LeaveLoby()
	{
		if (_isUserLoggedIn)
		{
			new LogEventRequest()
				.SetEventKey("leaveLoby")
				.Send((response) => { });
		}
	}

	//

	public void OnChallengeStarted(bool isMyturn, string name, string opponentName)
	{
		
	}

    public void OnPositionDetected(List<DrawQueueItem> items, Vector3 center)
	{
		
	}

	public void OnTurnEnd(Vector3 position, string word) {
		
	}

	public void OnChallengeFinishedEvent(ChallengeManager.GameStates state,
		ChallengeManager.GameStateMessage message)
	{
		MyDebug.Log("OnChallengeFinishedEvent: " + message);
	}

	public void OnChallengeIssued(string challengeId, string username)
	{
		MyDebug.Log("OnChallengeIssued: " + username);

		// auto cancel challenge, user leaved gameboard already
		ChallengeManager.Instance.CancelChallenge (challengeId);
	}

	public void OnChallengeDeclined(string playerName)
	{
		MyDebug.Log("OnChallengeDeclined: " + playerName);
	}

	public void OnErrorReceived(string message)
	{
		MyDebug.Log("OnErrorReceived: " + message);

		if ( message == "REJOIN_LOBBY" ) {
			LeaveLoby ();
			StartGame ();
        } else {
            DialogManager.Instance.ShowInfo(message);
        }
	}

	public void OnChallengesListeFetched(List<ChallengeManager.Challenge> challenges)
	{
        if ( _searchField == null || _searchField.text.Length > 0 ) {
			return;
		}

        _activityIndicator.SetActive(false);

		_mapUsersAvatar.Clear ();

		var myUserId = PreferencesFactory.GetString (Constants.ProfileUserId);

		foreach (Transform child in _scrollView.transform)
		{
			Destroy(child.gameObject);
		}

		foreach (var challenge in challenges) {
			var prefabButton = Instantiate(_usersPrefab);
			prefabButton.transform.SetParent(_scrollView.transform, false);

			var nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text> (prefabButton, "Name", true);
			nameText.text = challenge.UserName;

			var avatar = challenge.AvatarUploadId;

			if (avatar != null) {
				var image = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage> (prefabButton, "AvatarImage", true);

				GameSparksManager.Instance.DownloadAvatar (avatar, (Texture2D tex) => {
					if ( tex != null && image != null ) {
						image.texture = tex;
					}
				});
			} else {
				string FBUserId = challenge.ExternalIds == null ? null : challenge.ExternalIds.GetString ("FB");

				if ( FBUserId != null && !_mapUsersAvatar.ContainsKey (FBUserId) ) {
					_mapUsersAvatar.Add (FBUserId, prefabButton);

					FacebookRequests.Instance.LoadProfileImages (FBUserId);
				}
			}

			if (myUserId != challenge.UserId)
			{
				var inside = prefabButton.GetComponent<Button>();
				var challenge1 = challenge;
				inside.onClick.AddListener(delegate
					{
						ButtonUtils.PlayClickSound ();

						if (_isUserLoggedIn)
						{
							if (!Reachability.Instance.IsReachable ())
							{
								DialogManager.Instance.ShowInfo (LocaliseText.Get("GeneralMessage.NoInternet"));
							}
							else
							{
								LeaveLoby();
								ChallengeManager.Instance.SendJoinChallengeRequest(challenge1.ChallengeId);
							}
						}
						else
						{
							DialogManager.Instance.Show ("SignInDialog");
						}
					});
			}
		}

        _loadingAnimation.wrapMode = WrapMode.Once;
	}

    public void OnWordFound(string word, WordState state) {
		
	}
}
