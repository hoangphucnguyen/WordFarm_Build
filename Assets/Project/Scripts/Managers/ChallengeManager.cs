using System;
using System.Collections.Generic;
using System.Linq;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameFramework.Preferences;
using GameFramework.GameObjects.Components;
using GameFramework.GameStructure;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Debugging;
using GameFramework.Localisation;
using System.Collections;
using GameFramework.Display.Other;

public class ChallengeManager : Singleton <ChallengeManager>
{
    public enum GameStates
    {
        Won = 0,
        Lost = 1,
        Draw = 2,
        Leaved = 3
    }

    public class Challenge
    {
        public string UserName;
		public string UserId;
        public string ChallengeId;
		public string AvatarUploadId;
		public GSData ExternalIds;

		public override string ToString ()
		{
			return string.Format ("[Challenge]: UserName: {0}; UserId: {1}; ChallengeId: {2}; AvatarUploadId: {3}; ExternalIds: {4}", UserName, UserId, ChallengeId, AvatarUploadId, ExternalIds.JSON);
		}
    }

    public class GameStateMessage
    {
        public string Message;
        public int Points;
    }

    public bool ShouldGetChallengesList;

    private string _currentUserId;
    private string _matchId;
    private string _currentChallengeId;
    private bool _isRematch;

    private readonly List<Challenge> _challenges = new List<Challenge>();
    private IOnChallengeDetect _onChallengeDetected;
    private IOnAchievementDetected _onAchievementDetected;
    private IEnumerator _refreshCoroutine;
    float _challengeRefreshRate = 8f;

    public string GetCurrentChallengeId()
    {
        return _currentChallengeId;
    }

    public void SetOnChallengeDetected(IOnChallengeDetect onChallengeDetected)
    {
        _onChallengeDetected = onChallengeDetected;
    }

    public void SetOnIOnAchievementDetected(IOnAchievementDetected onAchievementDetected)
    {
        _onAchievementDetected = onAchievementDetected;
    }

    public interface IOnChallengeDetect
    {
		void OnChallengeStarted(bool isMyturn, string name, string opponentName);

        void OnPositionDetected(List<DrawQueueItem> items, Vector3 center);

        void OnWordFound(string word, WordState state);

		void OnTurnEnd(Vector3 position, string word);

        void OnChallengeFinishedEvent(GameStates state, GameStateMessage message);

        void OnChallengeIssued(string challengeId, string username);

        void OnChallengesListeFetched(List<Challenge> challenges);

        void OnErrorReceived(string message);

        void OnChallengeDeclined(string playerName);
    }

    public interface IOnAchievementDetected
    {
        void OnAchievmentEarned(string name);
    }

