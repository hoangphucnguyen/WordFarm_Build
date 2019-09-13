using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components.AbstractClasses;

public class EnableOnUserLogged : EnableDisableGameObject {

	public override bool IsConditionMet()
	{
		return GameSparksManager.IsUserLoggedIn ();
	}
}
