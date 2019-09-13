using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Audio.Messages;
using GameFramework.GameStructure;
using GameFramework.Messaging.Components.AbstractClasses;
using UnityEngine.UI;

public class EnableSwitchOnEffectsAudioMessage : RunOnMessage<EffectVolumeChangedMessage> {

	public Switch SwitchObject;

	public override void Start()
	{
		base.Start ();

		RunMethod (new EffectVolumeChangedMessage(GameManager.Instance.EffectAudioVolume, GameManager.Instance.EffectAudioVolume));
	}

	public override bool RunMethod(EffectVolumeChangedMessage message)
	{
		var isConditionMet = IsConditionMet(message);

		SwitchObject.SetOn (!isConditionMet);

		return true;
	}

	public bool IsConditionMet(EffectVolumeChangedMessage message)
	{
		return message.NewVolume < 0.1f;
	}
}
