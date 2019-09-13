using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components;

public class Loading : Singleton<Loading> {
	public GameObject LoadingObject;

	public void Show () {
		LoadingObject.SetActive (true);
	}

	public void Hide () {
		LoadingObject.SetActive (false);
	}
}
