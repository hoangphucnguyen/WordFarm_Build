using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameFramework.GameStructure;

[RequireComponent(typeof(Button))]
public class ButtonClickAudio : MonoBehaviour {
	public AudioClip audioClip;
	Button button;

	void Start () {
		button = GetComponent <Button> ();
		button.onClick.AddListener (() => Play());
	}

	void Play () {
		if ( audioClip == null ) {
			return;
		}

		GameManager.Instance.PlayEffect (audioClip);
	}
}
