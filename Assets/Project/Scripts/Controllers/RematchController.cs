using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.UI.Dialogs.Components;
using GameFramework.GameStructure;

public class RematchController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	public void AcceptButton() {
		GameController.Instance.AcceptRematch ();
	}

	public void DeclineButton() {
		GameController.Instance.DeclineRematch ();
	}

	public void Close() {
		DialogInstance dialogInstance = gameObject.GetComponent <DialogInstance> ();
		dialogInstance.Done ();

		GameManager.LoadSceneWithTransitions ("Lobby");
	}
}
