using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;
using GameFramework.Debugging;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Localisation;
using GameFramework.GameObjects.Components;
using GameFramework.GameObjects;
using GameFramework.GameStructure;
using GameFramework.Facebook.Messages;
using GameFramework.Messaging;
using GameFramework.Display.Other;
using System.Collections;

public class LeaderboardController : Singleton <LeaderboardController>
{
    public class LeaderBoardUser
    {
        public int Rank;
        public string Score;
        public string Username;
		public string FBUserId;
    }

    public RectTransform TotalContentPanel;
    public RectTransform MonthlyContentPanel;
	public RectTransform RewardsContentPanel;
    public GameObject PlayerButtonPrefab;
	public GameObject PlayerButtonPrefabGold;
	public GameObject PlayerButtonPrefabSilver;
	public GameObject PlayerButtonPrefabBronze;
    public Button MontlyButton;
    public Button TotalButton;
	public Button RewardsButton;
	public Color DeselectButtonColor = Color.gray;

    private readonly List<LeaderBoardUser> _totalUsers = new List<LeaderBoardUser>();
    private readonly List<LeaderBoardUser> _monthlyUsers = new List<LeaderBoardUser>();

	private Dictionary <string, GameObject> users = new Dictionary<string, GameObject>();

	[SerializeField]
	private Spinner spinner;

    bool _showInfo = false;

    private void Start()
    {
		if (!GameSparksManager.IsTokenAvailable ()) {
			GameSparksManager.Instance.AnonymousLogin ();
		}

		GameManager.SafeAddListener<FacebookProfilePictureMessage> (FacebookProfilePictureHandler);

        if (!_showInfo)
        {
            StartCoroutine(CoRoutines.DelayedCallback(0.35f, GetMonthlyScores));

            MontlyButton.GetComponent<ButtonHover>().selected = true;
            TotalButton.GetComponent<ButtonHover>().selected = false;
            RewardsButton.GetComponent<ButtonHover>().selected = false;
        }

		if (!Debug.isDebugBuild) {
			Flurry.Flurry.Instance.LogEvent ("Leaderboard");
			Fabric.Answers.Answers.LogContentView ("Leaderboard", "Dialog");
		}
    }

	void OnDestroy() {
		GameManager.SafeRemoveListener<FacebookProfilePictureMessage> (FacebookProfilePictureHandler);
	}

    void Close() {
        GameObject CloseButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "Close", true);
        Button _button = CloseButton.GetComponent<Button>();

        _button.onClick.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

	public void Info() {
        _showInfo = true;

		spinner.Hide ();

		MontlyButton.GetComponent <ButtonHover> ().selected = false;
		TotalButton.GetComponent <ButtonHover>().selected = false;
		RewardsButton.GetComponent <ButtonHover>().selected = true;

		MonthlyContentPanel.parent.gameObject.SetActive(false);
		TotalContentPanel.parent.gameObject.SetActive(false);
		RewardsContentPanel.parent.gameObject.SetActive (true);
	}

	bool FacebookProfilePictureHandler(BaseMessage message) {
		FacebookProfilePictureMessage m = message as FacebookProfilePictureMessage;

		string key = string.Format ("{0}.{1}", "monthly", m.UserId);

		if ( users.ContainsKey (key) ) {
			GameObject item = users [key];

			GameObject avatar = GameObjectHelper.GetChildNamedGameObject (item, "AvatarImage", true);
			avatar.GetComponent <RawImage> ().texture = m.Texture;
		}

		key = string.Format ("{0}.{1}", "alltime", m.UserId);

		if (users.ContainsKey (key)) {
			GameObject item = users [key];

			GameObject avatar = GameObjectHelper.GetChildNamedGameObject (item, "AvatarImage", true);
			avatar.GetComponent <RawImage> ().texture = m.Texture;
		}

		return true;
	}

    public void OnMonthlyButtonClickListener()
    {
		MontlyButton.GetComponent <ButtonHover> ().selected = true;
		TotalButton.GetComponent <ButtonHover>().selected = false;
		RewardsButton.GetComponent <ButtonHover>().selected = false;

        MonthlyContentPanel.parent.gameObject.SetActive(true);
        TotalContentPanel.parent.gameObject.SetActive(false);
		RewardsContentPanel.parent.gameObject.SetActive (false);

		if (_monthlyUsers.Count == 0) {
			GetMonthlyScores ();
		}
    }

    public void OnTotalButtonClickListener()
    {
		MontlyButton.GetComponent <ButtonHover> ().selected = false;
		TotalButton.GetComponent <ButtonHover>().selected = true;
		RewardsButton.GetComponent <ButtonHover>().selected = false;

        MonthlyContentPanel.parent.gameObject.SetActive(false);
        TotalContentPanel.parent.gameObject.SetActive(true);
		RewardsContentPanel.parent.gameObject.SetActive (false);

		if (_totalUsers.Count == 0) {
			GetTotalScores ();
		}
    }

