using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components;
using System;
using GameFramework.GameObjects;
using TouchScript.Gestures;
using UnityEngine.UI;
using System.Resources;
using GameFramework.GameStructure.Levels;
using GameFramework.Helper;
using GameFramework.GameStructure;
using GameFramework.GameStructure.Levels.ObjectModel;
using System.Text;
using System.Linq;
using DG.Tweening;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Display.Other;
using GameSparks.Core;
using GameFramework.Preferences;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;
using TouchScript;
using GameFramework.Facebook.Messages;
using GameFramework.Messaging;
using GameFramework.Localisation;
using GameFramework.Localisation.Messages;
using GameFramework.Debugging;
using GameFramework.UI.Other;
using PaperPlaneTools;
using Flurry;
using SA.IOSNative.Core;
using GameFramework.Facebook.Components;
using Facebook.Unity;
using System.Globalization;
using UnityEngine.Advertisements;
using UnityEngine.UI.Extensions;

public enum WordState
{
    NotFound = 0, Found, Already
}

public class GameController : Singleton<GameController>, ChallengeManager.IOnChallengeDetect,
ChallengeManager.IOnAchievementDetected, IGestureDelegate
{
    public AudioClip[] Sounds;

    private float time;
    private bool paused;
    public bool isPaused { get { return paused; } }
    private float _timeSinceLastCalled;

    private List<GameObject> _chars = new List<GameObject>(); // буквите, които са на таблото
    private List<GameObject> _matchChars = new List<GameObject>(); // буквите, които са избрани при drag
    private List<GameObject> _draggableObjects = new List<GameObject>(); // обектите, които съдържат Box collider
    private StringBuilder _matchWord = new StringBuilder(8);
    private List<string> _allWords = new List<string>(); // всички думи
    private List<string> _foundWords = new List<string>(); // думите които са намерени вече
    private Dictionary<string, GameObject> _wordsCointainers = new Dictionary<string, GameObject>();
    private List<GameObject> _wordsCointainersItems = new List<GameObject>();

    private GameObject _draggingObject; // текущата буква която се drag-ва
    private Vector3 _draggingObjectStartPosition;

    [SerializeField]
    private LineRenderer _lineRenderer; // линията, която свързва избраните букви
    private LineRenderer _currentLineRenderer;
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    private List<Vector3> _points = new List<Vector3>();

    private Dictionary<string, GameObject> _mapUsersAvatar = new Dictionary<string, GameObject>();

    [SerializeField]
    private GameObject _table;
    [SerializeField]
    private GameObject _board;
    private GameObject _boardContainer;
    [SerializeField]
    private GameObject _charPrefab;
    [SerializeField]
    private GameObject _wordContainerPrefab;
    [SerializeField]
    private GameObject _wordContainerCharPrefab;
    [SerializeField]
    private GameObject _shovel;
    [SerializeField]
    private GameObject _lady;
    [SerializeField]
    private GameObject _columnLayoutPrefab;
    [SerializeField]
    private GameObject _columnLayoutPrefabSingleWord;
    [SerializeField]
    private GameObject multiplayerContainer;
    private Text multiplayerTimerText;
    private bool _sendTurnTakenEvent;
    [SerializeField]
    private Text _currentLevelText;

    private GameObject _boostsContainer;
    private GameObject _turnIndicator;
    private Button _quitGameButton;

    private bool _dragBegan = false;
    private bool _charsFollowShovel = false;

    private bool _isMyTurn = true;
    private bool _userLeftMatch;
    private float _startTime;
    private float _turnTime;
    private GameObject _yourTurnObject;
    private string _rematchChallengeId;

    private float _nextBroadcastTime = 0;
    private bool _startBroadcast = false;
    private bool _startDrawFromQueue = false;
    private List<DrawQueueItem> _broadcastQueue = new List<DrawQueueItem>();
    private Queue<DrawQueueItem> _drawPositionsQueue = new Queue<DrawQueueItem>();
    private Queue<Action> _actionsQueue = new Queue<Action>(); // Queue of action to execute only when _broadcastQueue is empty

    private GameMode _gameMode;
    private JSONArray _gameWords;

    float debugAnimationTime = 0f;

    private GameObject _rematchObject;
    private LevelCompleted _levelCompleted;
    private IEnumerator _visualResetCoroutine;
    private IEnumerator _previewHideCoroutine;
    private GameObject _askFriendsDialog;
    private GameObject _rewardShareButton;
    private int _gamesPlayed;

    [SerializeField]
    private Color _defaultColor;
    [SerializeField]
    private Color _correctColor;
    [SerializeField]
    private Color _wrongColor;
    [SerializeField]
    private Color _alreadyColor;
    [SerializeField]
    private GameObject _previewContainer;
    [SerializeField]
    private GameObject _greetingObject;
    private int wrongTimes = 0;
    float _timeWithNoUserAction = 0;
    bool _countingUserAction = false;
    public int usedHintsCount { get; private set; }
    GameObject shuffleButton;
    private float _updateTableScaleTime;
    bool singleWordTable = false;
    int indexOfSearchingWord = -1;
    private HorizontalScrollSnap _scrollSnap;
    [SerializeField]
    Text _pageText;

    void Start()
    {
        this.Pause();

        GameManager.SafeAddListener<FacebookProfilePictureMessage>(FacebookProfilePictureHandler);
        GameManager.SafeAddListener<FacebookLoginMessage>(OnFacebookLoginMessage);
        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);
        GameManager.SafeAddListener<SettingsStateChanged>(SettingsStateHandler);
        GameManager.SafeAddListener<GameResetedMessage>(GameResetedHandler);

#if UNITY_EDITOR
        debugAnimationTime = 0f;
#endif

        _gameMode = GameSparksManager.Instance.GetGameMode();
        _gameWords = GameWords();
        _gamesPlayed = PreferencesFactory.GetInt("GamesPlayed");

        if (_gameMode == GameMode.Multi)
        {
            GameObject menuContainer = GameObjectHelper.GetChildNamedGameObject(gameObject, "MultiPlayerMenu", true);
            menuContainer.SetActive(true);

            GameObject boostsContainer = GameObjectHelper.GetChildNamedGameObject(gameObject, "Boosts", true);
            GameObject boostsWord = GameObjectHelper.GetChildNamedGameObject(boostsContainer, "Word", true);

            boostsWord.SetActive(false);

            GameObject _sceneScope = GameObject.Find("_SceneScope");
            _sceneScope.GetComponent<ChallengeManager>().enabled = true;
        }
        else
        {
            GameObject menuContainer = GameObjectHelper.GetChildNamedGameObject(gameObject, "SinglePlayerMenu", true);
            menuContainer.SetActive(true);

            shuffleButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "ShuffleButton", true);
            shuffleButton.SetActive(true);

            singleWordTable = ShouldSetAsSingleWordTable();
        }

        _boostsContainer = GameObjectHelper.GetChildNamedGameObject(gameObject, "Boosts", true);

        GameObject _q = GameObjectHelper.GetChildNamedGameObject(SettingsContainer.Instance.gameObject, "QuitGameButton", true);
        _quitGameButton = _q.GetComponent<Button>();

        _quitGameButton.onClick.RemoveAllListeners();

        _quitGameButton.onClick.AddListener(() =>
        {
            MainMenu();
        });

        _quitGameButton.gameObject.SetActive(true);

        GameObject _inviteButton = GameObjectHelper.GetChildNamedGameObject(SettingsContainer.Instance.gameObject, "InviteButton", true);
        _inviteButton.SetActive(false);

        _askFriendsDialog = GameObjectHelper.GetChildNamedGameObject(gameObject, "AskFriendsDialog", true);
        _rewardShareButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "RewardButton", true);

        if (_gameWords != null)
        {
            StartGameWithWords(_gameWords);
        }
        else
        {
            Loading.Instance.Show();

            LevelController.LoadLevelFromServer((JSONArray _words) =>
            {
                Loading.Instance.Hide();

                _gameWords = _words;

                StartGameWithWords(_gameWords);
            });
        }

        if (PreferencesFactory.GetInt(Constants.KeyShowTutorial, 0) == 1)
        {
            PreferencesFactory.SetInt(Constants.KeyShowTutorial, 2);

            DialogManager.Instance.Show("HowToPlay");
        }

        if (!Debug.isDebugBuild)
        {
            FlurryIOS.LogPageView();
            FlurryAndroid.OnPageView();

            Fabric.Answers.Answers.LogContentView("Game", "Screen");
        }
    }

    void StartGameWithWords(JSONArray _words)
    {
        if (singleWordTable)
        {
            _boardContainer = GameObject.Find("ContainerSingleWord").transform.GetChild(0).gameObject;
            _pageText.gameObject.SetActive(true);
        }
        else
        {
            _boardContainer = _board;
            _pageText.gameObject.SetActive(false);
        }

        if (_gameMode == GameMode.Multi)
        {
            var orderChar = _words[0].Obj.GetString("order");
            Char[] chars = null;

            if (orderChar != null)
            {
                chars = orderChar.ToCharArray();
            }

            PrepareHintTable(_words, chars);
            PrepareTable(_words);

            PrepareMultiplayer();
        }
        else
        {
            PrepareHintTable(_words);
            PrepareTable(_words);
        }

        StartGame();
    }

    protected override void GameDestroy()
    {
        CloseAllDialogs();

        if (_rematchChallengeId != null)
        {
            ChallengeManager.Instance.CancelChallenge(_rematchChallengeId);
        }

        PreferencesFactory.SetInt("GamesPlayed", _gamesPlayed);

        GameSparksManager.Instance.SetGameMode(GameMode.Single);
        GameSparksManager.Instance.SetCurrentChallengeId(null);
        GameSparksManager.Instance.Challenger = null;
        GameSparksManager.Instance.Challenged = null;
        GameSparksManager.Instance.Opponent = null;
        GameSparksManager.Instance.SetWords(null);

        ChallengeManager.Instance.SetOnChallengeDetected(null);

        GameManager.SafeRemoveListener<FacebookProfilePictureMessage>(FacebookProfilePictureHandler);
        GameManager.SafeRemoveListener<FacebookLoginMessage>(OnFacebookLoginMessage);
        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
        GameManager.SafeRemoveListener<SettingsStateChanged>(SettingsStateHandler);
        GameManager.SafeRemoveListener<GameResetedMessage>(GameResetedHandler);
    }

    protected override void OnApplicationPause(bool pauseStatus)
    {
        base.OnApplicationPause(pauseStatus);

        if (!pauseStatus && _gameMode == GameMode.Multi)
        {
            //ChallengeInfo();
        }
    }

    public JSONArray GameWords()
    {
        if (_gameMode == GameMode.Single)
        {
            Level level = GameManager.Instance.Levels.Selected;

            if (level.JsonData == null)
            {
                level.LoadData();
            }

            if (level.JsonData != null)
            {
                return level.JsonData.GetArray("words");
            }

            JSONArray _arr = LevelController.GetWordsFromServer();

            return _arr;
        }

        List<GSData> _w = GameSparksManager.Instance.GetWords();

        JSONArray _words = new JSONArray();

        foreach (GSData _d in _w)
        {
            JSONObject _o = JSONObject.Parse(_d.JSON);
            _words.Add(new JSONValue(_o));
        }

        return _words;
    }

    public void NextLevel()
    {
        this.Pause();
        time = 0;
        _updateTableScaleTime = 0;
        _timeSinceLastCalled = 0;
        wrongTimes = 0;
        usedHintsCount = 0;

        _chars.Clear();
        _draggableObjects.Clear();
        _allWords.Clear();
        _wordsCointainers.Clear();
        _wordsCointainersItems.Clear();
        _foundWords.Clear();

        foreach (Transform _child in _table.transform)
        {
            Destroy(_child.gameObject);
        }

        // Using DestroyImmediate because Next level scroll rect is wrong calculated
        List<Transform> _childrens = new List<Transform>();
        foreach (Transform _child in _boardContainer.transform)
        {
            _childrens.Add(_child);
        }

        foreach (Transform _child in _childrens)
        {
            DestroyImmediate(_child.gameObject);
        }

        Reset();
        ResetBoosts();
        VisualReset();

        if (singleWordTable && _scrollSnap != null)
        {
            _scrollSnap.UpdateLayout();
        }

        _gameWords = GameWords();

        if (_gameWords != null)
        {
            StartGameWithWords(_gameWords);
            StartCoroutine(CoRoutines.DelayedCallback(2.0f, ShowRateWindow));
        }
        else
        {
            Loading.Instance.Show();

            LevelController.LoadLevelFromServer((JSONArray _words) =>
            {
                Loading.Instance.Hide();

                _gameWords = _words;

                StartGameWithWords(_gameWords);
                StartCoroutine(CoRoutines.DelayedCallback(2.0f, ShowRateWindow));
            });
        }
    }

    void ShowRateWindow()
    {
        if (_gameMode == GameMode.Single && GameManager.Instance.Levels.Selected.Number == 5)
        {
            if (!Debug.isDebugBuild)
            {
                Flurry.Flurry.Instance.LogEvent("AppRate_GamePlay", new Dictionary<string, string>() { { "Level", GameManager.Instance.Levels.Selected.Number.ToString() } });
                Fabric.Answers.Answers.LogCustom("AppRate_GamePlay", new Dictionary<string, object>() { { "Level", GameManager.Instance.Levels.Selected.Number.ToString() } });
            }

            RateBox.Instance.ForceShow();
            return;
        }

        if (RateBox.Instance.CheckConditionsAreMet())
        {
            if (!Debug.isDebugBuild)
            {
                Flurry.Flurry.Instance.LogEvent("AppRate_GamePlay", new Dictionary<string, string>() { { "Level", GameManager.Instance.Levels.Selected.Number.ToString() } });
                Fabric.Answers.Answers.LogCustom("AppRate_GamePlay", new Dictionary<string, object>() { { "Level", GameManager.Instance.Levels.Selected.Number.ToString() } });
            }
        }

        RateBox.Instance.Show();
    }

    void StartGame()
    {
        _gamesPlayed += 1;

        this.Resume();

        if (_gameMode == GameMode.Single)
        {
            LevelManager.Instance.StartLevel();

            int levelNumber = PreferencesFactory.GetInt(Constants.KeyFoundWordsLevel, -1);

            _currentLevelText.text = LocaliseText.Format("Text.LevelNumber", GameManager.Instance.Levels.Selected.Number);

            if (levelNumber == GameManager.Instance.Levels.Selected.Number)
            {
                string _foundWordsString = PreferencesFactory.GetString(Constants.KeyFoundWords);

                if (_foundWordsString != null)
                {
                    char[] delimiterChars = { '|' };
                    string[] _words = _foundWordsString.Split(delimiterChars);

                    foreach (string _w in _words)
                    {
                        SetWordAsFound(_w);
                    }
                }

                if (singleWordTable)
                {
                    ScrollToFirstUnfoundWord();
                }
            }

            if (!Debug.isDebugBuild)
            {
                Level level = GameManager.Instance.Levels.Selected;
                levelNumber = level.Number;

                if (levelNumber >= 100 && levelNumber < 1000)
                {
                    levelNumber = (int)Math.Round(levelNumber / 100f) * 100; // 100 | 200 | 300
                }

                if (levelNumber >= 1000 && levelNumber < 10000)
                {
                    levelNumber = (int)Math.Round(levelNumber / 1000f) * 1000; // 1000 | 2000 | 3000
                }

                if (levelNumber >= 10000 && levelNumber < 100000)
                {
                    levelNumber = (int)Math.Round(levelNumber / 10000f) * 10000; // 10000 | 20000 | 30000
                }

                Flurry.Flurry.Instance.LogEvent(string.Format("Level_{0}", levelNumber)); // flurry have event names limit
                Fabric.Answers.Answers.LogLevelStart(string.Format("Level_{0}", level.Number));
            }
        }
        else
        {
            if (!Debug.isDebugBuild)
            {
                Flurry.Flurry.Instance.LogEvent("Multiplayer");
                Fabric.Answers.Answers.LogLevelStart("Multiplayer");
            }
        }

        if (_askFriendsDialog != null && AskFriendsHelper.WillReward())
        {
            AskFriendsHelper.SetRewardText(_askFriendsDialog);
            _askFriendsDialog.SetActive(true);
        }

        if (_rewardShareButton != null && RewardsShareHelper.WillReward())
        {
            ShowRewardShareButton();
        }

        // change background sound
        int random = UnityEngine.Random.Range(0, Sounds.Length);
        GameManager.Instance.BackGroundAudioSource.clip = Sounds[random];

        if (((CustomGameManager)GameManager.Instance).SoundEnabled())
        {
            GameManager.Instance.BackGroundAudioSource.Play();
        }

        if (!singleWordTable)
        {
            StartCoroutine(FixSizes());
        }
    }

    IEnumerator FixSizes()
    {
        yield return new WaitForEndOfFrame();

        FixSizesProcess(_boardContainer);
    }

    void FixSizesProcess(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        RectTransform parentRect = go.transform.parent.GetComponent<RectTransform>();

        Vector2 offset = Vector2.zero;

        SizeOffset sizeOffset = parentRect.gameObject.GetComponent<SizeOffset>();

        if (sizeOffset != null)
        {
            offset = sizeOffset.sizeOffset;
        }

        float scale = 1.0f;

        if (rect.sizeDelta.y > parentRect.sizeDelta.y - offset.y)
        {
            scale = (parentRect.sizeDelta.y - offset.y) / rect.sizeDelta.y;
        }

        if (rect.sizeDelta.x > parentRect.sizeDelta.x - offset.x)
        {
            float scaleY = (parentRect.sizeDelta.x - offset.x) / rect.sizeDelta.x;

            if (scaleY < scale)
            {
                scale = scaleY;
            }
        }

        go.transform.localScale = new Vector3(scale, scale, 1);
    }

    void MoveCharsToTheirPlace()
    {
        int index = 0;
        int angle = 360 / _chars.Count;

        _charsFollowShovel = false;

        foreach (GameObject _char in _chars)
        {
            Vector3 pos = Vector3Utils.RandomCircle(_table.transform.position, 1.2f, angle, index);

            Animator anim = GameObjectHelper.GetChildComponentOnNamedGameObject<Animator>(_char, "Smoke", true);

            float random = UnityEngine.Random.Range(0.15f, 0.35f);

            _char.transform.DOMove(pos, random + debugAnimationTime).SetEase(Ease.Linear);

            StartCoroutine(CoRoutines.DelayedCallback(random - 0.8f, () =>
            {
                anim.SetTrigger("Smoke");

                EnableDragging(true);
            }));

            _char.transform.DORotate(Vector3.zero, 0.25f + debugAnimationTime).SetEase(Ease.Linear);

            index++;
        }

        StartCoroutine(CoRoutines.DelayedCallback(0.25f, () =>
        {
            ButtonUtils.PlayGameBombSound();
        }));
    }

    void EnableDragging(bool enabling)
    {
        foreach (GameObject _drag in _draggableObjects)
        {
            _drag.SetActive(enabling);
        }
    }

    void PrepareHintTable(JSONArray words, char[] characters = null)
    {
        JSONObject word = words[0].Obj;

        if (_gameMode == GameMode.Single || characters == null)
        {
            string _w = word.GetString("word");
            characters = _w.ToCharArray();

            ArrayUtils.Shuffle(characters);
        }

        foreach (char _char in characters)
        {
            GameObject _c = Instantiate(_charPrefab, _table.transform);
            _c.transform.position = _shovel.transform.position;

            GameObject _drag = GameObjectHelper.GetChildNamedGameObject(_c, "Draggable", true);
            ObserveGestures(_drag);

            GameObject _text = GameObjectHelper.GetChildNamedGameObject(_c, "Text", true);
            _text.GetComponent<Text>().text = _char.ToString().ToUpper();

            _chars.Add(_c);
            _draggableObjects.Add(_drag);
        }

        _charsFollowShovel = true;

        Animator anim = _shovel.GetComponent<Animator>();
        anim.SetTrigger("Drop");

        StartCoroutine(CoRoutines.DelayedCallback(0.2f, MoveCharsToTheirPlace));
    }

    void PrepareTable(JSONArray words)
    {
        string[] _words = new string[words.Length];

        int i = 0;
        bool sort = false;
        foreach (JSONValue _w in words)
        {
            if (_w.Obj.ContainsKey("pos"))
            {
                int pos = (int)_w.Obj.GetNumber("pos");

                _words[pos - 1] = _w.Obj.GetString("word");
            }
            else
            {
                sort = true;
                _words[i] = _w.Obj.GetString("word");
            }

            i++;
        }

        if (sort)
        {
            Array.Sort(_words, (x, y) => x.Length.CompareTo(y.Length));
        }

        GameObject _columnLayout = _boardContainer;
        GameObject _prefab = singleWordTable ? _columnLayoutPrefabSingleWord : _columnLayoutPrefab;

        float columns = 3.0f;

        if (_words.Length <= 6)
        {
            columns = 2.0f;
        }

        int rows = (int)Math.Ceiling(_words.Length / columns);
        int currentRow = 1;

        if (singleWordTable)
        {
            rows = 1;
        }

        indexOfSearchingWord = 0;

        foreach (string _word in _words)
        {
            if (currentRow == rows + 1 || singleWordTable)
            {
                currentRow = 1; // reset
            }

            if (currentRow == 1)
            {
                _columnLayout = Instantiate(_prefab, _boardContainer.transform);

                if (singleWordTable)
                {
                    RectTransform rect = _columnLayout.transform as RectTransform;

                    rect.pivot = new Vector2(0.5f, 0.5f);
                }
            }

            GameObject wordContainer = Instantiate(_wordContainerPrefab, _columnLayout.transform);

            if (singleWordTable)
            {
                RectTransform rect = wordContainer.transform as RectTransform;

                rect.pivot = new Vector2(0.5f, 0.5f);
            }

            _wordsCointainers.Add(_word, wordContainer);
            _wordsCointainersItems.Add(wordContainer);

            char[] characters = _word.ToCharArray();

            int index = 0;
            foreach (char _char in characters)
            {
                GameObject _charContainer = Instantiate(_wordContainerCharPrefab, wordContainer.transform);
                CharTable _charTable = _charContainer.GetComponent<CharTable>();
                _charTable.position = index;

                Text _text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(_charContainer, "Text", true);
                _text.text = "";

                index++;
            }

            if (singleWordTable)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_columnLayout.transform as RectTransform);
                FixSizesProcess(wordContainer);
            }

            _allWords.Add(_word);

            currentRow++;
        }

        if (singleWordTable)
        {
            PrepareSingleWordTable();
        }
        else
        {
            PrepareMultiWordTable();
        }
    }

    void PrepareMultiplayer()
    {
        ContentSizeFitter f = GameObject.Find("Boosts").GetComponent<ContentSizeFitter>();
        f.verticalFit = ContentSizeFitter.FitMode.MinSize;

        _yourTurnObject = GameObjectHelper.GetChildNamedGameObject(gameObject, "YourTurn", true);

        ChallengeManager.Instance.SetOnChallengeDetected(this);

        _isMyTurn = GameSparksManager.Instance.GetIsMyTurn();
        _turnTime = Constants.ChallengeTurnDuration;

        multiplayerContainer.SetActive(true);

        multiplayerTimerText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(multiplayerContainer, "Timer", true);

        _turnIndicator = GameObjectHelper.GetChildNamedGameObject(multiplayerContainer, "TurnIndicator", true);

        // challenger
        Text nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(multiplayerContainer, "Name", true);
        nameText.text = GameSparksManager.Instance.Challenger != null ? GameSparksManager.Instance.Challenger.UserName : "";

        // chalenged
        Text opponentNameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(multiplayerContainer, "OpponentName", true);
        opponentNameText.text = GameSparksManager.Instance.Challenged != null ? GameSparksManager.Instance.Challenged.UserName : "";

        SetUserTurnIndicator();

        //

        if (GeneralUtils.IsRequiredAspectRatio(4.0f / 3.0f))
        {
            GameObject _bottomMenu = GameObjectHelper.GetChildNamedGameObject(gameObject, "BottomMenu", true);

            VerticalLayoutGroup _vertical = _bottomMenu.GetComponent<VerticalLayoutGroup>();
            DestroyImmediate(_vertical);

            HorizontalLayoutGroup _horizontal = _bottomMenu.AddComponent<HorizontalLayoutGroup>();
            _horizontal.childAlignment = TextAnchor.MiddleCenter;
            _horizontal.childControlHeight = false;
            _horizontal.childControlWidth = false;
            _horizontal.spacing = 50;
            _horizontal.padding.left = 50;
            _horizontal.padding.right = 50;

            FitImage _fitImage = _bottomMenu.GetComponent<FitImage>();
            _fitImage.enabled = true;

            ContentSizeFitter _sizeFitter = _bottomMenu.GetComponent<ContentSizeFitter>();
            _sizeFitter.enabled = true;
        }
        else
        {
            GameObject _bottomMenu = GameObjectHelper.GetChildNamedGameObject(gameObject, "BottomMenu", true);

            VerticalLayoutGroup _vertical = _bottomMenu.GetComponent<VerticalLayoutGroup>();
            _vertical.spacing = 50;

            FitImage _fitImage = _bottomMenu.GetComponent<FitImage>();
            _fitImage.stretchDirection = FitImage.Stretch.Vertical;
            _fitImage.enabled = true;

            ContentSizeFitter _sizeFitter = _bottomMenu.GetComponent<ContentSizeFitter>();
            _sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            _sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _sizeFitter.enabled = true;
        }
    }

    private void SetUserTurnIndicator()
    {
        if (_isMyTurn)
        {
            GameObjectHelper.SafeSetActive(_yourTurnObject, true);
            ButtonUtils.PlayYourTurnSound();

            if (GameSparksManager.Instance.Challenged.UserId == PreferencesFactory.GetString(Constants.ProfileUserId))
            {
                MoveTurnIndicatorForward();
            }
            else
            {
                MoveTurnIndicatorBackward();
            }
        }
        else
        {
            if (GameSparksManager.Instance.Challenged.UserId == PreferencesFactory.GetString(Constants.ProfileUserId))
            {
                MoveTurnIndicatorBackward();
            }
            else
            {
                MoveTurnIndicatorForward();
            }
        }
    }

    void ObserveGestures(GameObject _draggable)
    {
        PressGesture press = _draggable.GetComponent<PressGesture>();

        if (press != null)
        {
            press.Delegate = this;
            press.Pressed += PressedHandler;
        }

        ReleaseGesture release = _draggable.GetComponent<ReleaseGesture>();

        if (release != null)
        {
            release.Delegate = this;
            release.Released += ReleasedHandler;
        }

        TransformGesture transf = _draggable.GetComponent<TransformGesture>();

        if (transf != null)
        {
            transf.Delegate = this;
            transf.Transformed += TransformHandler;
        }
    }

    void Update()
    {
        if (!this.paused)
        {
            time += Time.deltaTime;

            if (_countingUserAction)
            {
                _timeWithNoUserAction += Time.deltaTime;

                if ((_gameMode == GameMode.Single && _timeWithNoUserAction > 15f)
                    || (_gameMode == GameMode.Multi && _isMyTurn == true && _timeWithNoUserAction > 15f))
                {
                    _timeWithNoUserAction = 0f;
                    AnimateAskFriends();
                }
            }

            if (_updateTableScaleTime < 1f)
            {
                _updateTableScaleTime += Time.deltaTime;

                if (!singleWordTable)
                {
                    FixSizesProcess(_boardContainer);
                }
            }
        }

        // in multiplayer there is no pausing 
        _timeSinceLastCalled += Time.deltaTime;
        _turnTime -= Time.deltaTime;

        if (_timeSinceLastCalled > 1f)
        {
            if (_gameMode == GameMode.Multi)
            {
                if (_turnTime < 0f)
                {
                    _turnTime = 0f;
                }

                TimeSpan timeSpan = TimeSpan.FromSeconds(_turnTime);

                multiplayerTimerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            }

            _timeSinceLastCalled = 0f;
        }

        // broadcast drag data
        if (_startBroadcast && _gameMode == GameMode.Multi && Time.time > _nextBroadcastTime)
        {
            if (_broadcastQueue.Count > 0)
            {
                LogDragging(_broadcastQueue);
                _nextBroadcastTime = Time.time + 0.15f;
                _broadcastQueue.Clear();
            }
            else
            {
                _startBroadcast = false;
            }
        }

        // execute Action after broadcast
        if (_gameMode == GameMode.Multi && _actionsQueue.Count > 0)
        {
            // adding some delay so tasks can execute proper because most of them are Requests
            if (_broadcastQueue.Count == 0 && Time.time > _nextBroadcastTime)
            {
                Action action = _actionsQueue.Dequeue();

                if (action != null)
                {
                    action.Invoke();
                }

                _nextBroadcastTime = Time.time + 0.15f;
            }
        }

        // draw opponent drag positions
        if (_gameMode == GameMode.Multi && _startDrawFromQueue)
        {
            if (_drawPositionsQueue.Count > 0)
            {
                DrawQueueItem item = _drawPositionsQueue.Dequeue();

                ProcessQueueItem(item);
            }
            else
            {
                _startDrawFromQueue = false;
            }
        }

        if (_charsFollowShovel)
        {
            foreach (GameObject _char in _chars)
            {
                _char.transform.position = _shovel.transform.position;
                _char.transform.rotation = _shovel.transform.rotation;
            }
        }
    }

    public void Pause()
    {
        this.paused = true;
    }

    public void Resume()
    {
        this.paused = false;
    }

    // Array with indexes of characters which are not found yet
    int[] GetUnfoundCharacters(char[] characters, GameObject _wordContainer)
    {
        List<int> _charList = new List<int>();

        int index = 0;

        foreach (char _char in characters)
        {
            Transform _child = _wordContainer.transform.GetChild(index);
            CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

            if (!_charTable.found)
            {
                _charList.Add(index);
            }

            index++;
        }

        return _charList.ToArray();
    }

    string GetUnfoundWord()
    {
        List<string> _all = ListUtils.Shuffle(_allWords);

        foreach (string _w in _all)
        {
            if (_foundWords.Exists(e => e.Equals(_w)))
            {
                continue;
            }

            return _w;
        }

        return null;
    }

    #region Hints

    void ResetBoosts()
    {
        Button firstLetterHint = GameObjectHelper.GetChildComponentOnNamedGameObject<Button>(_boostsContainer, "First", true);
        Button lastLetterHint = GameObjectHelper.GetChildComponentOnNamedGameObject<Button>(_boostsContainer, "Last", true);
        Button randomLetterHint = GameObjectHelper.GetChildComponentOnNamedGameObject<Button>(_boostsContainer, "Random", true);

        firstLetterHint.interactable = true;
        lastLetterHint.interactable = true;
        randomLetterHint.interactable = true;
    }

    void DisableHintIfNeedle()
    {
        bool enableFirstLetterHint = false;
        bool enableLastLetterHint = false;
        bool enableRandomLetterHint = false;

        foreach (string _w in _allWords)
        {
            if (_foundWords.Exists(e => e.Equals(_w)))
            {
                continue;
            }

            GameObject _wordContainer = ContainerForWord(_w);

            // first letter
            Transform _childFirst = _wordContainer.transform.GetChild(0);

            CharTable _charFirstTable = _childFirst.gameObject.GetComponent<CharTable>();

            if (_charFirstTable.found == false)
            {
                enableFirstLetterHint = true;
            }

            // last letter
            Transform _childLast = _wordContainer.transform.GetChild(_w.Length - 1);

            CharTable _charLastTable = _childLast.gameObject.GetComponent<CharTable>();

            if (_charLastTable.found == false)
            {
                enableLastLetterHint = true;
            }

            // random letter, if any word is not in _foundWords, then there is unfound character
            enableRandomLetterHint = true;
        }

        Button firstLetterHint = GameObjectHelper.GetChildComponentOnNamedGameObject<Button>(_boostsContainer, "First", true);
        Button lastLetterHint = GameObjectHelper.GetChildComponentOnNamedGameObject<Button>(_boostsContainer, "Last", true);
        Button randomLetterHint = GameObjectHelper.GetChildComponentOnNamedGameObject<Button>(_boostsContainer, "Random", true);

        firstLetterHint.interactable = enableFirstLetterHint;
        lastLetterHint.interactable = enableLastLetterHint;
        randomLetterHint.interactable = enableRandomLetterHint;
    }

    int HintPrice(int hint, int characters)
    {
        if (hint == 3)
        {
            return Constants.HintPrice * characters;
        }

        return Constants.HintPrice;
    }

    GameObject UseRandomCharacterHint(int hint, bool free = false)
    {
        int hintPrice = HintPrice(hint, 1);

        if (!free && GameManager.Instance.Player.Coins < hintPrice)
        {
            ShowNoCoinsDialog(false, (hintPrice - GameManager.Instance.Player.Coins));

            return null;
        }

        List<string> _all = ListUtils.Shuffle(_allWords);

        GameObject _wordContainerObject = null;

        foreach (string _w in _all)
        {
            if (_foundWords.Exists(e => e.Equals(_w)))
            {
                continue;
            }

            char[] characters = _w.ToCharArray();
            GameObject _wordContainer = ContainerForWord(_w);

            if (hint == 0)
            { // first
                Transform _child = _wordContainer.transform.GetChild(0);
                CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                if (_charTable.found)
                {
                    continue;
                }

                _charTable.found = true;

                _wordContainerObject = _wordContainer;
                ShowCharacter(_child.gameObject, characters[0].ToString(), 0);

                if (!free)
                {
                    GameManager.Instance.Player.RemoveCoins(hintPrice);
                }

                usedHintsCount += 1;

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("UseHint", new Dictionary<string, string>() { { "Hint", "First" } });
                    Fabric.Answers.Answers.LogCustom("UseHint", new Dictionary<string, object>() { { "Hint", "First" } });
                }

                break;
            }

            if (hint == 1)
            { // random
                int[] _indexes = GetUnfoundCharacters(characters, _wordContainer);

                if (_indexes.Length == 0)
                {
                    continue;
                }

                int index = UnityEngine.Random.Range(0, _indexes.Length - 1);
                int _charIndex = _indexes[index];

                Transform _child = _wordContainer.transform.GetChild(_charIndex);
                CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                if (_charTable.found)
                {
                    continue;
                }

                _charTable.found = true;

                _wordContainerObject = _wordContainer;
                ShowCharacter(_child.gameObject, characters[_charIndex].ToString(), 0);

                if (!free)
                {
                    GameManager.Instance.Player.RemoveCoins(hintPrice);
                }

                usedHintsCount += 1;

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("UseHint", new Dictionary<string, string>() { { "Hint", "Random" } });
                    Fabric.Answers.Answers.LogCustom("UseHint", new Dictionary<string, object>() { { "Hint", "Random" } });
                }

                break;
            }

            if (hint == 2)
            { // last
                int index = _w.Length - 1;

                Transform _child = _wordContainer.transform.GetChild(index);
                CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                if (_charTable.found)
                {
                    continue;
                }

                _charTable.found = true;

                _wordContainerObject = _wordContainer;
                ShowCharacter(_child.gameObject, characters[index].ToString(), 0);

                if (!free)
                {
                    GameManager.Instance.Player.RemoveCoins(hintPrice);
                }

                usedHintsCount += 1;

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("UseHint", new Dictionary<string, string>() { { "Hint", "Last" } });
                    Fabric.Answers.Answers.LogCustom("UseHint", new Dictionary<string, object>() { { "Hint", "Last" } });
                }

                break;
            }
        }

        return _wordContainerObject;
    }

    GameObject UseWordHint(bool free = false)
    {
        GameObject _wordContainerObject = null;
        int longest_word = LongestWordLength();

        bool _found = false;
        for (int i = 0; i < longest_word; i++)
        {
            if (_found)
            {
                break;
            }

            foreach (string _w in _allWords)
            {
                if (_foundWords.Exists(e => e.Equals(_w)))
                {
                    continue;
                }

                if (i >= _w.Length)
                {
                    continue;
                }

                GameObject _wordContainer = ContainerForWord(_w);

                int _charactersToBeFound = 0;
                for (int j = 0; j < _wordContainer.transform.childCount; j++)
                {
                    Transform _child = _wordContainer.transform.GetChild(j);
                    CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                    if (_charTable.found)
                    {
                        continue;
                    }

                    _charactersToBeFound += 1;
                }

                int hintPrice = Constants.HintPrice * _charactersToBeFound;

                if (!free && GameManager.Instance.Player.Coins < hintPrice)
                {
                    ShowNoCoinsDialog(true, (hintPrice - GameManager.Instance.Player.Coins));

                    return null;
                }

                for (int j = 0; j < _wordContainer.transform.childCount; j++)
                {
                    Transform _child = _wordContainer.transform.GetChild(j);
                    CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                    if (_charTable.found)
                    {
                        continue;
                    }

                    _charTable.found = true;

                    _wordContainerObject = _wordContainer;

                    char[] characters = _w.ToCharArray();
                    ShowCharacter(_child.gameObject, characters[j].ToString(), 0);
                }

                if (!free)
                {
                    GameManager.Instance.Player.RemoveCoins(hintPrice);
                }

                usedHintsCount += 1;
                _found = true;

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("UseHint", new Dictionary<string, string>() { { "Hint", "Word" } });
                    Fabric.Answers.Answers.LogCustom("UseHint", new Dictionary<string, object>() { { "Hint", "Word" } });
                }

                break;
            }
        }

        return _wordContainerObject;
    }

    void ShowNoCoinsDialog(bool showWord = false, int price = 0)
    {
        DialogManager.Instance.Show("NoMoneyForHintDialog",
                                            text: LocaliseText.Format("Game.NotCoinsForHintNeedMore", price),
                                    doneCallback: (DialogInstance dialogInstance) =>
        {
            if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok)
            {
#if !UNITY_EDITOR
    AdColonyManager.Instance.RequestAd(Constants.AdColonyDoubleCoins);
#endif

                Loading.Instance.Show();

                AdColonyManager.Instance.SetCallback((string zoneId, int amount, bool success) =>
                {
                    HintAdsClosed(zoneId, amount, success, showWord);
                });
                AdColonyManager.Instance.PlayAd(AdColonyManager.Instance.AdForZoneId());

                Flurry.Flurry.Instance.LogEvent("Hint_Ad_For_Word");
                Fabric.Answers.Answers.LogCustom("Hint_Ad_For_Word");
            }
            else if (dialogInstance.DialogResult == DialogInstance.DialogResultType.No)
            {
                Market();

                Flurry.Flurry.Instance.LogEvent("Hint_Shop");
                Fabric.Answers.Answers.LogCustom("Hint_Shop");
            }
        });
    }

    void HintAdsClosed(string zoneId, int amount, bool success, bool showWord)
    {
        if (!success)
        {
            ShowVideoAd((int amount2, bool success2) =>
            {
                HintAdsClosedProcess(amount2, success2, showWord);
            });
            return;
        }

        Loading.Instance.Hide();
        ProcessFreeHint(showWord);
    }

    void HintAdsClosedProcess(int amount, bool success, bool showWord)
    {
        Loading.Instance.Hide();

        if (success)
        {
            ProcessFreeHint(showWord);
        }
    }

    public void ShowVideoAd(Action<int, bool> action)
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions
            {
                resultCallback = (ShowResult result) =>
                {
                    action(0, result == ShowResult.Finished);
                }
            };
            Advertisement.Show("rewardedVideo", options);
        }
        else
        {
            action(0, false);
        }
    }

    void ProcessFreeHint(bool showWord)
    {
        if (showWord)
        {
            UseHint(3, true);
        }
        else
        {
            UseHint(0, true);
        }
    }

    GameObject UseNextLetterHintSingleWord(bool free = false)
    {
        int hintPrice = Constants.HintPrice;

        if (!free && GameManager.Instance.Player.Coins < hintPrice)
        {
            ShowNoCoinsDialog(false, (hintPrice - GameManager.Instance.Player.Coins));

            return null;
        }

        GameObject _wordContainerObject = null;

        foreach (string _w in _allWords)
        {
            if (_foundWords.Exists(e => e.Equals(_w)))
            {
                continue;
            }

            GameObject _wordContainer = ContainerForWord(_w);
            Transform _child = null;
            int foundCharIndex = 0;

            for (int i = 0; i < _w.Length; i++)
            {
                _child = _wordContainer.transform.GetChild(i);
                CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                if (_charTable.found)
                {
                    continue;
                }

                _charTable.found = true;
                foundCharIndex = i;
                break;
            }

            _wordContainerObject = _wordContainer;

            char[] characters = _w.ToCharArray();
            ShowCharacter(_child.gameObject, characters[foundCharIndex].ToString(), 0);

            if (!free)
            {
                GameManager.Instance.Player.RemoveCoins(Constants.HintPrice);
            }

            usedHintsCount += 1;

            if (!Debug.isDebugBuild)
            {
                Flurry.Flurry.Instance.LogEvent("UseHint", new Dictionary<string, string>() { { "Hint", "Letter" } });
                Fabric.Answers.Answers.LogCustom("UseHint", new Dictionary<string, object>() { { "Hint", "Letter" } });
            }

            break;
        }

        return _wordContainerObject;
    }

    GameObject UseNextLetterHint(bool free = false)
    {
        int hintPrice = Constants.HintPrice;

        if (!free && GameManager.Instance.Player.Coins < hintPrice)
        {
            ShowNoCoinsDialog(false, (hintPrice - GameManager.Instance.Player.Coins));

            return null;
        }

        GameObject _wordContainerObject = null;
        int longest_word = LongestWordLength();

        bool _found = false;
        for (int i = 0; i < longest_word; i++)
        {
            if (_found)
            {
                break;
            }

            foreach (string _w in _allWords)
            {
                if (_foundWords.Exists(e => e.Equals(_w)))
                {
                    continue;
                }

                if (i >= _w.Length)
                {
                    continue;
                }

                GameObject _wordContainer = ContainerForWord(_w);

                Transform _child = _wordContainer.transform.GetChild(i);
                CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                if (_charTable.found)
                {
                    continue;
                }

                _charTable.found = true;

                _wordContainerObject = _wordContainer;

                char[] characters = _w.ToCharArray();
                ShowCharacter(_child.gameObject, characters[i].ToString(), 0);

                if (!free)
                {
                    GameManager.Instance.Player.RemoveCoins(Constants.HintPrice);
                }

                usedHintsCount += 1;
                _found = true;

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("UseHint", new Dictionary<string, string>() { { "Hint", "Letter" } });
                    Fabric.Answers.Answers.LogCustom("UseHint", new Dictionary<string, object>() { { "Hint", "Letter" } });
                }

                break;
            }
        }

        return _wordContainerObject;
    }

    public void UseHint(int hint)
    {
        UseHint(hint, false);
    }

    public void UseHint(int hint, bool free = false)
    {
        if (_gameMode == GameMode.Multi && _isMyTurn == false)
        {
            return;
        }

        if (IsLevelCompleted())
        {
            return;
        }

        GameObject _wordContainerObject = null;

        if (hint == 3)
        {
            _wordContainerObject = UseWordHint(free);
        }
        else if (hint == 0)
        {
            if (singleWordTable)
            {
                _wordContainerObject = UseNextLetterHintSingleWord(free);
            }
            else
            {
                _wordContainerObject = UseNextLetterHint(free);
            }
        }

        if (_wordContainerObject == null)
        {
            if (_gameMode == GameMode.Single && IsLevelCompleted())
            {
                StartCoroutine(CoRoutines.DelayedCallback(0.5f, ShowLevelCompleted));
            }

            return;
        }

        StringBuilder _stringBuilder = new StringBuilder(16);

        foreach (Transform _child in _wordContainerObject.transform)
        {
            CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

            if (_charTable.found)
            {
                GameObject textObject = GameObjectHelper.GetChildNamedGameObject(_child.gameObject, "Text", true);
                Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(textObject, "Text", true);

                _stringBuilder.Append(text.text.ToLower());
            }
            else
            {
                break; // if at least one char is NOT found, stop. This means this word is not found at all
            }
        }

        string _word = _stringBuilder.ToString();

        if (IsCompleted(_word))
        {
            CompletedCorrect(_word);

            if (_gameMode == GameMode.Single && IsLevelCompleted())
            {
                StartCoroutine(CoRoutines.DelayedCallback(1.5f, ShowLevelCompleted));
            }
        }
        else
        {
            if (_gameMode == GameMode.Multi)
            {
                EndTurn();
                Reset();
                VisualReset();
            }
        }
    }

    int LongestWordLength()
    {
        int longest_word = 0;
        foreach (string _w in _allWords)
        {
            if (longest_word < _w.Length)
            {
                longest_word = _w.Length;
            }
        }

        return longest_word;
    }

    #endregion

    void CreateNewLineRenderer()
    {
        GameObject _g = new GameObject();
        _g.layer = _lineRenderer.gameObject.layer;

        RectTransform rect = _g.AddComponent<RectTransform>();

        _g.transform.SetParent(_lineRenderer.transform);
        _g.transform.localScale = _lineRenderer.transform.localScale;

        RectTransform _parentRect = _lineRenderer.gameObject.GetComponent<RectTransform>();

        rect.anchoredPosition = _parentRect.anchoredPosition;
        rect.anchorMax = _parentRect.anchorMax;
        rect.anchorMin = _parentRect.anchorMin;
        rect.sizeDelta = _parentRect.sizeDelta;

        _g.transform.localPosition = new Vector3(_g.transform.localPosition.x, _g.transform.localPosition.y, 10f);

        LineRenderer _ln = _g.AddComponent<LineRenderer>();
        _ln.useWorldSpace = _lineRenderer.useWorldSpace;
        _ln.material = Instantiate(_lineRenderer.material);
        _ln.startWidth = _lineRenderer.startWidth;
        _ln.endWidth = _lineRenderer.endWidth;
        _ln.textureMode = LineTextureMode.DistributePerSegment;

        Animator _anim = _lineRenderer.gameObject.GetComponent<Animator>();
        Animator _gAnim = _g.AddComponent<Animator>();
        _gAnim.runtimeAnimatorController = _anim.runtimeAnimatorController;

        _currentLineRenderer = _ln;

        _lineRenderers.Add(_ln);
    }

    #region Messages

    bool FacebookProfilePictureHandler(BaseMessage message)
    {
        FacebookProfilePictureMessage m = message as FacebookProfilePictureMessage;

        if (_mapUsersAvatar.ContainsKey(m.UserId))
        {
            GameObject item = _mapUsersAvatar[m.UserId];

            var image = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage>(item, "AvatarImage", true);
            image.texture = m.Texture;
        }

        return true;
    }

    #endregion

    #region Transforms

    public bool ShouldReceiveTouch(Gesture gesture, TouchPoint touch)
    {
        if (_gameMode == GameMode.Multi && _isMyTurn == false)
        {
            return false;
        }

        return true;
    }

    public bool ShouldBegin(Gesture gesture)
    {
        if (_gameMode == GameMode.Multi && _isMyTurn == false)
        {
            return false;
        }

        return true;
    }

    public bool ShouldRecognizeSimultaneously(Gesture first, Gesture second)
    {
        return true;
    }

    void UpdateDraggableSize(int _size, bool _multiply = false, GameObject _exclude = null)
    {
        foreach (GameObject _draggable in _draggableObjects)
        {
            if (_exclude != null && _draggable.GetInstanceID() == _exclude.GetInstanceID())
            {
                continue;
            }

            BoxCollider2D _boxCollider = _draggable.GetComponent<BoxCollider2D>();

            if (_multiply)
            {
                _boxCollider.size = new Vector2(_boxCollider.size.x * _size, _boxCollider.size.y * _size);
            }
            else
            {
                _boxCollider.size = new Vector2(_boxCollider.size.x / _size, _boxCollider.size.y / _size);
            }
        }
    }

    void LineRendererMoved(Vector3 _position)
    {
        if (_currentLineRenderer == null)
        {
            return;
        }

        _position = _currentLineRenderer.gameObject.transform.InverseTransformPoint(_position);

        _points[_points.Count - 1] = new Vector3(_position.x, _position.y, 0);

        _currentLineRenderer.SetPosition(1, _points[_points.Count - 1]);
    }

    private void TransformHandler(object sender, EventArgs e)
    {
        if (!_dragBegan)
        {
            return;
        }

        var gesture = sender as TransformGesture;

        if (_matchChars.Count == 2
            && Vector3.Distance(_draggingObjectStartPosition, gesture.gameObject.transform.position) < 0.21f)
        {
            RemoveWordFromSelected(gesture.gameObject); // remove last char as selected
            return;
        }

        LineRendererMoved(gesture.gameObject.transform.position);

        if (_gameMode == GameMode.Multi)
        {
            DrawQueueItem item = new DrawQueueItem
            {
                position = gesture.gameObject.transform.position,
                start = false,
                end = false
            };

            _broadcastQueue.Add(item);
            _startBroadcast = true;
        }
    }

    private void PressedHandler(object sender, EventArgs e)
    {
        if (_gameMode == GameMode.Multi && _isMyTurn == false)
        {
            return;
        }

        if (this.paused)
        {
            return;
        }

        var gesture = sender as PressGesture;

        if (_dragBegan == true)
        {
            return;
        }

        if (DoesNeedVisualReset())
        { // if user start to drag the next word while visual elements are still animating
            VisualReset();
        }

        _timeWithNoUserAction = 0.0f;
        _countingUserAction = false;
        _dragBegan = true;

        _matchChars.Add(gesture.gameObject);

        SetInPreviewWord(gesture.gameObject.transform.parent.gameObject);

        Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(gesture.gameObject.transform.parent.gameObject, "Text", true);
        _matchWord.Append(text.text);

        LineRendererStartDragging(gesture.gameObject.transform.position);

        _draggingObject = gesture.gameObject;
        _draggingObjectStartPosition = _draggingObject.transform.position;

        BoxCollider2D _boxCollider = _draggingObject.GetComponent<BoxCollider2D>();
        _boxCollider.size = new Vector2(_boxCollider.size.x / 2, _boxCollider.size.y / 2);

        UpdateDraggableSize(2, false, _draggingObject);

        if (_gameMode == GameMode.Multi)
        {
            DrawQueueItem item = new DrawQueueItem
            {
                position = gesture.gameObject.transform.position,
                start = true,
                end = false
            };

            _broadcastQueue.Add(item);
            _startBroadcast = true;
        }
    }

    void LineRendererStartDragging(Vector3 _position)
    {
        CreateNewLineRenderer();

        _position = _currentLineRenderer.gameObject.transform.InverseTransformPoint(_position);

        _points.Add(new Vector3(_position.x, _position.y, 0));
        _points.Add(new Vector3(_position.x, _position.y, 0));

        _currentLineRenderer.SetPosition(0, _points[0]);
        _currentLineRenderer.SetPosition(1, _points[1]);
    }

    private void ReleasedHandler(object sender, EventArgs e)
    {
        if (!_dragBegan)
        {
            return;
        }

        if (_draggingObject == null)
        {
            return;
        }

        _countingUserAction = true;

        var gesture = sender as ReleaseGesture;

        BoxCollider2D _boxCollider = _draggingObject.GetComponent<BoxCollider2D>();

        if (_boxCollider == null)
        {
            return;
        }

        _boxCollider.size = new Vector2(_boxCollider.size.x * 2, _boxCollider.size.y * 2);

        UpdateDraggableSize(2, true, _draggingObject);

        if (_gameMode == GameMode.Multi)
        {
            DrawQueueItem item = new DrawQueueItem
            {
                position = gesture.gameObject.transform.position,
                start = false,
                end = true
            };

            _broadcastQueue.Add(item);
            _startBroadcast = true;
        }

        string _word = _matchWord.ToString().ToLower();

        if (IsCompleted(_word))
        {
            CompletedCorrect(_word);

            if (_gameMode == GameMode.Single && IsLevelCompleted())
            {
                StartCoroutine(CoRoutines.DelayedCallback(1.0f, ShowLevelCompleted));
            }
        }
        else
        {
            CompletedWrong(_word);
        }

        Reset();

        _dragBegan = false;
        _draggingObject = null;
        _draggingObjectStartPosition = new Vector3(0, 0, 0);

        _visualResetCoroutine = CoRoutines.DelayedCallback(1.0f, () =>
        {
            VisualReset();
        });

        StartCoroutine(_visualResetCoroutine);
    }

    public void RemoveWordFromSelected(GameObject wordObject)
    {
        GameObject _beforeLastObject = _matchChars.ElementAt(_matchChars.Count - 2);

        if (_beforeLastObject == wordObject)
        {
            int j = _matchChars.Count - 1; // remove only last selected char
            for (int i = _matchChars.Count; i > j; i--)
            {
                GameObject _removeObject = _matchChars.Last();

                if (_removeObject == null)
                {
                    continue;
                }

                LineRendererRemoveChar(_removeObject.transform.position);

                _matchChars.RemoveAt(_matchChars.Count - 1);

                RemoveLastFromPreview();

                string _charText = _matchWord[_matchWord.Length - 1].ToString();

                _matchWord.Remove(_matchWord.Length - 1, 1);

                if (_gameMode == GameMode.Multi)
                {
                    DrawQueueItem item = new DrawQueueItem
                    {
                        position = _removeObject.transform.position,
                        start = false,
                        end = false,
                        character = _charText,
                        touched = false
                    };

                    _broadcastQueue.Add(item);
                    _startBroadcast = true;
                }
            }
        }
    }

    public void WordTouched(GameObject wordObject)
    {
        if (!_dragBegan)
        {
            return;
        }

        if (_draggingObject == wordObject)
        {
            return;
        }

        if (_matchChars.Contains(wordObject))
        {
            RemoveWordFromSelected(wordObject);

            return;
        }

        LineRendererCharTouched(wordObject.transform.position);

        _matchChars.Add(wordObject);

        SetInPreviewWord(wordObject.transform.parent.gameObject);

        Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(wordObject.transform.parent.gameObject, "Text", true);
        _matchWord.Append(text.text);

        if (_gameMode == GameMode.Multi)
        {
            DrawQueueItem item = new DrawQueueItem
            {
                position = wordObject.transform.position,
                start = false,
                end = false,
                character = text.text,
                touched = true
            };

            _broadcastQueue.Add(item);
            _startBroadcast = true;
        }
    }

    void LineRendererRemoveChar(Vector3 _position)
    {
        if (_currentLineRenderer == null)
        {
            return;
        }

        _position = _currentLineRenderer.gameObject.transform.InverseTransformPoint(_position);

        Destroy(_currentLineRenderer.gameObject);

        _lineRenderers.RemoveAt(_lineRenderers.Count - 1);
        _currentLineRenderer = _lineRenderers.Last();

        _points.Clear();

        _points.Add(_currentLineRenderer.GetPosition(0));
        _points.Add(new Vector3(_position.x, _position.y, 0));

        _currentLineRenderer.SetPosition(1, _points[1]);
    }

    void LineRendererCharTouched(Vector3 _position)
    {
        if (_currentLineRenderer == null)
        {
            return;
        }

        _position = _currentLineRenderer.gameObject.transform.InverseTransformPoint(_position);

        _points[_points.Count - 1] = new Vector3(_position.x, _position.y, 0);

        _currentLineRenderer.SetPosition(1, _points[_points.Count - 1]);

        CreateNewLineRenderer();
        _points.Clear();

        _points.Add(new Vector3(_position.x, _position.y, 0));
        _points.Add(new Vector3(_position.x, _position.y, 0));

        _currentLineRenderer.SetPosition(0, _points[0]);
        _currentLineRenderer.SetPosition(1, _points[1]);
    }

    #endregion

    public List<string> GetFoundWords()
    {
        return _foundWords;
    }

    bool IsCompleted(string word)
    {
        int length = word.Length;

        if ( singleWordTable ) // extra checks
        {
            if (length != _allWords[indexOfSearchingWord].Length)
            {
                return false;
            }

            if ( CanFitInContainer(word, indexOfSearchingWord) == false ) {
                return false;
            }
        }

        foreach (JSONValue _w in _gameWords)
        {
            string _word = _w.Obj.GetString("word");

            if (_word.Length != word.Length)
            {
                continue;
            }

            if (String.Compare(_word, word, true) == 0)
            {
                return true;
            }
        }

        return false;
    }

    GameObject ContainerForWord(string word) {
        if ( singleWordTable ) {
            GameObject _container = _wordsCointainersItems[indexOfSearchingWord];

            if (word.Length != _container.transform.childCount)
            {
                return null;
            }

            string[] _word = new string[word.Length];

            int index = 0;
            int common_chars = 0;
            foreach (Transform _child in _container.transform)
            {
                CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

                if (_charTable.found)
                {
                    GameObject textObject = GameObjectHelper.GetChildNamedGameObject(_child.gameObject, "Text", true);
                    Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(textObject, "Text", true);

                    string _entryChar = text.text.ToLower();

                    char _char = word[index];

                    // this character fit in this box
                    if (_char.ToString().ToLower() == _entryChar)
                    {
                        common_chars++;
                    }
                }
                else
                {
                    common_chars++;
                }

                index++;
            }

            return (common_chars == word.Length) ? _container : null;
        }

        return _wordsCointainers[word];
    }

    void SetWordAsFound(string word)
    {
        if (!_wordsCointainers.ContainsKey(word))
        {
            return;
        }

        if (_foundWords.Exists(e => e.Equals(word)))
        {
            return;
        }

        _foundWords.Add(word);

        GameObject container = ContainerForWord(word);

        char[] characters = word.ToCharArray();

        int index = 0;
        foreach (Transform _child in container.transform)
        {
            CharTable _charTable = _child.gameObject.GetComponent<CharTable>();
            _charTable.found = true;

            ShowCharacter(_child.gameObject, character: characters[index].ToString(), index: index, animating: false);

            index++;
        }
    }

    void CompletedCorrect(string word, bool sendToServer = true)
    {
        if (!_wordsCointainers.ContainsKey(word))
        {
            return;
        }

        if (_foundWords.Exists(e => e.Equals(word)))
        {
            ButtonUtils.PlayAlreadySound();

            SetPreviewAlready();

            foreach (LineRenderer _lr in _lineRenderers)
            {
                Animator _anim = _lr.gameObject.GetComponent<Animator>();
                _anim.SetTrigger("Already");
            }

            _previewHideCoroutine = CoRoutines.DelayedCallback(0.5f, HidePreview);
            StartCoroutine(_previewHideCoroutine);

            if (_gameMode == GameMode.Multi && sendToServer == true)
            {
                _actionsQueue.Enqueue(() =>
                {
                    ChallengeManager.Instance.SendFoundWord(word, Time.time - _startTime, (LogChallengeEventResponse response) =>
                    {
                        if (!response.HasErrors)
                        {
                            MyDebug.Log(response.JSONData);
                        }
                    });
                });

                EndTurn();
            }

            return;
        }

        _foundWords.Add(word);

        if (_gameMode == GameMode.Single)
        {
            Level level = GameManager.Instance.Levels.Selected;

            PreferencesFactory.SetString(Constants.KeyFoundWords, string.Join("|", _foundWords.ToArray()));
            PreferencesFactory.SetInt(Constants.KeyFoundWordsLevel, level.Number);
        }

        GameObject container = ContainerForWord(word);

        char[] characters = word.ToCharArray();

        int index = 0;
        float delay = 0.35f;
        foreach (Transform _child in container.transform)
        {
            CharTable _charTable = _child.gameObject.GetComponent<CharTable>();
            _charTable.found = true;

            ShowCharacter(_child.gameObject, characters[index].ToString(), index);
            delay += index * 0.1f;

            index++;
        }

        if (singleWordTable)
        {
            if (_scrollSnap.CurrentPage != indexOfSearchingWord)
            {
                _scrollSnap.GoToScreen(indexOfSearchingWord);
            }

            ShowNextWord(delay);
        }

        ButtonUtils.PlayCorrectSound();

        SetPreviewCorrect();

        if ((_gameMode == GameMode.Multi && sendToServer == true) || _gameMode == GameMode.Single)
        {
            ShowGreetingText();
            ShowLadyWaterDrop();
        }

        foreach (LineRenderer _lr in _lineRenderers)
        {
            Animator _anim = _lr.gameObject.GetComponent<Animator>();
            _anim.SetTrigger("Correct");
        }

        _previewHideCoroutine = CoRoutines.DelayedCallback(0.5f, HidePreview);
        StartCoroutine(_previewHideCoroutine);

        if (_gameMode == GameMode.Multi && sendToServer == true)
        {
            _actionsQueue.Enqueue(() =>
            {
                ChallengeManager.Instance.SendFoundWord(word, Time.time - _startTime, (LogChallengeEventResponse response) =>
                {
                    if (!response.HasErrors)
                    {
                        MyDebug.Log(response.JSONData);
                    }
                });
            });

            EndTurn();
        }
    }

    void ShowCharacter(GameObject GO, string character, int index = 0, bool animating = true)
    {
        GameObject textObject = GameObjectHelper.GetChildNamedGameObject(GO, "Text", true);
        Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(textObject, "Text", true);

        if (animating)
        {
            textObject.transform.DOScale(Vector3.zero, 0f);

            text.text = character.ToUpper();

            textObject.transform.DOScale(new Vector3(1, 1, 1), 0.35f).SetEase(Ease.OutElastic).SetDelay(index * 0.1f);

            ParticleSystem _particle = GameObjectHelper.GetChildComponentOnNamedGameObject<ParticleSystem>(GO, "Particle", true);
            _particle.Play();

        }
        else
        {
            text.text = character.ToUpper();
        }
    }

    void CompletedWrong(string word, bool send = true)
    {
        wrongTimes += 1;

        ButtonUtils.PlayWrongSound();

        SetPreviewWrong();

        foreach (LineRenderer _lr in _lineRenderers)
        {
            Animator _anim = _lr.gameObject.GetComponent<Animator>();
            _anim.SetTrigger("Wrong");
        }

        _previewHideCoroutine = CoRoutines.DelayedCallback(0.5f, HidePreview);
        StartCoroutine(_previewHideCoroutine);

        if (_gameMode == GameMode.Multi && send == true)
        {
            _actionsQueue.Enqueue(() =>
            {
                ChallengeManager.Instance.SendFoundWord(word, Time.time - _startTime, (LogChallengeEventResponse response) =>
                {
                    if (!response.HasErrors)
                    {
                        MyDebug.Log(response.JSONData);
                    }
                });
            });

            EndTurn();
        }

        if (_gameMode == GameMode.Single)
        {
            if (wrongTimes > 0 && wrongTimes % 3 == 0)
            {
                StartCoroutine(CoRoutines.DelayedCallback(1.0f, AnimateAskFriends));
            }
        }
    }

    bool IsLevelCompleted()
    {
        if (_foundWords.Count == _gameWords.Length)
        {
            return true;
        }

        return false;
    }

    int PointsForCurrentLevel()
    {
        int points = 0;

        foreach (JSONValue _w in _gameWords)
        {
            string _word = _w.Obj.GetString("word");

            points += _word.Length;
        }

        return points;
    }

    void ShowLadyWaterDrop()
    {
        Animator _anim = _lady.GetComponent<Animator>();
        _anim.SetTrigger("Drop");
    }

    void ShowGreetingText()
    {
        int i = (int)Math.Ceiling(_allWords.Count / 2.0f);
        if (!(_foundWords.Count % i == 0))
        {
            return;
        }

        string[] _greetingTexts = new string[9]{
            LocaliseText.Get("Greeting.Farmtastic"),
            LocaliseText.Get("Greeting.Fantastic"),
            LocaliseText.Get("Greeting.Good"),
            LocaliseText.Get("Greeting.Great"),
            LocaliseText.Get("Greeting.Excellent"),
            LocaliseText.Get("Greeting.Nice"),
            LocaliseText.Get("Greeting.Super"),
            LocaliseText.Get("Greeting.Wonderful"),
            LocaliseText.Get("Greeting.Awesome"),
        };

        int index = UnityEngine.Random.Range(0, _greetingTexts.Length);

        Text text = _greetingObject.GetComponent<Text>();
        text.text = _greetingTexts[index];

        _greetingObject.SetActive(true);

        ButtonUtils.PlayFantasticSound();
    }

    void ShowLevelCompleted()
    {
        this.Pause();

        // delete cached level progress
        PreferencesFactory.DeleteKey(Constants.KeyFoundWordsLevel);
        PreferencesFactory.DeleteKey(Constants.KeyFoundWords);

        if (LevelController.NeedToGenerateMoreLevels())
        {
            LevelController.GenerateMoreLevels();
            LevelController.LoadLevelFromServer();
        }

        DialogInstance dialogInstance = DialogManager.Instance.Show("LevelCompleted");
        _levelCompleted = dialogInstance.GetComponent<LevelCompleted>();

        var currentLevel = GameManager.Instance.Levels.Selected;

        int points = 0;

        if (currentLevel.ProgressBest < 0.9f)
        { // first time played
            points = PointsForCurrentLevel();
        }

        LoadFacebookFriendsPlaying();

        _levelCompleted.Show(true, time, points);

        if (!Debug.isDebugBuild)
        {
            Fabric.Answers.Answers.LogLevelEnd(string.Format("Level_{0}", currentLevel.Number));
        }
    }

    void LoadFacebookFriendsPlaying()
    {
        if (_levelCompleted == null)
        {
            return;
        }

        GameObject FacebookButton = GameObjectHelper.GetChildNamedGameObject(_levelCompleted.gameObject, "FacebookButton", true);

        if (PreferencesFactory.HasKey(Constants.ProfileFBUserId))
        {
            FacebookButton.SetActive(false);
            var currentLevel = GameManager.Instance.Levels.Selected;

            GameSparksManager.FriendsForLevel(currentLevel.Number, time, (List<UserLevelInfo> list) =>
            {
                if (list.Count == 0)
                {
                    return;
                }

                if (_levelCompleted == null)
                {
                    return;
                }

                GameObject FriendsPlaying = GameObjectHelper.GetChildNamedGameObject(_levelCompleted.gameObject, "FriendsPlaying", true);
                FriendsPlaying.SetActive(true);

                GameObject Friends = GameObjectHelper.GetChildNamedGameObject(FriendsPlaying, "Friends", true);
                FriendsPopulate fp = Friends.GetComponent<FriendsPopulate>();
                fp.Populate(list, currentLevel.Number);
            });
        }
        else
        {
            FacebookButton.SetActive(true);
        }
    }

    bool OnFacebookLoginMessage(BaseMessage message)
    {
        var facebookLoginMessage = message as FacebookLoginMessage;

        if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.OK)
        {
            LoadFacebookFriendsPlaying();
        }

        return true;
    }

    bool GameResetedHandler(BaseMessage message)
    {
        if (_gameMode == GameMode.Multi)
        {
            return true;
        }

        // change to first unplayed level only on single mode
        Level level = LevelController.FirstUnplayedLevel();

        if (level != null && GameManager.Instance.Levels.Selected.Number != level.Number)
        {
            GameManager.Instance.Levels.Selected = level;

            GameManager.LoadSceneWithTransitions("Game");
        }

        return true;
    }

    bool SettingsStateHandler(BaseMessage message)
    {
        SettingsStateChanged msg = (SettingsStateChanged)message;

        if (msg.Opened)
        {
            this.Pause();
        }
        else
        {
            this.Resume();
        }

        return true;
    }

    bool LocalisationHandler(BaseMessage message)
    {
        if (_gameMode != GameMode.Single)
        {
            return true;
        }

        singleWordTable = ShouldSetAsSingleWordTable();

        if (SettingsContainer.Instance.isSettingsOpened)
        {
            NextLevel();

            // game stays paused
            this.Pause();
        }

        return true;
    }

    void Reset()
    {
        _points.Clear();
        _matchWord.Length = 0; // reset stringbuilder because there is no Clear() method
        _matchChars.Clear();

        if (_draggingObject != null)
        {
            _draggingObject.transform.position = _draggingObjectStartPosition;
        }
    }

    void VisualReset()
    {
        if (_visualResetCoroutine != null)
        {
            StopCoroutine(_visualResetCoroutine);
            _visualResetCoroutine = null;
        }

        if (_previewHideCoroutine != null)
        {
            StopCoroutine(_previewHideCoroutine);
            _previewHideCoroutine = null;
        }

        foreach (Transform _child in _lineRenderer.transform)
        {
            Destroy(_child.gameObject);
        }

        ClearPreview();

        _lineRenderers.Clear();
    }

    bool DoesNeedVisualReset()
    {
        return _lineRenderers.Count > 0;
    }

    #region Preview

    GameObject FindCharacterInPosition(Vector3 position)
    {
        GameObject previewObject = null;

        foreach (GameObject _draggable in _draggableObjects)
        {
            BoxCollider2D _col = _draggable.GetComponent<BoxCollider2D>();

            if (_col.OverlapPoint(position))
            {
                previewObject = _draggable.transform.parent.gameObject;
                break;
            }
        }

        return previewObject;
    }

    void SetInPreviewWord(GameObject wordObject)
    {
        Text _wordText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(wordObject, "Text", true);

        GameObject _c = Instantiate(_charPrefab, _previewContainer.transform);

        RectTransform _rect = _c.GetComponent<RectTransform>();
        _rect.sizeDelta = new Vector2(_rect.sizeDelta.x / 2, _rect.sizeDelta.y);

        GameObject _text = GameObjectHelper.GetChildNamedGameObject(_c, "Text", true);
        _text.GetComponent<Text>().text = _wordText.text;

        Image _image = _previewContainer.GetComponent<Image>();
        _image.enabled = true;
        _image.color = _defaultColor;
    }

    void RemoveLastFromPreview()
    {
        Transform _previewChild = _previewContainer.transform.GetChild(_previewContainer.transform.childCount - 1);
        Destroy(_previewChild.gameObject);
    }

    void ClearPreview()
    {
        Image _image = _previewContainer.GetComponent<Image>();
        _image.enabled = false;
        _image.color = _defaultColor;

        foreach (Transform _child in _previewContainer.transform)
        {
            Destroy(_child.gameObject);
        }
    }

    void HidePreview()
    {
        Image _image = _previewContainer.GetComponent<Image>();
        _image.DOFade(0.0f, 0.25f).SetEase(Ease.Linear);

        foreach (Transform _child in _previewContainer.transform)
        {
            Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(_child.gameObject, "Text", true);

            if (text != null)
            {
                text.DOFade(0.0f, 0.25f).SetEase(Ease.Linear);
            }
        }
    }

    void SetPreviewCorrect()
    {
        if (_previewContainer.transform.childCount == 0)
        {
            return;
        }

        Image _image = _previewContainer.GetComponent<Image>();
        _image.enabled = true;
        _image.DOColor(_correctColor, 0.25f).SetEase(Ease.Linear);
    }

    void SetPreviewWrong()
    {
        if (_previewContainer.transform.childCount == 0)
        {
            return;
        }

        Image _image = _previewContainer.GetComponent<Image>();
        _image.enabled = true;
        _image.DOColor(_wrongColor, 0.25f).SetEase(Ease.Linear);
    }

    void SetPreviewAlready()
    {
        if (_previewContainer.transform.childCount == 0)
        {
            return;
        }

        Image _image = _previewContainer.GetComponent<Image>();
        _image.enabled = true;
        _image.DOColor(_alreadyColor, 0.25f).SetEase(Ease.Linear);
    }

    #endregion

    static GameObject _marketObject;

    public void Market()
    {
        if (_marketObject != null)
        {
            return;
        }

        Pause();

        GameObject purchaseButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "PurchaseButton", true);

        if (purchaseButton == null)
        {
            return;
        }

        GameObject originalParent = purchaseButton.transform.parent.gameObject;
        Vector3 originalPosition = purchaseButton.transform.position;

        DialogInstance marketInstance = DialogManager.Instance.Show("Market", doneCallback: (DialogInstance dialogInstance) =>
        {
            _marketObject = null;

            if (purchaseButton != null && originalParent != null)
            {
                GameObjectUtils.MoveObjectTo(purchaseButton, originalParent, originalPosition);
            }

            Resume();
        });

        _marketObject = marketInstance.gameObject;

        GameObjectUtils.MoveObjectTo(purchaseButton, marketInstance.Target);
    }

    public void Settings()
    {
        GameManager.SafeQueueMessage(new SettingsOpenMessage());
    }

    public void MainMenu()
    {
        if (_gameMode == GameMode.Multi)
        {
            DialogManager.Instance.Show(prefabName: "ConfirmDialog",
                title: LocaliseText.Get("Game.QuitMultiplayer"),
                text: LocaliseText.Get("Game.QuitMultiplayerDescr"),
                doneCallback: (DialogInstance dialogInstance) =>
                {
                    if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok)
                    {
                        ChallengeManager.Instance.CancelChallenge();

                        GameManager.LoadSceneWithTransitions("Lobby");
                    }
                });

            return;
        }

        DialogManager.Instance.Show(prefabName: "ConfirmDialog",
            title: LocaliseText.Get("Game.QuitLevel"),
            text: LocaliseText.Get("Game.QuitLevelDescr"),
            doneCallback: (DialogInstance dialogInstance) =>
            {
                if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok)
                {
                    PreferencesFactory.SetInt(Constants.KeyShowSelectedPack, 1);

                    GameManager.LoadSceneWithTransitions("Levels");
                }
            });
    }

    #region Multiplayer

    void CloseAllDialogs()
    {
        if (_levelCompleted != null)
        {
            _levelCompleted.GetComponent<DialogInstance>().Done();
        }

        if (_rematchObject != null)
        {
            _rematchObject.GetComponent<DialogInstance>().Done();
        }
    }

    public void Rematch()
    {
        if (!_userLeftMatch)
        {
            _levelCompleted.GetComponent<DialogInstance>().Done();

            var opponentId = GameSparksManager.Instance.Opponent.UserId;
            ChallengeManager.Instance.CreateChallengeWithUser(opponentId, true);

            DialogInstance dialogInstance = DialogManager.Instance.Show("Rematch");

            _rematchObject = dialogInstance.gameObject;

            GameObject waitingObject = GameObjectHelper.GetChildNamedGameObject(dialogInstance.Content, "Waiting", true);
            waitingObject.SetActive(true);

            Text usernameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(waitingObject, "UsernamePlaceholder", true);
            usernameText.text = GameSparksManager.Instance.Opponent.UserName;

            string avatarUploadId = GameSparksManager.Instance.Opponent.AvatarUploadId;

            var image = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage>(waitingObject, "AvatarImage", true);

            if (avatarUploadId != null)
            {
                GameSparksManager.Instance.DownloadAvatar(avatarUploadId, (Texture2D tex) =>
                {
                    if (tex != null && image != null)
                    {
                        image.texture = tex;
                    }
                });
            }
            else
            {
                string FBUserId = GameSparksManager.Instance.Opponent.ExternalIds == null ? null : GameSparksManager.Instance.Opponent.ExternalIds.GetString("FB");

                if (FBUserId != null && !_mapUsersAvatar.ContainsKey(FBUserId))
                {
                    _mapUsersAvatar.Add(FBUserId, image.gameObject);

                    FacebookRequests.Instance.LoadProfileImages(FBUserId);
                }
            }
        }
        else
        {
            GameManager.LoadSceneWithTransitions("Lobby");
        }
    }

    void MoveTurnIndicatorForward()
    {
        _turnIndicator.GetComponent<Animator>().SetTrigger("Forward");
    }

    void MoveTurnIndicatorBackward()
    {
        _turnIndicator.GetComponent<Animator>().SetTrigger("Backward");
    }

    void LogDragging(List<DrawQueueItem> _queue)
    { // world position
        ChallengeManager.Instance.SendDragPosition(_queue, _table.transform.position, delegate (LogChallengeEventResponse response)
        {
            if (!response.HasErrors)
            {

            }
        });
    }

    public void OnChallengeStarted(bool isMyTurn, string challengerName, string challengedName)
    {
        if (_rematchObject != null)
        {
            DialogInstance dialog = _rematchObject.GetComponent<DialogInstance>();
            dialog.Done();
        }

        NextLevel();

        _isMyTurn = isMyTurn;
        _turnTime = Constants.ChallengeTurnDuration;

        SetUserTurnIndicator();

        Text nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(multiplayerContainer, "Name", true);
        nameText.text = challengerName;

        Text opponentNameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(multiplayerContainer, "OpponentName", true);
        opponentNameText.text = challengedName;

        _startTime = Time.time;
    }

    public void OnPositionDetected(List<DrawQueueItem> items, Vector3 center)
    {
        Vector3 _myCenter = _table.transform.position;
        Vector3 _offset = _myCenter - center;

        foreach (DrawQueueItem item in items)
        {
            // recalculate position based on receiver device aspect ratio
            Vector3 pos = item.position + _offset;

            item.position = pos;

            _drawPositionsQueue.Enqueue(item);
        }

        if (_startDrawFromQueue == false && _drawPositionsQueue.Count > 0)
        {
            _startDrawFromQueue = true;
        }
    }

    void ProcessQueueItem(DrawQueueItem item)
    {
        if (item.start)
        {
            LineRendererStartDragging(item.position);

            GameObject previewObject = FindCharacterInPosition(item.position);

            if (previewObject != null)
            {
                SetInPreviewWord(previewObject);
            }
        }
        else if (item.end)
        {

        }
        else
        {
            if (item.character == null)
            {
                LineRendererMoved(item.position);
            }
            else
            {
                if (item.touched)
                {
                    LineRendererCharTouched(item.position);

                    GameObject previewObject = FindCharacterInPosition(item.position);

                    if (previewObject != null)
                    {
                        SetInPreviewWord(previewObject);
                    }
                }
                else
                {
                    LineRendererRemoveChar(item.position);

                    RemoveLastFromPreview();
                }
            }
        }
    }

    void ChallengeInfo()
    {
        var challengeId = ChallengeManager.Instance.GetCurrentChallengeId();

        if (challengeId == null)
        {
            return;
        }

        ChallengeManager.Instance.ChallengeInfo(challengeId, (GSData scriptData) =>
        {
            if (scriptData.ContainsKey("turnStartTime") && scriptData.ContainsKey("currentTime"))
            {
                long timestamp = Int64.Parse(scriptData.GetString("turnStartTime"));
                long currentTime = Int64.Parse(scriptData.GetString("currentTime"));

                float diff = (currentTime - timestamp) / 1000.0f;

                var nextPlayerId = scriptData.GetString("nextPlayer");
                var isMyTurn = (nextPlayerId == PreferencesFactory.GetString(Constants.ProfileUserId));

                if (isMyTurn)
                {
                    _isMyTurn = isMyTurn;
                    GameSparksManager.Instance.SetIsMyTurn(isMyTurn);

                    Reset();
                    VisualReset();

                    _dragBegan = false;

                    SetUserTurnIndicator();

                    if (diff > 0)
                    {
                        _turnTime = Constants.ChallengeTurnDuration - diff;
                        _startTime = Time.time;
                    }
                }
            }
        });
    }

    public void OnTurnEnd(Vector3 position, string word)
    {
        StartCoroutine(CoRoutines.DelayedCallback(1.0f, () =>
        {
            Reset();
            VisualReset();

            _dragBegan = false;

            _isMyTurn = GameSparksManager.Instance.GetIsMyTurn();
            _turnTime = Constants.ChallengeTurnDuration;

            _startTime = Time.time;

            SetUserTurnIndicator();
        }));
    }

    public void OnChallengeFinishedEvent(ChallengeManager.GameStates gameState,
        ChallengeManager.GameStateMessage message)
    {
        _turnTime = 0;

        if (gameState == ChallengeManager.GameStates.Leaved)
        {
            CloseAllDialogs();

            _userLeftMatch = true;
            DialogManager.Instance.ShowInfo(LocaliseText.Get("Game.OpponentLeftGame"), doneCallback: (DialogInstance dialogInstance) =>
            {
                GameManager.LoadSceneWithTransitions("Lobby");
            });
        }
        else
        {
            _userLeftMatch = false;

            StartCoroutine(CoRoutines.DelayedCallback(Constants.DelayButtonClickAction, () =>
            {
                DialogInstance dialogInstance = DialogManager.Instance.Show("LevelCompleted");
                _levelCompleted = dialogInstance.GetComponent<LevelCompleted>();

                _levelCompleted.ShowMultiplayer(gameState, message, time);

                GameSparksManager.Instance.GetPoints((int points) =>
                {
                    GameManager.Instance.Player.Score = points;
                    GameManager.Instance.Player.UpdatePlayerPrefs();
                });

                if (!Debug.isDebugBuild)
                {
                    Fabric.Answers.Answers.LogLevelEnd("Multiplayer");
                }
            }));
        }

        GameSparksManager.Instance.SetCurrentChallengeId(null);
    }

    public void OnChallengeIssued(string challengeId, string username)
    {
        _rematchChallengeId = challengeId;

        _levelCompleted.GetComponent<DialogInstance>().Done();

        DialogInstance dialogInstance = DialogManager.Instance.Show("Rematch");

        _rematchObject = dialogInstance.gameObject;

        GameObject receivingObject = GameObjectHelper.GetChildNamedGameObject(dialogInstance.Content, "Receiving", true);
        receivingObject.SetActive(true);

        Text usernameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(receivingObject, "UsernamePlaceholder", true);
        usernameText.text = username;

        string avatarUploadId = GameSparksManager.Instance.Opponent.AvatarUploadId;
        var image = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage>(receivingObject, "AvatarImage", true);

        if (avatarUploadId != null)
        {
            GameSparksManager.Instance.DownloadAvatar(avatarUploadId, (Texture2D tex) =>
            {
                if (tex != null && image != null)
                {
                    image.texture = tex;
                }
            });
        }
        else
        {
            string FBUserId = GameSparksManager.Instance.Opponent.ExternalIds == null ? null : GameSparksManager.Instance.Opponent.ExternalIds.GetString("FB");

            if (FBUserId != null && !_mapUsersAvatar.ContainsKey(FBUserId))
            {
                _mapUsersAvatar.Add(FBUserId, image.gameObject);

                FacebookRequests.Instance.LoadProfileImages(FBUserId);
            }
        }
    }

    public void AcceptRematch()
    {
        if (_rematchChallengeId == null)
        {
            return;
        }

        new AcceptChallengeRequest()
            .SetChallengeInstanceId(_rematchChallengeId)
            .Send((response) => { _startTime = Time.time; });

        DialogInstance dialog = _rematchObject.GetComponent<DialogInstance>();
        dialog.Done();

        _rematchChallengeId = null;
    }

    public void DeclineRematch()
    {
        if (_rematchChallengeId == null)
        {
            return;
        }

        new DeclineChallengeRequest()
            .SetChallengeInstanceId(_rematchChallengeId)
            .Send((response) => { });

        DialogInstance dialog = _rematchObject.GetComponent<DialogInstance>();
        dialog.Done();

        StartCoroutine(CoRoutines.DelayedCallback(Constants.DelayButtonClickAction, () =>
        {
            GameManager.LoadSceneWithTransitions("Lobby");
        }));

        _rematchChallengeId = null;
    }

    public void OnChallengeDeclined(string playerName)
    {
        DialogInstance dialog = _rematchObject.GetComponent<DialogInstance>();
        dialog.Done();

        DialogManager.Instance.ShowInfo(LocaliseText.Format("Game.RequestDeclined", playerName), doneCallback: (DialogInstance dialogInstance) =>
        {
            GameManager.LoadSceneWithTransitions("Lobby");
        });
    }

    public void OnChallengesListeFetched(List<ChallengeManager.Challenge> challenges)
    {
        // NOT USED IN THIS CLASS
    }

    public void OnErrorReceived(string message)
    {
        MyDebug.Log("OnErrorReceived: " + message);

        if (message == "REJOIN_LOBBY")
        {
            GameManager.LoadSceneWithTransitions("Lobby");
        }
        else
        {
            DialogManager.Instance.ShowInfo(message, doneCallback: (DialogInstance dialogInstance) =>
            {
                GameManager.LoadSceneWithTransitions("Lobby");
            });
        }
    }

    public void OnAchievmentEarned(string achievementName)
    {

    }

    public void OnWordFound(string word, WordState state)
    {
        if (state == WordState.Found || state == WordState.Already)
        {
            CompletedCorrect(word, false);
        }

        if (state == WordState.NotFound)
        {
            CompletedWrong(word, false);
        }
    }

    void EndTurn()
    {
        _actionsQueue.Enqueue(() =>
        {
            EndTurnProcess();
        });
    }

    void EndTurnProcess()
    {
        if (!_sendTurnTakenEvent)
        {
            _sendTurnTakenEvent = true;
            ChallengeManager.Instance.SendPlayerTurnEvent(delegate (LogChallengeEventResponse response)
                {
                    if (!response.HasErrors)
                    {
                        _isMyTurn = !_isMyTurn;
                        //GameSparksManager.Instance.SetIsMyTurn(_isMyTurn);

                        _turnTime = Constants.ChallengeTurnDuration;

                        SetUserTurnIndicator();
                    }
                    else
                    {
                        MyDebug.Log("ERROR: " + response.Errors.JSON);

                        var error = response.Errors.GetString("challengeInstanceId");
                        if (error == "NOT_YOUR_TURN")
                        {
                            DialogManager.Instance.ShowError(LocaliseText.Get("Game.NotYourTurn"));
                        }
                        else if (error == "DECLINED")
                        {
                            DialogManager.Instance.ShowError(LocaliseText.Get("Game.YourRequestWasDeclined"));
                        }
                    }
                    _sendTurnTakenEvent = false;
                });
        }
    }

    #endregion

    public Texture2D TakeScreenShot()
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

        //Render from all!
        foreach (Camera cam in Camera.allCameras)
        {
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;
        }

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        Camera.main.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);

        return screenShot;
    }

    void AnimateAskFriends()
    {
        GameObject _askFriends = GameObjectHelper.GetChildNamedGameObject(gameObject, "AskFriends", true);

        if (_askFriends == null)
        {
            return;
        }

        Animator _anim = _askFriends.GetComponent<Animator>();
        _anim.SetTrigger("Pressed");
    }

    public void AskFriends()
    {
        if (_askFriendsDialog != null)
        {
            _askFriendsDialog.SetActive(false);
        }

        Texture2D screenshot = TakeScreenShot();

#if UNITY_EDITOR
        string filePath = Application.persistentDataPath + "/screen.jpg";
        byte[] bytes = screenshot.EncodeToJPG(100);

        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log(filePath);

        AskFriendsReward();
#endif

#if UNITY_IOS
        IOSSocialManager.OnMediaSharePostResult += HandleOnShareCallback;
        IOSSocialManager.Instance.ShareMedia(LocaliseText.Format("Game.AskFriendsShareText", string.Format("\n{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.AskFriends), Constants.HashTagSocials)), screenshot);
#endif

#if UNITY_ANDROID
        AndroidSocialGate.OnShareIntentCallback += HandleOnShareIntentCallback;
        AndroidSocialGate.StartShareIntent(LocaliseText.Get("Game.AskFriends"), LocaliseText.Format("Game.AskFriendsShareText", string.Format("\n{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.AskFriends), Constants.HashTagSocials)), screenshot);
#endif

        Destroy(screenshot);
    }

    void AskFriendsReward()
    {
        AskFriendsHelper.BonusCoins();
    }

    void HandleOnShareCallback(SA.Common.Models.Result result, string data)
    {
        IOSSocialManager.OnMediaSharePostResult -= HandleOnShareCallback;

        if (result.Error != null)
        {
            return;
        }

        if (!Debug.isDebugBuild)
        {
            Fabric.Answers.Answers.LogShare(data, contentId: "AskFriends");
            Fabric.Answers.Answers.LogCustom("AskFriends");
            Flurry.Flurry.Instance.LogEvent("AskFriends", new Dictionary<string, string>() { { "Share", data } });
        }

        AskFriendsReward();
    }

    void HandleOnShareIntentCallback(bool status, string package)
    {
        AndroidSocialGate.OnShareIntentCallback -= HandleOnShareIntentCallback;

        if (!Debug.isDebugBuild)
        {
            Fabric.Answers.Answers.LogShare(package, contentId: "AskFriends");
            Fabric.Answers.Answers.LogCustom("AskFriends");
            Flurry.Flurry.Instance.LogEvent("AskFriends", new Dictionary<string, string>() { { "Share", package } });
        }

        AskFriendsReward();
    }

    public void Leaderboard()
    {
        DialogInstance dialog = DialogManager.Instance.Show("LeaderBoard");
        LeaderboardController _leaderboard = dialog.GetComponent<LeaderboardController>();
        _leaderboard.Info();
    }

    void ShowRewardShareButton()
    {
        if (!(_gamesPlayed == 1 || _gamesPlayed % 3 == 0))
        {
            return;
        }

        _rewardShareButton.transform.DOScale(Vector3.zero, 0f).SetEase(Ease.Linear);
        _rewardShareButton.SetActive(true);

        _rewardShareButton.transform.DOScale(new Vector3(1f, 1f, 1f), 0.25f).SetEase(Ease.Linear);
    }

    public void RewardShare()
    {
#if UNITY_EDITOR
        RewardsShareHelper.RewardShareCoins();
        _rewardShareButton.SetActive(false);
        return;
#endif

        FacebookRequests.Instance.FeedShare(Constants.ShareURLLink(Constants.ShareCodes.FacebookFeed),
                                            LocaliseText.Get("GameName"),
                                            string.Format("{0} #{1}", Constants.ShareURLLink(Constants.ShareCodes.FacebookFeed), Constants.HashTagSocials),
                                            (FacebookShareLinkMessage.ResultType result) =>
        {
            if (result == FacebookShareLinkMessage.ResultType.OK)
            {
                RewardsShareHelper.RewardShareCoins();
                _rewardShareButton.SetActive(false);

                if (!Debug.isDebugBuild)
                {
                    Flurry.Flurry.Instance.LogEvent("Share_Facebook_Feed");
                    Fabric.Answers.Answers.LogCustom("Share_Facebook_Feed");
                }
            }
        });
    }

    public void ShuffleCharacters()
    {
        shuffleButton.GetComponent<Button>().enabled = false;
        EnableDragging(false);

        foreach (GameObject _char in _chars)
        {
            _char.transform.DOMove(_table.transform.position, 0.25f).SetEase(Ease.Linear);
            _char.transform.DOScale(new Vector3(2.0f, 2.0f, 2.0f), 0.25f).SetEase(Ease.Linear);
            _char.transform.DOLocalRotate(new Vector3(0, 0, 360.0f), 0.25f, RotateMode.LocalAxisAdd).SetLoops(3).SetEase(Ease.Linear);
        }

        StartCoroutine(CoRoutines.DelayedCallback(0.6f, PutBackShuffleCharacters));
    }

    void PutBackShuffleCharacters()
    {
        int index = 0;
        int angle = 360 / _chars.Count;

        GameObject[] _characters = _chars.ToArray();

        ArrayUtils.Shuffle(_characters);

        foreach (GameObject _char in _characters)
        {
            Vector3 pos = Vector3Utils.RandomCircle(_table.transform.position, 1.2f, angle, index);

            Animator anim = GameObjectHelper.GetChildComponentOnNamedGameObject<Animator>(_char, "Smoke", true);

            _char.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.25f + debugAnimationTime).SetEase(Ease.Linear);
            _char.transform.DOMove(pos, 0.25f + debugAnimationTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                anim.SetTrigger("Smoke");
            });

            index++;
        }

        StartCoroutine(CoRoutines.DelayedCallback(0.25f, () =>
        {
            ButtonUtils.PlayGameBombSound();
            shuffleButton.GetComponent<Button>().enabled = true;
            EnableDragging(true);
        }));
    }

    #region SingleWordTable

    bool CanFitInContainer(string word, int index)
    {
        GameObject _container = _wordsCointainersItems[index];

        if (word.Length != _container.transform.childCount)
        {
            return false;
        }

        int i = 0;
        int common_chars = 0;
        foreach (Transform _child in _container.transform)
        {
            CharTable _charTable = _child.gameObject.GetComponent<CharTable>();

            if (_charTable.found)
            {
                GameObject textObject = GameObjectHelper.GetChildNamedGameObject(_child.gameObject, "Text", true);
                Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(textObject, "Text", true);

                string _entryChar = text.text.ToLower();

                char _char = word[i];

                // this character fit in this box
                if (_char.ToString().ToLower() == _entryChar)
                {
                    common_chars++;
                }
            }
            else
            {
                common_chars++;
            }

            i++;
        }

        return (common_chars == word.Length);
    }

    void PrepareMultiWordTable()
    {
        if (_scrollSnap != null)
        {
            _scrollSnap.PrevButton.SetActive(false);
            _scrollSnap.NextButton.SetActive(false);
            _scrollSnap = null;
        }
    }

    void PrepareSingleWordTable()
    {
        _scrollSnap = GameObject.Find("ContainerSingleWord").GetComponent<HorizontalScrollSnap>();
        _scrollSnap.CurrentPage = 0;
        _scrollSnap.UpdateLayout();

        PageChanged();
    }

    void ShowNextWord(float delay)
    {
        if (!(indexOfSearchingWord + 1 < _gameWords.Length))
        {
            return;
        }

        indexOfSearchingWord += 1;

        StartCoroutine(CoRoutines.DelayedCallback(delay, () =>
        {
            _scrollSnap.NextScreen();
        }));
    }

    bool ShouldSetAsSingleWordTable()
    {
        return LocaliseText.Language == "Russian";
    }

    void ScrollToFirstUnfoundWord()
    {
        int index = 0;
        foreach (string _w in _allWords)
        {
            if (_foundWords.Exists(e => e.Equals(_w)))
            {
                index++;
                continue;
            }
            break;
        }

        if (index > 0)
        {
            indexOfSearchingWord = index;

            _scrollSnap.CurrentPage = index;
            _scrollSnap.UpdateLayout();

            PageChanged();
        }
    }

    void ShowWordsPageNumber()
    {
        _pageText.text = string.Format("{0} / {1}", _scrollSnap.CurrentPage + 1, _allWords.Count);
    }

    void PageChanged()
    {
        ShowWordsPageNumber();

        if (_scrollSnap.CurrentPage == indexOfSearchingWord)
        {
            _scrollSnap.NextButton.SetActive(false);
        }
        else
        {
            _scrollSnap.NextButton.SetActive(true);
        }

        if (_scrollSnap.CurrentPage == 0)
        {
            _scrollSnap.PrevButton.SetActive(false);
        }
        else
        {
            _scrollSnap.PrevButton.SetActive(true);
        }
    }

    public void OnPageChanged(HorizontalScrollSnap scrollSnap)
    {
        PageChanged();
    }

    #endregion
}