    private void Start()
    {
		_currentUserId = PreferencesFactory.GetString (Constants.ProfileUserId);

        if (ShouldGetChallengesList)
        {
            RefreshChallengesList();
        }

        GS.GameSparksAuthenticated = (playerId) => { 
            _currentUserId = playerId;

            // connection was interupted
            // user now reconnect but challenge was canceled
            if ( GameSparksManager.Instance.GetCurrentChallengeId() != null ) {
                if (_onChallengeDetected != null)
                {
                    _onChallengeDetected.OnErrorReceived(LocaliseText.Get("Challenge.NotAvailable"));
                }
            }
        };

        ChallengeIssuedMessage.Listener = (listener) =>
        {
            _currentChallengeId = listener.Challenge.ChallengeId;
			GameSparksManager.Instance.SetCurrentChallengeId(_currentChallengeId);

            var challengerId = listener.Challenge.Challenger.Id;
            if (_currentUserId != challengerId)
            {
                var username = listener.Challenge.Challenger.Name;

                if (_onChallengeDetected != null)
                {
                    if (!_isRematch)
                    {
                        _onChallengeDetected.OnChallengeIssued(_currentChallengeId, username);
                    }
                    else
                    {
                        new AcceptChallengeRequest()
                            .SetChallengeInstanceId(_currentChallengeId)
                            .Send((response) => { });
                    }
                }
            }
        };

        ChallengeStartedMessage.Listener = (listener) =>
        {
            if (!listener.HasErrors)
            {
                var challenge = listener.Challenge;
                if (challenge != null)
                {
                    if (_currentUserId == null)
                    {
						_currentUserId = PreferencesFactory.GetString (Constants.ProfileUserId);
                    }

                    _currentChallengeId = challenge.ChallengeId;

					GameSparksManager.Instance.SetCurrentChallengeId(_currentChallengeId);

					var acceptedPlayersEnumerator = challenge.Accepted.GetEnumerator ();
					var challengedEnumerator = challenge.Challenged.GetEnumerator();

					var challenger = new Challenge
					{
						ChallengeId = challenge.ChallengeId,
						UserName = challenge.Challenger.Name,
						UserId = challenge.Challenger.Id,
						AvatarUploadId = challenge.ScriptData.GetGSData (challenge.Challenger.Id).GetString ("avatarUploadId"),
						ExternalIds = challenge.ScriptData.GetGSData (challenge.Challenger.Id).GetGSData("externalIds")
					};

					GameSparksManager.Instance.Challenger = challenger;

                    while (challengedEnumerator.MoveNext())
                    {
                        var playerDetail = challengedEnumerator.Current;
                        if (playerDetail != null)
                        {
							var challenged = new Challenge
							{
								ChallengeId = challenge.ChallengeId,
								UserName = playerDetail.Name,
								UserId = playerDetail.Id,
								AvatarUploadId = challenge.ScriptData.GetGSData (playerDetail.Id).GetString ("avatarUploadId"),
								ExternalIds = challenge.ScriptData.GetGSData (playerDetail.Id).GetGSData("externalIds")
							};

							GameSparksManager.Instance.Challenged = challenged;
                        }
                    }
                    challengedEnumerator.Dispose();

					while ( acceptedPlayersEnumerator.MoveNext () ) {
						var playerDetail = acceptedPlayersEnumerator.Current;
						if (playerDetail != null)
						{
							if ( playerDetail.Id != _currentUserId ) {
								var opponent = new Challenge
								{
									ChallengeId = challenge.ChallengeId,
									UserName = playerDetail.Name,
									UserId = playerDetail.Id,
									AvatarUploadId = challenge.ScriptData.GetGSData (playerDetail.Id).GetString ("avatarUploadId"),
									ExternalIds = challenge.ScriptData.GetGSData (playerDetail.Id).GetGSData("externalIds")
								};

								GameSparksManager.Instance.Opponent = opponent;
							}
						}
					}

					acceptedPlayersEnumerator.Dispose();

                    var nextPlayerId = challenge.NextPlayer;

                    var isMyTurn = nextPlayerId == _currentUserId;
					GameSparksManager.Instance.SetIsMyTurn(isMyTurn);
					List <GSData> words = null;

                    var gsData = listener.Challenge.ScriptData;

					_isRematch = false;

					GameSparksManager.Instance.SetGameMode (GameMode.Multi);

                    if (gsData != null)
                    {
						words = gsData.GetGSDataList ("words");

						GameSparksManager.Instance.SetWords (words);

                        if (_onChallengeDetected != null)
                        {
							_onChallengeDetected.OnChallengeStarted(isMyTurn, GameSparksManager.Instance.Challenger.UserName, GameSparksManager.Instance.Challenged.UserName);
                        }
                    }

					if ( words == null ) {
                        DialogManager.Instance.ShowError (LocaliseText.Get("Game.ChallengerNotCreated"), doneCallback:(DialogInstance dialogInstance) => {
							if (_onChallengeDetected != null)
							{
								_onChallengeDetected.OnErrorReceived ("REJOIN_LOBBY");
							}
						});

						return;
					}

                    if (SceneManager.GetActiveScene().name != "Game")
                    {
						GameManager.LoadSceneWithTransitions ("Game");
                    }
                }
            }
            else
            {
                MyDebug.Log("Challenge Started Error: " + listener.Errors.JSON.ToString());
            }
        };

        ChallengeTurnTakenMessage.Listener = (listener) =>
        {
            if (!listener.HasErrors)
            {
                var challenge = listener.Challenge;
                _currentChallengeId = challenge.ChallengeId;

				var nextPlayerId = challenge.ScriptData.GetString ("nextPlayer");
                var isMyTurn = nextPlayerId == _currentUserId;

                GameSparksManager.Instance.SetIsMyTurn(isMyTurn);
                
				var scriptData = challenge.ScriptData;

				if (scriptData != null)
                {
					var eventKey = scriptData.GetString ("EventKey");

					if (eventKey == "WordFound" && scriptData != null
						&& scriptData.GetString("SenderPlayerId") != _currentUserId 
                        && scriptData.ContainsKey("WordFound") ) 
                    {
                        GSData wordFound = scriptData.GetGSData("WordFound");

                        if ( wordFound.ContainsKey("state") )
                        {
                            WordState state = (WordState)wordFound.GetInt("state");
                            string word = wordFound.GetString("word");

                            if (_onChallengeDetected != null)
                            {
                                _onChallengeDetected.OnWordFound(word, state);
                            }
                        } else { // backward compability
							if (scriptData.ContainsKey("found_words"))
							{
								List<GSData> found_words = scriptData.GetGSDataList("found_words");

								foreach (GSData _w in found_words)
								{
									string word = _w.GetString("word");
									string player_id = _w.GetString("player_id");

									if (_onChallengeDetected != null && player_id != _currentUserId)
									{
                                        _onChallengeDetected.OnWordFound(word, WordState.Found);
									}
								}
							}
                        }
                    }

                    var wordDrag = scriptData.GetGSData("WordDrag");

                    if ( eventKey == "WordDrag" && wordDrag != null 
                        && wordDrag.ContainsKey("positions") )
                    {
						if (scriptData.GetString ("SenderPlayerId") != _currentUserId)
	                    {
                            List<DrawQueueItem> _positions = new List<DrawQueueItem>();
                            var center = Vector3.zero;

                            if (wordDrag.ContainsKey("positions"))
                            {
                                List<GSData> _l = wordDrag.GetGSDataList("positions");

                                foreach ( GSData _d in _l ) {
                                    string json = _d.GetString("item");

                                    _positions.Add(new DrawQueueItem(json));
                                }
                            }

                            if ( wordDrag.ContainsKey("center_x") && wordDrag.ContainsKey("center_y") ) {
								float cx = (float)wordDrag.GetFloat("center_x");
								float cy = (float)wordDrag.GetFloat("center_y");
								float cz = (float)wordDrag.GetFloat("center_z");

                                center = new Vector3(cx, cy, cz);
                            }

                            if (_onChallengeDetected != null)
                            {
								_onChallengeDetected.OnPositionDetected(_positions, center);
                            }
						}
					} 

					if ( ((eventKey == "takeTurn" || eventKey == null) 
						&& scriptData.GetString ("SenderPlayerId") != _currentUserId 
						&& scriptData.GetString ("SenderPlayerId") != nextPlayerId) 
						|| scriptData.GetBoolean ("TimeOut") != null) {
						if (_onChallengeDetected != null )
						{
							_onChallengeDetected.OnTurnEnd (Vector3.zero, "word");
						}
					}
                }
            }
            else
            {
                MyDebug.Log("Challenge Turn Taken Error: " + listener.Errors.JSON.ToString());
            }
        };

        ChallengeDeclinedMessage.Listener = (listener) =>
        {
            if (!listener.HasErrors)
            {
                var challenge = listener.Challenge;
                if (challenge != null)
                {
                    var declined = challenge.Declined;
                    var enumerator = declined.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var playerDetail = enumerator.Current;
                        var playerName = playerDetail.Name;
                        if (_onChallengeDetected != null)
                        {
                            _onChallengeDetected.OnChallengeDeclined(playerName);
                        }
                    }
                    enumerator.Dispose();
                }
            }
        };

