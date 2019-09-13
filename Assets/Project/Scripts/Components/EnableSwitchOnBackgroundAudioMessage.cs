using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Audio.Messages;
using GameFramework.GameStructure;
using GameFramework.Messaging.Components.AbstractClasses;
using UnityEngine.UI;

public class EnableSwitchOnBackgroundAudioMessage : RunOnMessage<BackgroundVolumeChangedMessage> {

	public Switch SwitchObject;

	public override void Start()
	{
		base.Start ();

		RunMethod (new BackgroundVolumeChangedMessage(GameManager.Instance.BackGroundAudioVolume, GameManager.Instance.BackGroundAudioVolume));
	}

	public override bool RunMethod(BackgroundVolumeChangedMessage message)
	{
		var isConditionMet = IsConditionMet(message);

		SwitchObject.SetOn (!isConditionMet);

		return true;
	}

	public bool IsConditionMet(BackgroundVolumeChangedMessage message)
	{
		return message.NewVolume < 0.1f;
	}
}
