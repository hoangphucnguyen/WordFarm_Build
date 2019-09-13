using System.Collections;
using System.Collections.Generic;
using GameFramework.GameObjects.Components.AbstractClasses;
using GameFramework.GameStructure;
using GameFramework.Localisation;
using GameFramework.Localisation.Messages;
using GameFramework.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class LanguageIcon : RunOnState {
    [SerializeField]
    private Sprite[] _languageImages;

    public override void Awake()
	{
        base.Awake();

        GameManager.SafeAddListener<LocalisationChangedMessage>(LocalisationHandler);
	}

    private void OnDestroy()
    {
        GameManager.SafeRemoveListener<LocalisationChangedMessage>(LocalisationHandler);
    }

    bool LocalisationHandler(BaseMessage message) {
        Process();

        return true;
    }

	public override void RunMethod()
	{
        Process();
	}

    void Process() {
		for (int i = 0; i < LocaliseText.AllowedLanguages.Length; i++)
		{
			if (LocaliseText.Language == LocaliseText.AllowedLanguages[i])
			{
				Sprite sprite = _languageImages[i];
				gameObject.GetComponent<Image>().sprite = sprite;

				break;
			}
		}
    }
}
