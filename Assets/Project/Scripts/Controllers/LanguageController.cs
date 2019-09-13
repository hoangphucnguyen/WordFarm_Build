using System.Collections;
using System.Collections.Generic;
using FlipWebApps.BeautifulTransitions.Scripts.Transitions.Components.GameObject;
using GameFramework.GameObjects;
using GameFramework.Localisation;
using GameFramework.Preferences;
using GameFramework.UI.Dialogs.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LanguageController : MonoBehaviour {

    [SerializeField]
    private GameObject _container;
    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private GameObject[] _languageObjects;
    private int _itemsPerPage = 5;
    private int _pages = 0;

	void Start () {
        ChangeLangSelected();

        _pages = Mathf.CeilToInt(_languageObjects.Length / (float)_itemsPerPage);

        int langIndex = 0;
        for (var i = 0; i < LocaliseText.AllowedLanguages.Length; i++)
        {
            if (LocaliseText.AllowedLanguages[i] == LocaliseText.Language)
            {
                langIndex = i;
                break;
            }
        }

        CustomVerticalScrollSnap snap = scrollRect.GetComponent<CustomVerticalScrollSnap>();
        int page = (_pages - 1) - Mathf.CeilToInt(langIndex / _itemsPerPage);

        snap.StartingScreen = page;
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
	
    public void Close() {
        GameObject CloseButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "Close", true);
        Button _button = CloseButton.GetComponent<Button>();

        _button.onClick.Invoke();
    }

    public void ChangeLanguage(int index) {
        if ( index >= _languageObjects.Length ) {
            return;
        }

        LocaliseText.Language = LocaliseText.AllowedLanguages[index];
        PlayerPrefs.SetInt("ManualChangedLanguage", 1);

		if (!Debug.isDebugBuild)
		{
            Flurry.Flurry.Instance.LogEvent("ChangeLanguage",new Dictionary<string, string>() { { "Language", LocaliseText.Language } });
            Fabric.Answers.Answers.LogCustom("ChangeLanguage", new Dictionary<string, object>() { 
                { "Language", LocaliseText.Language }, 
                { "Scene", SceneManager.GetActiveScene().name } 
            });
		}

        ChangeLangSelected();
    }

    public static void ChangeLanguage(string language) {
        for (var i = 0; i < LocaliseText.AllowedLanguages.Length; i++)
        {
            if (LocaliseText.AllowedLanguages[i] == language) {
                LocaliseText.Language = language;

                PreferencesFactory.SetString("Language", language, useSecurePrefs: false);
            }
        }
    }

    void ChangeLangSelected() {
		for (var i = 0; i < LocaliseText.AllowedLanguages.Length; i++)
		{
			GameObject _obj = GameObjectHelper.GetChildNamedGameObject(_languageObjects[i], "Selected", true);

			if (LocaliseText.AllowedLanguages[i] == LocaliseText.Language)
			{
				_obj.SetActive(true);
			}
			else
			{
				_obj.SetActive(false);
			}
		}
    }
}
