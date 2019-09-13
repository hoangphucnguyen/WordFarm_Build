using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components.AbstractClasses;
using UnityEngine.Assertions;

public class EnableIfPrizeAvailable : RunOnState {
	/// <summary>
	/// Optional Gameobject to show when a prize is available.
	/// </summary>
	[Tooltip("Optional Gameobject to show when a prize is available.")]
	public GameObject PrizeAvailableGameObject;

	/// <summary>
	/// Optional Gameobject to show when the countdown is taking place.
	/// </summary>
	[Tooltip("Optional Gameobject to show when the countdown is taking place.")]
	public GameObject PrizeCountdownGameObject;

	/// <summary>
	/// Optional Gameobject to show when waiting for a new countdown to start.
	/// </summary>
	[Tooltip("Optional Gameobject to show when waiting for a new countdown to start.")]
	public GameObject DelayGameObject;

	/// <summary>
	/// Show the correct gameobject based upon the current state.
	/// </summary>
	public override void RunMethod()
	{
		Assert.IsTrue(CustomFreePrizeManager.IsActive, "Please ensure that GameFramework.FreePrize.Components.FreePrizeManager is added to Edit->ProjectSettings->ScriptExecution before 'Default Time'.\n" +
			"GameFramework.FreePrize.Components.EnableIfPrizeAvailable does not necessarily need to appear in this list, but if it does ensure FreePrizeManager comes first");

		var isPrizeAvailable = CustomFreePrizeManager.Instance.IsPrizeAvailable();
		var isCountingDown = CustomFreePrizeManager.Instance.IsCountingDown();

		if (PrizeAvailableGameObject != null)
			PrizeAvailableGameObject.SetActive(isPrizeAvailable);

		if (PrizeCountdownGameObject != null)
			PrizeCountdownGameObject.SetActive(isCountingDown);

		if (DelayGameObject != null)
			DelayGameObject.SetActive(!isPrizeAvailable && !isCountingDown);
	}
}
