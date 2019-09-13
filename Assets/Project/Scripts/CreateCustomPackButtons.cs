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

public class CreateCustomPackButtons : CreatePackButtons {
	
	[HideInInspector]
	public List<GameObject> buttons;

    [SerializeField]
    private GameObject pagePrefab;

	public new void Awake()
	{
		buttons = new List<GameObject> ();
		Refresh ();
	}

	void OnDestroy() {

	}

	public void RefreshButton(int index) {
		GameObject b = buttons [index-1];
		b.GetComponent<PackButton> ().Unlock ();
	}

	public void Refresh() {
		RankGameItemManager ranksManager = LevelController.Ranks ();

        for (int r = ranksManager.Items.Length; r > 0; r--)
        {
            Rank rank = ranksManager.GetItem(r);
            rank.LoadData();

            GameObject pageObject = Instantiate(pagePrefab);
            pageObject.SetActive(false);
            pageObject.transform.SetParent(transform, false);

            Text _rankSignText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(pageObject, "RankName", true);
            _rankSignText.text = LocaliseText.Get(rank.JsonData.GetString("name"));

            ItemsRange range = LevelController.RankPacksRange(rank);

            for (int i = range.From; i < range.To + 1; i++)
            {
                Pack pack = LevelController.Packs().GetItem(i);

                GameObject newObject = Instantiate(Prefab);
                newObject.GetComponent<PackItem>().Number = i;

                newObject.transform.SetParent(pageObject.transform, false);

                newObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    LevelsListController.Instance.OpenPackButton(pack);
                });

                buttons.Add(newObject);
            }
        }

        CustomUI_ScrollRectOcclusion _occ = GetComponent<CustomUI_ScrollRectOcclusion>();
        _occ.Init();
	}
}
