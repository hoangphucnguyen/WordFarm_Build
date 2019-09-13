using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components.AbstractClasses;
using GameFramework.Audio.Messages;
using GameFramework.GameStructure;

public class EnableOnEffectsAudioMessage : EnableDisableGameObjectMessaging<EffectVolumeChangedMessage> {

	public override void Start()
	{
		base.Start ();

		RunMethod (new EffectVolumeChangedMessage(GameManager.Instance.EffectAudioVolume, GameManager.Instance.EffectAudioVolume));
	}

	public override bool IsConditionMet(EffectVolumeChangedMessage message)
	{
		return message.NewVolume < 0.1f;
	}
}
