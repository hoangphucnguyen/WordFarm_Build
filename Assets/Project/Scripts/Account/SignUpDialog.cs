using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Preferences;
using GameFramework.GameObjects;
using UnityEngine.UI;
using GameFramework.Display.Other;
using GameFramework.Localisation;

public class SignUpDialog : MonoBehaviour, UserHandler.IUserActionsCallback {

	void Start() {
		if (!Debug.isDebugBuild) {
			Fabric.Answers.Answers.LogContentView ("SignUp", "Dialog");
		}
	}

	public void Close() {
		GameObject closeButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "Close", true);
		closeButton.GetComponent<Animator>().SetTrigger("Pressed");
        closeButton.GetComponent<Button>().onClick.Invoke();
	}

	public void Save() {
		DialogInstance dialogInstance = GetComponent <DialogInstance>();

		UserHandler.Instance.SetCallback (this);
		UserHandler.Instance.RegisterAction (dialogInstance.Content);
	}

	void OnDestroy() {
		PlayerPrefs.DeleteKey (Constants.KeyRedirectAfterSignUp);
		UserHandler.Instance.SetCallback (null);
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

	public void OnUserLoggedIn () {
		
	}

	public void OnUserRegistered () {
		if ( PlayerPrefs.HasKey (Constants.KeyRedirectAfterSignUp) ) {
			int redirect = PlayerPrefs.GetInt (Constants.KeyRedirectAfterSignUp);
			SettingsContainer.Instance.Redirect (redirect);
		}

        DialogManager.Instance.ShowInfo(LocaliseText.Get("Account.SuccessRegistered"));

		Close ();
	}
}
