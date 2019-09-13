using System.Collections;
using System.Collections.Generic;
using GameFramework.GameObjects;
using GameFramework.Localisation;
using UnityEngine;
using UnityEngine.UI;

public class PackItem : MonoBehaviour {
    public int Number;

    private Text _nameText;
    private Text _statusText;
    private CanvasGroup _canvasGroup;
    private GameObject _icon;
    private GameObject _pointsObject;
    private Text _pointsNumber;

    private void UpdateContent()
    {
		Pack pack = LevelController.Packs().GetItem(Number);
		pack.LoadData();

		if (pack.IsUnlocked)
		{
            if (_canvasGroup == null)
            {
                _canvasGroup = GameObjectHelper.GetChildComponentOnNamedGameObject<CanvasGroup>(gameObject, "PlayButton", true);
            }

			_canvasGroup.alpha = 1f;
		}

        if (_nameText == null)
        {
            _nameText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(gameObject, "Name", true);
        }

		_nameText.text = LocaliseText.Get(pack.JsonData.GetString("name"));

        if (_statusText == null)
        {
            _statusText = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(gameObject, "Status", true);
        }

		_statusText.text = LocaliseText.Format("Text.NumberLetters", pack.JsonData.GetNumber("letters"));

        if ( LocaliseText.Language == "Russian" ) {
            _statusText.gameObject.SetActive(false);
        }

        if (_icon == null)
        {
            _icon = GameObjectHelper.GetChildNamedGameObject(gameObject, "Icon", true);
        }

		if (pack.Progress > 0.9f)
		{
			_icon.SetActive(true);
		}

		int points = LevelController.Instance.PointsForPack(pack);

		if (points > 0)
		{
            if (_pointsObject == null)
            {
                _pointsObject = GameObjectHelper.GetChildNamedGameObject(gameObject, "Points", true);
            }

			_pointsObject.SetActive(true);

            if (_pointsNumber == null)
            {
                _pointsNumber = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(_pointsObject, "PointsNumber", true);
            }

			_pointsNumber.text = points.ToString();
		}
	}

    private void OnEnable()
    {
        if (gameObject.activeInHierarchy && gameObject.transform.parent != null)
        {
            UpdateContent();
        }
    }
}
