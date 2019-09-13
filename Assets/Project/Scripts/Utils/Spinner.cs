using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects;

public class Spinner : MonoBehaviour {
	GameObject _spinner;

	void Start() {
		_spinner = GameObjectHelper.GetChildNamedGameObject (gameObject, "Spinner", true);
	}

	public void Show() {
		GameObjectHelper.SafeSetActive (_spinner, true);
	}

	public void Hide() {
		GameObjectHelper.SafeSetActive (_spinner, false);
	}
}