        ChallengeExpiredMessage.Listener = (listener) => { RefreshChallengesList(); };

        ScriptMessage.Listener = (listener) =>
        {
            if (!listener.HasErrors)
            {
                var extCode = listener.ExtCode;
                if (extCode == "userLeaved")
                {
                    var gameStateMessage = new GameStateMessage
                    {
                        Message = listener.Summary,
                        Points = 0
                    };
                    if (_onChallengeDetected != null)
                    {
                        _onChallengeDetected.OnChallengeFinishedEvent(GameStates.Leaved, gameStateMessage);
                    }
                }

                if ( extCode == "userTurn" ) {
                    
                }
            }
        };

        ChallengeWonMessage.Listener = (listener) =>
        {
            if (listener.HasErrors) return;

            if ( StringUtils.IsNullOrWhiteSpace(_currentChallengeId))
            {
                _currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
            }
            if (_currentChallengeId != null && _currentChallengeId.Equals(listener.Challenge.ChallengeId))
            {
                var scriptData = listener.Challenge.ScriptData;
                if (scriptData != null)
                {
                    var points = 0;
					var point = scriptData.GetInt("winPoints");
                    if (point != null)
                    {
                        points = (int) point;
                    }
                    
                    var message = LocaliseText.Get("Game.CongratsYouWon");

                    if (_onChallengeDetected != null)
                    {
                        var gameStateMessage = new GameStateMessage
                        {
                            Message = message,
                            Points = points
                        };
                        _onChallengeDetected.OnChallengeFinishedEvent(GameStates.Won,
                            gameStateMessage);
                    }
                }
            }
        };
        ChallengeDrawnMessage.Listener = (listener) =>
        {
            if (listener.HasErrors) return;
            if (StringUtils.IsNullOrWhiteSpace(_currentChallengeId))
            {
                _currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
            }
            if (_currentChallengeId.Equals(listener.Challenge.ChallengeId))
            {
                var scriptData = listener.Challenge.ScriptData;
				if (scriptData != null && scriptData.GetNumber ("drawPoints") != null)
                {
					int points = (int)scriptData.GetNumber ("drawPoints");
					var message = "It's a draw game!";
                    
                    if (_onChallengeDetected != null)
                    {
                        var gameStateMessage = new GameStateMessage
                        {
                            Message = message,
							Points = points
                        };
                        _onChallengeDetected.OnChallengeFinishedEvent(GameStates.Draw,
                            gameStateMessage);
                    }
                }
            }
        };
        ChallengeLostMessage.Listener = (listener) =>
        {
            if (listener.HasErrors) return;

            if (StringUtils.IsNullOrWhiteSpace(_currentChallengeId))
            {
                _currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
            }
            if (_currentChallengeId.Equals(listener.Challenge.ChallengeId))
            {
                var scriptData = listener.Challenge.ScriptData;
                if (scriptData != null)
                {
                    var points = 0;
                    var point = scriptData.GetInt("losePoints");
                    if (point != null)
                    {
                        points = (int) point;
                    }
                    
					var message = "You Lost. Better luck next time!";
                    
                    if (_onChallengeDetected != null)
                    {
                        var gameStateMessage = new GameStateMessage
                        {
                            Message = message,
                            Points = points
                        };
                        _onChallengeDetected.OnChallengeFinishedEvent(GameStates.Lost,
                            gameStateMessage);
                    }
                }
            }
        };

