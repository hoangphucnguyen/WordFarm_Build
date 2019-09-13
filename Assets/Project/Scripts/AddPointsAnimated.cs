using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.GameObjects;
using GameFramework.GameStructure;
using UnityEngine;
using UnityEngine.UI;

public class AddPointsAnimated : MonoBehaviour {
	[SerializeField]
    GameObject bonusPoints;

	[SerializeField]
    GameObject pointImage;

    public void AnimateAdding(int points, Vector3 position = default(Vector3))
	{
		bonusPoints.transform.localPosition = new Vector3(position.x, position.y, 1000);
		GameObject bonusCoinsText = GameObjectHelper.GetChildNamedGameObject(bonusPoints, "Point", true);
		bonusCoinsText.GetComponent<Text>().text = string.Format("+ {0}", points);

		Camera cam = bonusPoints.GetComponent<Camera>();

		Vector3 pos = new Vector3(0, 0, 0);

		if (pointImage != null)
		{
			pos = pointImage.transform.position;
			cam = bonusPoints.GetComponent<Camera>();
		}

		Vector2 o = RectTransformUtility.WorldToScreenPoint(cam, pos);

		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(bonusPoints.transform.parent.transform as RectTransform,
			o,
			null,
			out localPoint);

		Animation anim = bonusPoints.GetComponent<Animation>();
		if (anim)
		{
			GameObjectHelper.SafeSetActive(bonusPoints, true);

			bonusPoints.transform.DOLocalMove(new Vector3(localPoint.x, localPoint.y, 0), 0.5f).SetDelay(0.5f).SetEase(Ease.Linear);

			anim.Play();
		}

		StartCoroutine(BonusCoinsCallback(points, 1.5f));
	}

	IEnumerator BonusCoinsCallback(int points, float delay)
	{
		yield return new WaitForSeconds(delay);

        GameManager.Instance.Player.AddPoints(points);
		GameManager.Instance.Player.UpdatePlayerPrefs();

		AudioClip audio = Resources.Load<AudioClip>("Audio/AddPoints");
		GameManager.Instance.PlayEffect(audio);

		GameObjectHelper.SafeSetActive(bonusPoints, false);

		Destroy(gameObject);
	}
}
