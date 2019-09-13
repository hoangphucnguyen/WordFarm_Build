using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Helper;
using GameFramework.Localisation;
using GameFramework.UI.Other;
using GameFramework.GameObjects;
using GameFramework.Preferences;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameStructure;
using GameFramework.Debugging;
using GameFramework.GameStructure.Levels.ObjectModel;

public class AwardsController : MonoBehaviour {
	private JSONObject _awards = null;

	void Start() {
		if (!Debug.isDebugBuild) {
			Fabric.Answers.Answers.LogContentView ("Awards", "Dialog");
		}
	}

	public void SetAward(JSONObject awards) {
		_awards = awards;

		PopulateData ();
	}

	// {"noAds":true}
	// {"unlockedLevels":true}
	void PopulateData() {
		string text = null;
		Sprite sprite = null;

		if ( _awards.ContainsKey ("noAds") ) {
			text = LocaliseText.Get ("Awards.NoAds");
		}

		if ( _awards.ContainsKey ("unlockedLevels") ) {
			text = LocaliseText.Get ("Awards.UnlockedLevels");
		}

		if ( _awards.ContainsKey ("noAds") && _awards.ContainsKey ("unlockedLevels") ) {
			text = LocaliseText.Get ("Awards.NoAdsANDUnlockedLevels");
		}

		if (_awards.ContainsKey ("coins")) {
			text = LocaliseText.Format ("Awards.Coins", (int)_awards.GetNumber ("coins"));
			sprite = Resources.Load <Sprite>("Images/coin-icon");
		}

        if (_awards.ContainsKey("unlockLevel")) {
            text = string.Format("Unlock level with number {0}", (int)_awards.GetNumber("unlockLevel"));
        }

        if (_awards.ContainsKey("unlockPack"))
        {
            text = "Unlock pack";
        }

        if (_awards.ContainsKey("unlockAll"))
        {
            text = "Unlock entire game";
        }

		GameObject GO = gameObject.GetComponent <DialogInstance> ().Content;
		GameObject childGameObject = null;

		if (text != null)
			UIHelper.SetTextOnChildGameObject(GO, "ph_Text", text, true);

		childGameObject = GameObjectHelper.GetChildNamedGameObject(GO, "ph_Text", true);
		if (childGameObject != null)
			childGameObject.SetActive(text != null);

		if (sprite != null)
			UIHelper.SetSpriteOnChildGameObject(GO, "ph_Image", sprite, true);

		childGameObject = GameObjectHelper.GetChildNamedGameObject(GO, "ph_Image", true);
		if (childGameObject != null)
			childGameObject.SetActive(sprite != null);
	}

	public void ClaimAwards() {
//		if ( _awards.ContainsKey ("unlockedLevels") ) {
//			bool unlocked = LevelController.Instance.UnlockNextPack ();
//
//			if ( unlocked ) {
//				MyDebug.Log ("Award: Unlocked levels: Unlock next pack");
//			}
//
//			if ( !unlocked ) {
//				LevelController.GenerateMoreLevels ();
//
//				int levels = LevelController.TotalLevels ();
//				LevelController.UnlockLevel ((levels - LevelController.levelsPerPage) + 1);
//
//				MyDebug.Log ("Award: Unlocked levels: Unlock additional levels: " + levels);
//			}
//		}

		if ( _awards.ContainsKey ("noAds") ) {
			PreferencesFactory.SetInt (Constants.KeyNoAds, 1);

			MyDebug.Log ("Award: No Ads: " + PreferencesFactory.GetInt (Constants.KeyNoAds));
		}

		if (_awards.ContainsKey ("coins")) {
			GameManager.Instance.Player.AddCoins ((int)_awards.GetNumber ("coins"));

			MyDebug.Log ("Award: Coins: " + (int)_awards.GetNumber ("coins"));
		}

        if (_awards.ContainsKey("unlockLevel"))
        {
            Level level = GameManager.Instance.Levels.GetItem((int)_awards.GetNumber("unlockLevel"));

            if ( level != null ) {
                level.IsUnlocked = true;
                level.UpdatePlayerPrefs();

                Pack pack = LevelController.Instance.PackForLevel(level);

                if (pack != null)
                {
                    LevelController.Instance.UnlockPack(pack);
                }

                GameManager.Instance.Levels.Selected = level;
            }
        }

        if (_awards.ContainsKey("unlockPack"))
        {
            Pack pack = ((CustomGameManager)CustomGameManager.Instance).Packs.GetItem((int)_awards.GetNumber("unlockPack"));

            if ( pack != null ) {
                LevelController.Instance.UnlockPack(pack);
            }
        }

        if (_awards.ContainsKey("unlockAll"))
        {
            LevelController.Instance.UnlockAll();
        }

		GameManager.Instance.Player.UpdatePlayerPrefs();
		PreferencesFactory.Save ();

		_awards = null;

		DialogInstance dialogInstance = gameObject.GetComponent <DialogInstance> ();
		dialogInstance.Done (); 
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClaimAwards();
        }
    }
}
