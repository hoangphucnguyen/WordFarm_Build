using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameFramework.GameObjects;
using GameFramework.Preferences;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Display.Other;
using GameFramework.Messaging;
using GameFramework.Facebook.Messages;
using GameFramework.GameStructure;
using GameSparks.Core;
using GameFramework.UI.Other;

public class ProfileController : MonoBehaviour {
	private RawImage _avatarImage;
	private InputField _userNameField;

    [SerializeField]
    private GameObject _statsRowPrefab;
    private GameObject _statsContainer;

	void Start () {
        GameManager.SafeAddListener<FacebookProfilePictureMessage>(FacebookProfilePictureHandler);

		_avatarImage = GameObjectHelper.GetChildComponentOnNamedGameObject<RawImage> (gameObject, "AvatarImage");
		_userNameField = GameObjectHelper.GetChildComponentOnNamedGameObject<InputField> (gameObject, "UsernameField", true);

		_userNameField.text = PreferencesFactory.GetString (Constants.ProfileUsername);

        _statsContainer = GameObjectHelper.GetChildNamedGameObject(gameObject, "Stats", true);

		#if UNITY_IOS
		IOSCamera.OnImagePicked += OnImage;
		#endif

		#if UNITY_ANDROID
		AndroidCamera.Instance.OnImagePicked += OnImage;
		#endif

		string avatarId = PreferencesFactory.GetString (Constants.ProfileAvatarUploadId);

		if ( avatarId != null ) {
			GameSparksManager.Instance.DownloadAvatar (avatarId, (Texture2D image) => {
				if ( _avatarImage != null && image != null ) {
					_avatarImage.texture = image;
				}
			});
        } else if ( PreferencesFactory.HasKey(Constants.ProfileFBUserId) ) {
            FacebookRequests.Instance.LoadProfileImages(PreferencesFactory.GetString(Constants.ProfileFBUserId));
        }

		GameSparksManager.Instance.LeaderboardRating ((int rating) => {
			PreferencesFactory.SetInt (Constants.ProfileRating, rating);
			SetRating(rating);
		});

        GameSparksManager.Instance.UserStats((GSData data) => {
            if ( data.GetGSData("Novice") != null ) {
                GSData rank = data.GetGSData("Novice");
                StatsAddRow("Novice", (int)rank.GetInt("wins"), (int)rank.GetInt("lose"), (int)rank.GetInt("draw"));
            }

			if (data.GetGSData("Advanced") != null)
			{
                GSData rank = data.GetGSData("Advanced");
				StatsAddRow("Advanced", (int)rank.GetInt("wins"), (int)rank.GetInt("lose"), (int)rank.GetInt("draw"));
			}

			if (data.GetGSData("Master") != null)
			{
                GSData rank = data.GetGSData("Master");
				StatsAddRow("Master", (int)rank.GetInt("wins"), (int)rank.GetInt("lose"), (int)rank.GetInt("draw"));
			}

			if (data.GetGSData("Professional") != null)
			{
                GSData rank = data.GetGSData("Professional");
				StatsAddRow("Professional", (int)rank.GetInt("wins"), (int)rank.GetInt("lose"), (int)rank.GetInt("draw"));
			}
        });

		SetRating(PreferencesFactory.GetInt (Constants.ProfileRating, 0));
	}

	void OnDestroy() {
        GameManager.SafeRemoveListener<FacebookProfilePictureMessage>(FacebookProfilePictureHandler);

		#if UNITY_IOS
		IOSCamera.OnImagePicked -= OnImage;
		#endif

		#if UNITY_ANDROID
		AndroidCamera.Instance.OnImagePicked -= OnImage;
		#endif
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    void Close() {
        GameObject CloseButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "Close", true);
        Button _button = CloseButton.GetComponent<Button>();

        _button.onClick.Invoke();
    }

	bool FacebookProfilePictureHandler(BaseMessage message)
	{
		FacebookProfilePictureMessage m = message as FacebookProfilePictureMessage;

		if (m.UserId == PreferencesFactory.GetString(Constants.ProfileFBUserId) )
		{
			_avatarImage.texture = m.Texture;
		}

		return true;
	}

    void StatsAddRow(string rank, int wins, int lose, int draw) {
        GameObject _rowObject = Instantiate(_statsRowPrefab, _statsContainer.transform);

        UIHelper.SetTextOnChildGameObject(_rowObject, "Name", rank);
        UIHelper.SetTextOnChildGameObject(_rowObject, "Wins", wins.ToString());
        UIHelper.SetTextOnChildGameObject(_rowObject, "Loses", lose.ToString());
    }

	#if UNITY_EDITOR
	private void PickImageEditor() {
		string image = "http://www.telegraph.co.uk/content/dam/news/2016/09/08/107667228_beech-tree-NEWS-large_trans_NvBQzQNjv4BqplGOf-dgG3z4gg9owgQTXEmhb5tXCQRHAvHRWfzHzHk.jpg";

		GameSparksManager.Instance.DownloadAvatarUrl (image, action: (Texture2D tex) => {
			_avatarImage.texture = tex;

            GameSparksManager.Instance.UploadAvatar (tex);
		});
	}
	#endif

	#if UNITY_IOS
	private void OnImage (IOSImagePickResult result) {
		if(result.IsSucceeded) {
			_avatarImage.texture = result.Image;

			GameSparksManager.Instance.UploadAvatar (result.Image);
		}
	}
	#endif

	#if UNITY_ANDROID
	private void OnImage(AndroidImagePickResult result) {
		if (result.IsSucceeded) {
			_avatarImage.texture = result.Image;

			GameSparksManager.Instance.UploadAvatar (result.Image);
		}	
	}
	#endif

	public void FacebookButton() {
		SettingsContainer.Instance.FacebookLogin ();
	}

	public void AvatarChangeButton() {
		DialogInstance _avatarDialogInstance = DialogManager.Instance.Show ("ProfileChangeAvatarDialog");

		Button galleryButton = GameObjectHelper.GetChildComponentOnNamedGameObject<Button> (_avatarDialogInstance.Content, "GalleryButton");
		Button cameraButton = GameObjectHelper.GetChildComponentOnNamedGameObject<Button> (_avatarDialogInstance.Content, "CameraButton");

		galleryButton.onClick.AddListener (()=> {
			#if UNITY_EDITOR
			PickImageEditor();
			#endif

			#if UNITY_IOS
			IOSCamera.Instance.PickImage(ISN_ImageSource.Library);
            #endif

            #if UNITY_ANDROID
			AndroidCamera.Instance.GetImageFromGallery();
            #endif

            _avatarDialogInstance.Done();
		});

		cameraButton.onClick.AddListener (()=> {
			#if UNITY_EDITOR
			PickImageEditor();
			#endif

			#if UNITY_IOS
			IOSCamera.Instance.PickImage(ISN_ImageSource.Camera);
			#endif

			#if UNITY_ANDROID
			AndroidCamera.Instance.GetImageFromCamera();
			#endif

            _avatarDialogInstance.Done();
		});
	}

	public void LeaderboardButton() {
		DialogManager.Instance.Show ("LeaderBoard");
	}

	void SetRating(int rating) {
		Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text> (gameObject, "Rating", true);

        if ( text == null || rating <= 0 ) {
			return;
		}

		text.text = string.Format ("#{0}", rating);
	}

	public void OnUsernameEnd() {
		string username = _userNameField.text;

		UserHandler.Instance.ChangeUser (username);
	}
}