    private void GetMonthlyScores()
    {
		spinner.Show ();

        var theDate = System.DateTime.Now.ToString("yyyyMM");

        new GameSparks.Api.Requests.LeaderboardDataRequest()
			.SetLeaderboardShortCode("Monthly.period."+theDate)
            .SetEntryCount(100)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    foreach (var entry in response.Data)
                    {
                        var rank = 0;
                        var rankObj = entry.Rank;
                        if (rankObj != null)
                        {
                            rank = (int) rankObj;
                        }

						string FBUserId = null;

						if ( entry.ExternalIds != null ) {
							if ( entry.ExternalIds.ContainsKey ("FB") ) {
								FBUserId = entry.ExternalIds.GetString ("FB");
							}
						}

                        var playerName = entry.UserName;
                        var score = "";
						var scoreObj = entry.JSONData["score"];
                        if (scoreObj != null)
                        {
                            score = scoreObj.ToString();
                        }
                        var leaderBoardUser = new LeaderBoardUser
                        {
                            Rank = rank,
                            Username = playerName,
							Score = score,
							FBUserId = FBUserId
                        };
                        _monthlyUsers.Add(leaderBoardUser);
                    }
                    PopulateMonthlyLeaderboard();
                }
                else
                {
					MyDebug.Log("Error Retrieving Leaderboard Data..." + response.Errors.JSON);
                }

				spinner.Hide ();
            });
    }

    private void GetTotalScores()
    {
		spinner.Show ();

        new GameSparks.Api.Requests.LeaderboardDataRequest().SetLeaderboardShortCode("allTime")
            .SetEntryCount(100)
            .Send((response) =>
            {
                if (!response.HasErrors)
                {
                    foreach (var entry in response.Data)
                    {
                        var rank = 0;
                        var rankObj = entry.Rank;
                        if (rankObj != null)
                        {
                            rank = (int) rankObj;
                        }

						string FBUserId = null;

						if ( entry.ExternalIds != null ) {
							if ( entry.ExternalIds.ContainsKey ("FB") ) {
								FBUserId = entry.ExternalIds.GetString ("FB");
							}
						}

                        var playerName = entry.UserName;
                        var score = "";
						var scoreObj = entry.JSONData.ContainsKey ("SUM-score") ? entry.JSONData["SUM-score"] : null;
                        if (scoreObj != null)
                        {
                            score = scoreObj.ToString();
                        }
                        var leaderBoardUser = new LeaderBoardUser
                        {
                            Rank = rank,
                            Username = playerName,
                            Score = score,
							FBUserId = FBUserId
                        };
                        _totalUsers.Add(leaderBoardUser);
                    }
                    PopulateTotalLeaderboard();
                }
                else
                {
					MyDebug.Log("Error Retrieving Leaderboard Data...");
                }

				spinner.Hide ();
            });
    }

    private void PopulateMonthlyLeaderboard()
    {
        if (MonthlyContentPanel == null) return;
        foreach (Transform child in MonthlyContentPanel)
        {
            Destroy(child.gameObject);
        }

		int index = 0;
        foreach (var user in _monthlyUsers)
        {
			GameObject playerPrefab;

			switch ( index ) {
			case 0:
				playerPrefab = Instantiate (PlayerButtonPrefabGold);
				break;
			case 1:
				playerPrefab = Instantiate (PlayerButtonPrefabSilver);
				break;
			case 2:
				playerPrefab = Instantiate (PlayerButtonPrefabBronze);
				break;
			default:
				playerPrefab = Instantiate (PlayerButtonPrefab);
				break;
			}

            playerPrefab.transform.SetParent(MonthlyContentPanel, false);

            var usernameText = playerPrefab.transform.Find("Username").GetComponent<Text>();
            usernameText.text = user.Username;

            var scoreText = playerPrefab.transform.Find("Score").GetComponent<Text>();
            scoreText.text = user.Score;

			var numberText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(playerPrefab, "Number", true);
			numberText.text = (index + 1).ToString();

			index++;
        }
    }

    private void PopulateTotalLeaderboard()
    {
        if (TotalContentPanel == null) return;
        foreach (Transform child in TotalContentPanel)
        {
            Destroy(child.gameObject);
        }

		int index = 0;

        foreach (var user in _totalUsers)
        {
			GameObject playerPrefab;

			switch ( index ) {
			case 0:
				playerPrefab = Instantiate (PlayerButtonPrefabGold);
				break;
			case 1:
				playerPrefab = Instantiate (PlayerButtonPrefabSilver);
				break;
			case 2:
				playerPrefab = Instantiate (PlayerButtonPrefabBronze);
				break;
			default:
				playerPrefab = Instantiate (PlayerButtonPrefab);
				break;
			}

            playerPrefab.transform.SetParent(TotalContentPanel, false);

            var usernameText = playerPrefab.transform.Find("Username").GetComponent<Text>();
            usernameText.text = user.Username;

            var scoreText = playerPrefab.transform.Find("Score").GetComponent<Text>();
            scoreText.text = user.Score;

            var numberText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(playerPrefab, "Number", true);
			numberText.text = (index + 1).ToString();

			index++;
        }
    }
}