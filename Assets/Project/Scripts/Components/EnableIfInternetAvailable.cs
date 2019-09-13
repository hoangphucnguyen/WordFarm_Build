using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components.AbstractClasses;

public class EnableIfInternetAvailable : EnableDisableGameObject {

	public override bool IsConditionMet()
	{
		return Reachability.Instance.IsReachable ();
	}
}
