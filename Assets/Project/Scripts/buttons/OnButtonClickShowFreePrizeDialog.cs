using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.UI.Buttons.Components.AbstractClasses;
using UnityEngine.Assertions;
using GameFramework.Display.Other;

/// <summary>
/// Show the free prize dialog when the button is clicked.
/// 
/// This automatically hooks up the button onClick listener
/// </summary>
public class OnButtonClickShowFreePrizeDialog : OnButtonClick
{
	public override void OnClick()
	{
		Assert.IsTrue(CustomFreePrizeManager.IsActive, "You need to add the FreePrizeManager to the scene.");

		StartCoroutine (CoRoutines.DelayedCallback (Constants.DelayButtonClickAction, () => {
			CustomFreePrizeManager.Instance.ShowFreePrizeDialog();
		}));
	}
}
