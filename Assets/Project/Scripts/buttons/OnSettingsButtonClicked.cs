using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameFramework.GameObjects.Components;
using GameFramework.GameStructure;
using GameFramework.UI.Buttons.Components.AbstractClasses;
using GameFramework.Display.Other;

[RequireComponent(typeof(Button))]
public class OnSettingsButtonClicked : OnButtonClick {
	public override void OnClick()
	{
		StartCoroutine (CoRoutines.DelayedCallback (Constants.DelayButtonClickAction, () => {
			GameManager.SafeQueueMessage (new SettingsOpenMessage());
		}));
	}
}
