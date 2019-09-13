using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.Levels.Components;
using GameFramework.GameStructure.GameItems.ObjectModel;
using UnityEngine.UI;
using GameFramework.UI.Buttons.Components;
using GameFramework.GameObjects;
using UnityEngine.UI.Extensions;
using GameFramework.GameStructure;
using GameFramework.Messaging;
using GameFramework.Localisation;
using GameFramework.GameStructure.Levels.ObjectModel;
using GameFramework.Helper;

public class CreateCustomLevelButtons : CreateLevelButtons {
	[HideInInspector]
	public List<GameObject> buttons;

	[SerializeField]
	private GameObject _charPrefab;

	public new void Awake()
	{
		buttons = new List<GameObject> ();

		GameManager.SafeAddListener<LevelUnlockedMessage> (LevelUnlockedHandler);
	}

	void OnDestroy() {
		GameManager.SafeRemoveListener<LevelUnlockedMessage> (LevelUnlockedHandler);
	}

	bool LevelUnlockedHandler(BaseMessage message) {
		LevelUnlockedMessage m = message as LevelUnlockedMessage;

		RefreshButton (m.level);

		return true;
	}

	public void RefreshButton(int index) {
		GameObject b = buttons [index-1];
		b.GetComponent<CustomLevelButton> ().Unlock ();
	}

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        foreach (GameObject child in buttons)
		{
			Destroy(child);
		}

        buttons.Clear();
    }

    public void Refresh() {
		Pack pack = ((CustomGameManager)CustomGameManager.Instance).Packs.Selected;

		ItemsRange range = LevelController.PackLevelsRange (pack);

		for ( int i = range.From; i < range.To+1; i++) 
		{
			Level level = GameManager.Instance.Levels.GetItem (i);
			level.LoadData ();

			GameObject newObject = Instantiate(Prefab);
			newObject.transform.SetParent(transform, false);

            CustomLevelButton levelButton = newObject.GetComponent<CustomLevelButton>();
            levelButton.Context.Number = i;

			if ( level.IsUnlocked ) {
				CanvasGroup canvasGroup = GameObjectHelper.GetChildComponentOnNamedGameObject<CanvasGroup> (newObject, "PlayButton", true);
				canvasGroup.alpha = 1f;
			}

			Level nextLevel = GameManager.Instance.Levels.GetItem(i + 1);

			if (nextLevel != null && nextLevel.IsUnlocked == false && level.IsUnlocked)
			{
				Animator anim = GameObjectHelper.GetChildComponentOnNamedGameObject<Animator>(newObject, "PlayButton", true);
                anim.enabled = true;
			}

            if ( level.ProgressBest > 0.9f ) {
                Text statusText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(newObject, "Status", true);
                statusText.text = LocaliseText.Get("Text.Done");
            }

			Text text = GameObjectHelper.GetChildComponentOnNamedGameObject <Text> (newObject, "Name", true);
            text.text = LocaliseText.Format("Text.LevelNumber", i);

			GameObject previewObject = GameObjectHelper.GetChildNamedGameObject (newObject, "Preview", true);
			GameObject wordsObject = GameObjectHelper.GetChildNamedGameObject (previewObject, "Words", true);

            string preview = level.JsonData.GetString("preview");
			char[] characters = preview.ToCharArray ();

			int index = 0;
			int angle = 360 / characters.Length;

			foreach (char _char in characters) {
				GameObject _charObject = Instantiate (_charPrefab, wordsObject.transform);

				Vector3 pos = Vector3Utils.RandomCircle(wordsObject.transform.position, 0.2f, angle, index);
				_charObject.transform.position = pos;
				_charObject.transform.localScale = new Vector3 (0.7f, 0.7f, 0.7f);

				GameObject _text = GameObjectHelper.GetChildNamedGameObject (_charObject, "Text", true);
				_text.GetComponent <Text> ().text = _char.ToString ().ToUpper ();

				index++;
			}

			buttons.Add (newObject);
		}
	}
}
