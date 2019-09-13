using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Messaging;

public class SettingsStateChanged : BaseMessage {
	public readonly bool Opened;

	public SettingsStateChanged(bool opened)
	{
		Opened = opened;
	}
}
