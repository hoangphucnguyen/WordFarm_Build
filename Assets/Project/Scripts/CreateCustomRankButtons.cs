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

public class CreateCustomRankButtons : CreateRankButtons {

	[HideInInspector]
	public List<GameObject> buttons;

	public new void Awake()
	{
		buttons = new List<GameObject> ();
		Refresh ();
	}

	void OnDestroy() {

	}

	public void RefreshButton(int index) {
		GameObject b = buttons [index-1];
		b.GetComponent<RankButton> ().Unlock ();
	}

	public void Refresh() {
		foreach (Rank rank in ((CustomGameManager)CustomGameManager.Instance).Ranks.Items)
		{
			rank.LoadData ();

			GameObject newObject = Instantiate(Prefab);
			newObject.transform.SetParent(transform, false);

			newObject.GetComponent <Button>().onClick.AddListener (() => {
				LevelsListController.Instance.OpenRankButton (rank);
			});

			if ( rank.IsUnlocked ) {
				CanvasGroup canvasGroup = GameObjectHelper.GetChildComponentOnNamedGameObject<CanvasGroup> (newObject, "PlayButton", true);
				canvasGroup.alpha = 1f;
			}

			Text text = GameObjectHelper.GetChildComponentOnNamedGameObject <Text> (newObject, "Name", true);
			text.text = LocaliseText.Get(rank.JsonData.GetString("name"));

			buttons.Add (newObject);
		}
	}
}