        AchievementEarnedMessage.Listener = (listener) =>
        {
            if (listener.HasErrors) return;
            var achievementName = listener.AchievementName;
            if (_onAchievementDetected != null)
            {
                _onAchievementDetected.OnAchievmentEarned(achievementName);
            }
        };
    }

    protected override void GameDestroy()
    {
        base.GameDestroy();
    }

    public void RefreshChallengesList()
    {
        if (_refreshCoroutine != null)
        {
            StopCoroutine(_refreshCoroutine);
        }

        _challenges.Clear();

		var requestData = new GSRequestData();
        requestData.AddString("Language", LanguageUtils.RealLanguage(LocaliseText.Language));

        var eligibilityCriteria = new GSRequestData();

        GSData _d = new GSData(new Dictionary<string, object>(){
                    {"Language", LanguageUtils.RealLanguage(LocaliseText.Language)}
                    });
        eligibilityCriteria.AddObject("segments", _d);

        new FindChallengeRequest()
            .SetAccessType("PUBLIC")
    		.SetShortCode(new List<string> {Constants.ChallengeShortCode})
            .SetScriptData(requestData)
            .SetEligibility(eligibilityCriteria)
            .Send((secondResponse) =>
            {
                var publicChallenges = secondResponse.ChallengeInstances;
                if (publicChallenges == null || !publicChallenges.Any())
                {
                    var challengesData = secondResponse.ScriptData;
                    if (challengesData == null) return;
                    var dataList = secondResponse.ScriptData.GetGSDataList("challenges");
                    if (dataList != null)
                    {
                        if (dataList.Count > 0)
                        {
                            var enumerator = dataList.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                var current = enumerator.Current;
    							
                                if (current != null)
                                {
                                    string username = null;
    								string userId = null;
                                    var challengeId = current.GetString("challengeId");
                                    var scriptData = current.GetGSData("scriptData");
                                    
                                    if (scriptData != null)
                                    {
                                        
                                    }
                                    var challenger = current.GetGSData("challenger");
                                    if (challenger != null)
                                    {
                                        username = challenger.GetString("name");
    									userId = challenger.GetString ("id");

    									if ( userId == PreferencesFactory.GetString (Constants.ProfileUserId) ) {
    										continue;
    									}
                                    }

                                    var ch = new Challenge
                                    {
                                        ChallengeId = challengeId,
                                        UserName = username,
    									UserId = userId,
    									AvatarUploadId = "",
    									ExternalIds = null
                                    };
                                    _challenges.Add(ch);
                                }
                            }
                            enumerator.Dispose();
                        }
                    }
                }

                List<string> _ignoreChallangeIds = new List<string>();

                var ignoreList = secondResponse.ScriptData.GetGSDataList("ignoreChallanges");
                if (ignoreList != null)
                {
                    var challengesIgnoreEnum = ignoreList.GetEnumerator();
                    while (challengesIgnoreEnum.MoveNext())
                    {
                        var challenge = challengesIgnoreEnum.Current;

                        _ignoreChallangeIds.Add(challenge.GetString("challengeId"));
                    }
                }

                if (publicChallenges != null)
                {
                    var challengesEnum = publicChallenges.GetEnumerator();
                    while (challengesEnum.MoveNext())
                    {
                        var challenge = challengesEnum.Current;

                        if ( _ignoreChallangeIds.Contains(challenge.ChallengeId) ) {
                            continue;
                        }
    					
                        var scriptData = challenge.ScriptData;
                        var showInList = true;
                        if (scriptData != null)
                        {
                            var k = scriptData.GetBoolean("showInList");
                            if (k != null)
                            {
                                showInList = (bool) k;
                            }
                        }
                        if (showInList)
                        {
                            var username = challenge.Challenger.Name;
    						var userId = challenge.Challenger.Id;
    						
    						if ( userId == PreferencesFactory.GetString (Constants.ProfileUserId) ) {
    							continue;
    						}

                            var ch = new Challenge
                            {
                                ChallengeId = challenge.ChallengeId,
                                UserName = username,
    							UserId = userId,
    							AvatarUploadId = challenge.ScriptData.GetString ("avatarUploadId"),
    							ExternalIds = challenge.Challenger.ExternalIds
                            };
                            _challenges.Add(ch);
                        }
                    }
                    challengesEnum.Dispose();

                    if (_onChallengeDetected != null)
                    {
                        _onChallengeDetected.OnChallengesListeFetched(_challenges);
                    }
                }

                if (ShouldGetChallengesList)
                {
                    _refreshCoroutine = CoRoutines.DelayedCallback(_challengeRefreshRate, RefreshChallengesList);
                    StartCoroutine(_refreshCoroutine);
                }
            });
    }

    public void SendJoinChallengeRequest(string challengeInstanceId)
    {
        new JoinChallengeRequest()
            .SetChallengeInstanceId(challengeInstanceId)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    if (response.Joined != null && (bool) !response.Joined)
                    {
                        if (_onChallengeDetected != null)
                        {
                            _onChallengeDetected.OnErrorReceived(LocaliseText.Get("Challenge.NotAvailable"));
                        }
                        RefreshChallengesList();
                    }
                }
                else
                {
                    if (_onChallengeDetected != null)
                    {
                        _onChallengeDetected.OnErrorReceived(LocaliseText.Get("Challenge.NotAvailable"));
                    }
                    RefreshChallengesList();
                }
            });
    }

    public void CreateChallengeWithUser(string userId, bool isRematch)
    {
        _isRematch = isRematch;
        var usersToChallenge = new List<string> {userId};

        var data = new GSRequestData();

        new CreateChallengeRequest()
			.SetChallengeShortCode(Constants.ChallengeShortCode)
            .SetMaxPlayers(2)
            .SetScriptData(data)
            .SetExpiryTime(DateTime.UtcNow.AddMinutes(10))
            .SetEndTime(DateTime.UtcNow.AddHours(24))
            .SetUsersToChallenge(usersToChallenge)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    
                }
            });
    }

    public void SendDragPosition(List <DrawQueueItem> queue, Vector3 center, Action<LogChallengeEventResponse> response) {
		if (_currentChallengeId == null)
		{
			_currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
		}

        List<GSData> _pos = new List<GSData>();
        Dictionary<string, object> _d = new Dictionary<string, object>();

        foreach ( DrawQueueItem _p in queue ) {
            _d.Add("item", _p.ToJson());

            GSData _data = new GSData(_d);

            _pos.Add(_data);
            _d.Clear();
        }

		GSRequestData data = new GSRequestData ();

        data.AddObjectList("positions", _pos);
		data.AddNumber("center_x", center.x);
		data.AddNumber("center_y", center.y);
		data.AddNumber("center_z", center.z);

		new LogChallengeEventRequest()
			.SetChallengeInstanceId(_currentChallengeId)
			.SetEventKey("WordDrag")
			.SetEventAttribute("data", data)
			.Send(response);
	}

	public void SendFoundWord(string word, float time, Action<LogChallengeEventResponse> response) {
		if (_currentChallengeId == null)
		{
			_currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
		}

		GSRequestData data = new GSRequestData ();
		data.AddString ("word", word);
		data.AddNumber ("time", time);

		new LogChallengeEventRequest()
			.SetChallengeInstanceId(_currentChallengeId)
			.SetEventKey("WordFound")
			.SetEventAttribute("data", data)
			.Send(response);
	}

    public void SendPlayerTurnEvent(Action<LogChallengeEventResponse> response)
    {
        if (_currentChallengeId == null)
        {
            _currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
        }

        new LogChallengeEventRequest()
            .SetChallengeInstanceId(_currentChallengeId)
            .SetEventKey("takeTurn")
            .Send(response);
    }

    public void CreateMatch()
    {
        // NOTE - Create challenge with Event request NOT with CreateChallengeRequest!!!
        new LogEventRequest()
            .SetEventKey("CreateChallenge")
            .SetEventAttribute("language", LanguageUtils.RealLanguage(LocaliseText.Language))
            .Send((response) =>
            {
                _challenges.Clear();

                if (_onChallengeDetected != null)
                {
                    _onChallengeDetected.OnChallengesListeFetched(_challenges);
                }
            });
    }

	public void CancelChallenge(string challengeId = null)
    {
        if (_currentChallengeId == null) {
            _currentChallengeId = GameSparksManager.Instance.GetCurrentChallengeId();
        }

		if ( challengeId == null ) {
			challengeId = _currentChallengeId;
		}

		if (challengeId != null)
        {
            new LogEventRequest()
                .SetEventKey("leaveChallenge")
				.SetEventAttribute("challengeId", challengeId)
                .Send((response) =>
                {
                    if (!response.HasErrors)
                    {
                        //var data = response.ScriptData;
                    }
                });
        }
    }

    public void ChallengeInfo(string challengeId, Action <GSData> action = null) {
        new GetChallengeRequest()
            .SetChallengeInstanceId(challengeId)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    var challenge = response.Challenge;
                    GSData scriptData = challenge.ScriptData;

                    if ( action != null ) {
                        action(scriptData);
                    }
                }
            });
    }

	public void OnAdFinishedCallback(bool hasError, string zoneId)
	{
        // NOT USED
	}

	public void OnAdStartedCallback()
	{
		// NOT USED IN THIS CLASS
	}
}