using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameObjects;
using UnityEngine.UI;
using GameFramework.Preferences;
using GameFramework.GameStructure;
using GameFramework.Facebook.Messages;
using GameFramework.Messaging;
using GameFramework.Display.Other;

public class SignInDialog : MonoBehaviour, UserHandler.IUserActionsCallback {

	void Start() {
		if (!Debug.isDebugBuild) {
			Fabric.Answers.Answers.LogContentView ("SignIn", "Dialog");
		}

		GameManager.SafeAddListener<FacebookLoginMessage>(FacebookLoginHandler);
	}

	public void Close() {
		GameObject closeButton = GameObjectHelper.GetChildNamedGameObject (gameObject, "Close", true);
		closeButton.GetComponent <Animator> ().SetTrigger ("Pressed");
        closeButton.GetComponent<Button>().onClick.Invoke();
	}

	public void SignUp() {
		if ( PlayerPrefs.HasKey (Constants.KeyRedirectAfterSignIn) ) {
			PlayerPrefs.SetInt (Constants.KeyRedirectAfterSignUp, PlayerPrefs.GetInt (Constants.KeyRedirectAfterSignIn));
		}

		Close ();

		StartCoroutine (CoRoutines.DelayedCallback (Constants.DelayButtonClickAction, () => {
			SettingsContainer.Instance.SignUp ();
		}));
	}

	public bool FacebookLoginHandler(BaseMessage message)
	{
		var facebookLoginMessage = message as FacebookLoginMessage;

		if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.OK) {
			DialogInstance dialog = gameObject.GetComponent <DialogInstance> ();
			dialog.DoneOk ();
		}

		return true;
	}

	void OnDestroy() {
		GameManager.SafeRemoveListener<FacebookLoginMessage> (FacebookLoginHandler);

		PlayerPrefs.DeleteKey (Constants.KeyRedirectAfterSignIn);
		UserHandler.Instance.SetCallback (null);
	}

	public void Save() {
		DialogInstance dialogInstance = GetComponent <DialogInstance>();

		UserHandler.Instance.SetCallback (this);
		UserHandler.Instance.LoginAction (dialogInstance.Content);
	}

	public void FacebookConnect() {
		SettingsContainer.Instance.FacebookLogin ();
	}

	public void ForgotPassword() {
		DialogInstance dialogInstance = GetComponent <DialogInstance>();

		GameObject email = GameObjectHelper.GetChildNamedGameObject (dialogInstance.Content, "EmailField", true);

		DialogInstance dialog = DialogManager.Instance.Show ("ForgotPassDialog");

		GameObject femail = GameObjectHelper.GetChildNamedGameObject (dialog.Content, "EmailField", true);

		femail.GetComponent <InputField>().text = email.GetComponent <InputField>().text;
	}

	public void RestorePassword() {
		DialogInstance dialogInstance = GameObject.Find ("ForgotPassDialog(Clone)").GetComponent <DialogInstance> ();
		UserHandler.Instance.ForgotPassAction (dialogInstance.Content, () => {
			Close ();
		});
	}

	public void OnUserLoggedIn () {
		if ( PlayerPrefs.HasKey (Constants.KeyRedirectAfterSignIn) ) {
			int redirect = PlayerPrefs.GetInt (Constants.KeyRedirectAfterSignIn);
			SettingsContainer.Instance.Redirect (redirect);
		}

		Close ();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

	public void OnUserRegistered () {}
}
