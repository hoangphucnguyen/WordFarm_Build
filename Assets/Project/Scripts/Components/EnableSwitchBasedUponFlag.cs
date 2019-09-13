using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components.AbstractClasses;
using GameFramework.Preferences;
using UnityEngine.UI;

public class EnableSwitchBasedUponFlag : RunOnState
{
	[Tooltip("A unique PlayerPrefs key that acts as the flag.")]
	public string Key;
	public Switch SwitchObject;

	public override void RunMethod()
	{
		var isConditionMet = IsConditionMet();

		SwitchObject.SetOn (isConditionMet);
	}

	public bool IsConditionMet()
	{
		return PreferencesFactory.IsFlagSet(Key);
	}

	public void SetFlag()
	{
		PreferencesFactory.SetFlag(Key);
	}

	public void ClearFlag()
	{
		PreferencesFactory.ClearFlag(Key);
	}
}
