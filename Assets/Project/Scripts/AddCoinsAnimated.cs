using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects;
using UnityEngine.UI;
using DG.Tweening;
using GameFramework.GameStructure;

public class AddCoinsAnimated : MonoBehaviour {
	[SerializeField]
	GameObject bonusCoins;

	[SerializeField]
	GameObject coinImage;
	
    public void AnimateCoinsAdding(int coins, Vector3 position = default(Vector3), RectTransform rect = null, bool showAnimation = true) {
		bonusCoins.transform.localPosition = new Vector3(position.x, position.y, 1000);
		GameObject bonusCoinsText = GameObjectHelper.GetChildNamedGameObject (bonusCoins, "Coin", true);
		bonusCoinsText.GetComponent <Text>().text = string.Format ("+ {0}", coins);

		Camera cam = bonusCoins.GetComponent <Camera> ();

		Vector3 pos = new Vector3(0, 0, 0);

		if ( coinImage!= null ) {
            pos = coinImage.transform.position + new Vector3(20, 0, 0);
			cam = bonusCoins.GetComponent <Camera> ();
		}
			
		Vector2 o = RectTransformUtility.WorldToScreenPoint (cam, pos);

        if ( rect == null ) {
            rect = bonusCoins.transform.parent.transform as RectTransform;
        }

		Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle (rect, 
			o, 
			null, 
			out localPoint);
		
		Animation anim = bonusCoins.GetComponent <Animation> ();
        if ( anim ) {
			GameObjectHelper.SafeSetActive (bonusCoins, true);

            if (showAnimation)
            {
                bonusCoins.transform.DOLocalMove(new Vector3(localPoint.x, localPoint.y, 0), 0.5f).SetDelay(0.5f).SetEase(Ease.Linear);
            }

            anim.Play();
		}

		StartCoroutine (BonusCoinsCallback(coins, 1.5f));
	}

	IEnumerator BonusCoinsCallback(int coins, float delay) {
		yield return new WaitForSeconds (delay);

		GameManager.Instance.Player.AddCoins (coins);
		GameManager.Instance.Player.UpdatePlayerPrefs ();

		AudioClip audio = Resources.Load<AudioClip> ("Audio/Coins");
		GameManager.Instance.PlayEffect (audio);

		GameObjectHelper.SafeSetActive (bonusCoins, false);

        Destroy(gameObject);
	}
}
