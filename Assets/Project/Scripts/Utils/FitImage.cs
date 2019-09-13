using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FitImage : MonoBehaviour {

	public enum Stretch{Horizontal, Vertical};
	public Stretch stretchDirection = Stretch.Vertical;

	[SerializeField]
	private RectOffset _padding = new RectOffset ();
	
	void Start () {
		Fit ();
	}

	void Update () {
		Fit();
	}

	void Fit() {
		RectTransform rect = gameObject.transform as RectTransform;
		RectTransform parentRect = gameObject.transform.parent.transform as RectTransform;

		if (stretchDirection == Stretch.Vertical) {
			float y = parentRect.rect.height / (rect.rect.height + _padding.top + _padding.bottom);

			transform.localScale = new Vector3 (y, y, transform.localScale.z);
		}

		if (stretchDirection == Stretch.Horizontal) {
			float x = parentRect.rect.width / (rect.rect.width + _padding.left + _padding.right);

			transform.localScale = new Vector3 (x, x, transform.localScale.z);
		}
	}
}
