using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api.Requests;
using GameSparks.Core;
using GameFramework.Preferences;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameObjects.Components;
using GameFramework.Facebook.Components;
using GameFramework.GameStructure;
using GameFramework.Localisation;
using GameFramework.Debugging;

public class UserHandler : Singleton <UserHandler>
{
	private IUserActionsCallback _callback;

	protected override void GameSetup ()
	{
		 
	}

	private static readonly Regex MailValidator = new Regex (
		                                              @"^((([A-Za-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$");

	public static bool IsMailValid (string mail)
	{
		return MailValidator.IsMatch (mail);
	}

	public static bool IsUsernameValid (string username)
	{
		return !string.IsNullOrEmpty (username) && username.Length > 3 && username.Length < 15;
	}

	public interface IUserActionsCallback
	{
		void OnUserLoggedIn ();

		void OnUserRegistered ();
	}

	public void SetCallback (IUserActionsCallback callback)
	{
		_callback = callback;
	}

	public void ForgotPassAction (GameObject GO, Action callback)
	{
		var mainTransform = GO.transform;
		var email = mainTransform.Find ("EmailField").GetComponent<InputField> ();

		var emailString = email.text;

		if (string.IsNullOrEmpty (emailString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.EmailEmptyError"), "CLOSE");
			return;
		}
		if (!IsMailValid (emailString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.EmailNotValidError"), "CLOSE");
			return;
		}

		if ( !Reachability.Instance.IsReachable () ) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.NoConnection"), "CLOSE");
			return;
		}

		if ( !GS.Available ) {
			GS.Reconnect ();
		}

		GSRequestData d = new GSRequestData ();
		d.Add ("email", emailString);
		d.Add ("action", "passwordRecoveryRequest");

		Loading.Instance.Show();

		new AuthenticationRequest ()
			.SetUserName ("")
			.SetPassword ("")
			.SetScriptData (d)
			.Send (((response) => {
				Loading.Instance.Hide();

				if (response.HasErrors) {
					ShowInfoDialogWithText (LocaliseText.Get ("Account.ForgotPassEmailSuccess"), "CLOSE");

					if ( callback != null ) {
						callback();
					}
				}
			}));
	}

	public void LoginAction (GameObject GO)
	{
		var mainTransform = GO.transform;
		var email = mainTransform.Find ("EmailField").GetComponent<InputField> ();
		var password = mainTransform.Find ("PasswordField").GetComponent<InputField> ();

		var emailString = email.text;
		var passwordString = password.text;
		if (string.IsNullOrEmpty (emailString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.EmailEmptyError"), "CLOSE");
			return;
		}
		if (!IsMailValid (emailString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.EmailNotValidError"), "CLOSE");
			return;
		}
		if (string.IsNullOrEmpty (passwordString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.PasswordEmptyError"), "CLOSE");
			return;
		}

		if ( !Reachability.Instance.IsReachable () ) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.NoConnection"), "CLOSE");
			return;
		}

		if ( !GS.Available ) {
			GS.Reconnect ();
		}

        Loading.Instance.Show();

		new AuthenticationRequest ()
            .SetUserName (emailString)
            .SetPassword (passwordString)
            .Send (((response) => {
            Loading.Instance.Hide();

			if (!response.HasErrors) {
				email.text = null;
				password.text = null;

				var scriptData = response.ScriptData;
				if (scriptData != null) {
					var avatar = scriptData.GetString ("avatar");
					
					if ( avatar != null ) {
						PreferencesFactory.SetString (Constants.ProfileAvatar, avatar);
					}
				}
				
				var username = response.DisplayName;
				PreferencesFactory.SetString (Constants.ProfileUsername, username);
				PreferencesFactory.SetString (Constants.ProfileUserId, response.UserId);
				PreferencesFactory.SetString (Constants.ProfileEmail, emailString);
				PreferencesFactory.Save ();
				
				if (_callback != null) {
					_callback.OnUserLoggedIn ();
				}

				GameManager.SafeQueueMessage (new UserLoginMessage());

				if (!Debug.isDebugBuild) {
					Fabric.Answers.Answers.LogLogin ("Email");
                    Branch.userCompletedAction("Login");
                    Branch.setIdentity(PreferencesFactory.GetString(Constants.ProfileUserId));
				}
			} else {
				ParseServerResponse (response.Errors);
			}
		}));
	}

	public void RegisterAction (GameObject GO)
	{
		var mainTransform = GO.transform;
		var email = mainTransform.Find ("EmailField").GetComponent<InputField> ();
		var username = mainTransform.Find ("UsernameField").GetComponent<InputField> ();
		var password = mainTransform.Find ("PasswordField").GetComponent<InputField> ();
		var repearPassword = mainTransform.Find ("RepeatPasswordField").GetComponent<InputField> ();

		var emailString = email.text;
		var usernameString = username.text;
		var passwordString = password.text;
		var repeatString = repearPassword.text;

		if (string.IsNullOrEmpty (emailString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.EmailEmptyError"), "OK");
			return;
		}
		if (!IsMailValid (emailString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.EmailNotValidError"), "OK");
			return;
		}
		if (string.IsNullOrEmpty (usernameString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.UsernameEmptyError"), "OK");
			return;
		}
		if (!IsUsernameValid (usernameString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.UsernameNotValid"), "OK");
			return;
		}
		if (string.IsNullOrEmpty (passwordString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.PasswordEmptyError"), "OK");
			return;
		}
		if (string.IsNullOrEmpty (repeatString) || !repeatString.Equals (passwordString)) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.PasswordsNotEqual"), "OK");
			return;
		}

		if ( !Reachability.Instance.IsReachable () ) {
			ShowInfoDialogWithText (LocaliseText.Get ("Account.NoConnection"), "CLOSE");
			return;
		}

		if ( !GS.Available ) {
			GS.Reconnect ();
		}

		Loading.Instance.Show();

		// user was logged from DeviceAuthenticationRequest and now only update his data
		if ( PreferencesFactory.HasKey(Constants.ProfileAnonymousUserId) ) {
			new ChangeUserDetailsRequest()
			.SetDisplayName(usernameString)
			.SetUserName(emailString)
			.SetNewPassword(passwordString)
			.Send((response) => {
                Loading.Instance.Hide();

				if (response.HasErrors)
				{
					ParseServerResponse(response.Errors);
					return;
				}

				email.text = null;
				username.text = null;
				password.text = null;
				repearPassword.text = null;

				PreferencesFactory.SetString(Constants.ProfileEmail, emailString);
				PreferencesFactory.SetString(Constants.ProfileUsername, usernameString);
                PreferencesFactory.SetString(Constants.ProfileUserId, PreferencesFactory.GetString(Constants.ProfileAnonymousUserId));
                PreferencesFactory.DeleteKey(Constants.ProfileAnonymousUserId);
				PreferencesFactory.Save();

				if (_callback != null)
				{
					_callback.OnUserRegistered();
				}
				GameManager.SafeQueueMessage(new UserRegisterMessage());

				if (!Debug.isDebugBuild)
				{
					Flurry.Flurry.Instance.LogEvent("Register_Email");
					Fabric.Answers.Answers.LogSignUp("Email");
                    Branch.userCompletedAction("Register");
                    Branch.setIdentity(PreferencesFactory.GetString(Constants.ProfileUserId));
				}
			});

            return;
        }

		new RegistrationRequest ()
			.SetUserName (emailString)
			.SetDisplayName (usernameString)
			.SetPassword (passwordString)
			.Send (((response) => {
			Loading.Instance.Hide();

			if (!response.HasErrors) {
				email.text = null;
				username.text = null;
				password.text = null;
				repearPassword.text = null;

				var scriptData = response.ScriptData;
				if (scriptData != null ) {
					var avatar = scriptData.GetString ("avatar");

					if ( avatar != null ) {
						PreferencesFactory.SetString (Constants.ProfileAvatar, avatar);
					}
				}

				PreferencesFactory.SetString (Constants.ProfileEmail, emailString);
				PreferencesFactory.SetString (Constants.ProfileUsername, response.DisplayName);
				PreferencesFactory.SetString (Constants.ProfileUserId, response.UserId);
				PreferencesFactory.Save ();

				if (_callback != null) {
					_callback.OnUserRegistered ();
				}
				GameManager.SafeQueueMessage (new UserRegisterMessage());

				if (!Debug.isDebugBuild) {
					Flurry.Flurry.Instance.LogEvent ("Register_Email");
					Fabric.Answers.Answers.LogSignUp ("Email");
                    Branch.userCompletedAction("Register");
                    Branch.setIdentity(PreferencesFactory.GetString(Constants.ProfileUserId));
				}
			} else {
				ParseServerResponse (response.Errors);
			}
		}));
	}

	public void ConnectedWithFacebook ()
	{
		if (FacebookManager.Instance.IsLoggedIn) {
			if ( !GS.Available ) {
				GS.Reconnect ();
			}

			new FacebookConnectRequest ()
                .SetAccessToken (AccessToken.CurrentAccessToken.TokenString)
                .SetDoNotLinkToCurrentPlayer (false)
                .SetSwitchIfPossible (true)
                .Send ((response) => {
				if (!response.HasErrors) {

					var scriptData = response.ScriptData;
					if (scriptData != null) {
						var avatar = scriptData.GetString ("avatar");

						if ( avatar != null ) {
							PreferencesFactory.SetString (Constants.ProfileAvatar, avatar);
						}
					}

					var displayName = response.DisplayName;
					var userId = response.UserId;
					
					PreferencesFactory.SetString (Constants.ProfileUsername, displayName);
					PreferencesFactory.SetString (Constants.ProfileUserId, response.UserId);
                    PreferencesFactory.SetString (Constants.ProfileFBUserId, AccessToken.CurrentAccessToken.UserId);
					PreferencesFactory.Save ();

					if (_callback != null) {
						_callback.OnUserLoggedIn ();
					}
					
					if ( response.NewPlayer == true ) {
						GameManager.SafeQueueMessage (new UserRegisterMessage());

						if (!Debug.isDebugBuild) {
							Fabric.Answers.Answers.LogSignUp ("Facebook");
                            Branch.userCompletedAction("RegisterFacebook");
                            Branch.setIdentity(PreferencesFactory.GetString(Constants.ProfileUserId));
						}
					} else {
						GameManager.SafeQueueMessage (new UserLoginMessage());

						if (!Debug.isDebugBuild) {
							Fabric.Answers.Answers.LogLogin ("Facebook");
                            Branch.userCompletedAction("LoginFacebook");
                            Branch.setIdentity(PreferencesFactory.GetString(Constants.ProfileUserId));
						}
					}
					
					if (!Debug.isDebugBuild) {
						Flurry.Flurry.Instance.LogEvent ("Register_Facebook");
					}
				} else {
					MyDebug.Log (response.Errors.JSON.ToString ());
					ParseServerResponse (response.Errors);
				}
			});
		}
	}

	public void ChangeUser(string userName) {
		if (string.IsNullOrEmpty (userName)) {
			DialogManager.Instance.ShowError (LocaliseText.Get ("Account.UsernameEmptyError"));
			return;
		}

		if (!IsUsernameValid (userName)) {
			DialogManager.Instance.ShowError (LocaliseText.Get ("Account.UsernameNotValid"));
			return;
		}

		new ChangeUserDetailsRequest ()
			.SetDisplayName (userName)
			.Send((response) => {
			if ( response.HasErrors ) {
				ParseServerResponse (response.Errors);
				return;
			}

			PreferencesFactory.SetString (Constants.ProfileUsername, userName); 
		});
	}

	public void Logout() {
		DialogManager.Instance.Show (prefabName: "ConfirmDialog", 
			title: LocaliseText.Get ("Text.Logout"), 
			text: LocaliseText.Get ("Text.Sure"), 
			doneCallback: LogoutCallback);
	}

	void LogoutCallback(DialogInstance dialogInstance) {
		if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok) {
			LogoutProcess ();
		}
	}

	public void LogoutProcess() {
		if (PreferencesFactory.IsFlagSet (Constants.KeyFacebookConnected)) {
			PreferencesFactory.ClearFlag (Constants.KeyFacebookConnected);
			FacebookManager.Instance.Logout ();
		}

		GameSparksManager.Instance.Logout ();
        Branch.logout();

		//

		PreferencesFactory.DeleteAll ();

		CustomGameManager manager = CustomGameManager.Instance as CustomGameManager;
		manager.ResetGame();

		// reload data from sync
		LevelController.Instance.Reload ();

		GameManager.LoadSceneWithTransitions("Menu");
	}

	private void ShowInfoDialogWithText (string message, string buttonTitle)
	{
		DialogManager.Instance.Show (prefabName: "GeneralMessageOkButton", 
			title: LocaliseText.Get ("Text.Info"), 
			text: message, 
			dialogButtons: DialogInstance.DialogButtonsType.Ok);
	}

	protected virtual void ParseServerResponse (GSData errors)
	{
		string message = null;
		
		foreach (var entry in errors.BaseData) {
			switch (entry.Key) {
			case "UNRECOGNISED":
				message = LocaliseText.Get ("Account.UserNotExist");
				break;
			case "LOCKED":
				message = LocaliseText.Get ("Account.LoginAttempt");
				break;
			case "USERNAME":
				message = LocaliseText.Get ("Account.EmailNotFree");
				break;
			case "DETAILS":
				message = LocaliseText.Get ("Account.WrongData");
				break;
			case "ALREADY_TAKEN":
				message = (string)entry.Value;
				break;
			case "error":
				switch(entry.Value.ToString ()) {
					case "DISPLAYNAME":
						message = LocaliseText.Get ("Account.UsernameTakenError");
						break;
					case "EMAIL":
						message = LocaliseText.Get ("Account.EmailNotFree");
						break;
				}
				break;
			}
		}
		if (!string.IsNullOrEmpty (message)) {
			ShowInfoDialogWithText (message, "CLOSE");
		}
	}
}