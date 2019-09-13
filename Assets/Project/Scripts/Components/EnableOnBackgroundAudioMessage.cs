using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components.AbstractClasses;
using GameFramework.Audio.Messages;
using GameFramework.GameStructure;

public class EnableOnBackgroundAudioMessage : EnableDisableGameObjectMessaging<BackgroundVolumeChangedMessage> {

	public override void Start()
	{
		base.Start ();

		RunMethod (new BackgroundVolumeChangedMessage(GameManager.Instance.BackGroundAudioVolume, GameManager.Instance.BackGroundAudioVolume));
	}

	public override bool IsConditionMet(BackgroundVolumeChangedMessage message)
	{
		return message.NewVolume < 0.1f;
	}
}
