using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects;
using DG.Tweening;
using UnityEngine.UI;

public class RemoveCoinsAnimated : MonoBehaviour {
	[SerializeField]
	GameObject _bonusCoins;

	[SerializeField]
	GameObject _coinImage;

	private bool _animating = false;

	public void AnimateCoinsRemoving(int coins, Vector3 position = default(Vector3)) {
		if ( _animating == true ) {
			return;
		}

		_animating = true;

		_bonusCoins.transform.localPosition = new Vector3(position.x, position.y, 1000);
		_bonusCoins.transform.localScale = new Vector3 (1, 1, 1);

		GameObject bonusCoinsText = GameObjectHelper.GetChildNamedGameObject (_bonusCoins, "Coin", true);
		bonusCoinsText.GetComponent <Text>().text = string.Format ("- {0}", coins);

		Vector3 pos = new Vector3(0, 0, 0);

		if ( _coinImage!= null ) {
			pos = _coinImage.transform.position;
		}

		Vector2 o = RectTransformUtility.WorldToScreenPoint (Camera.main, pos);

		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (_bonusCoins.transform.parent.transform as RectTransform, 
			o, 
			null, 
			out localPoint);

		Animator anim = _bonusCoins.GetComponent <Animator> ();

		if ( anim != null ) {
			_bonusCoins.transform.localPosition = new Vector3 (localPoint.x, localPoint.y, 0);
			GameObjectHelper.SafeSetActive (_bonusCoins, true);

			_bonusCoins.transform.DOLocalMoveY (localPoint.y+50, 0.45f).SetEase (Ease.Linear);
			_bonusCoins.transform.DOScale (new Vector3(2, 2, 2), 0.45f).SetEase (Ease.Linear);

			_bonusCoins.transform.DOLocalMoveY (localPoint.y-500, 0.25f).SetDelay (0.5f).SetEase (Ease.Linear);
			_bonusCoins.transform.DOScale (new Vector3(0, 0, 0), 0.25f).SetDelay (0.5f).SetEase (Ease.Linear);
		}

		StartCoroutine (BonusCoinsCallback(coins, 1.5f));
	}

	IEnumerator BonusCoinsCallback(int coins, float delay) {
		yield return new WaitForSeconds (delay);

		_animating = false;

		GameObjectHelper.SafeSetActive (_bonusCoins, false);

        Destroy(gameObject);
	}
}
