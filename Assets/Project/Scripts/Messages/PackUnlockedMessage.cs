using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Messaging;

public class PackUnlockedMessage : BaseMessage {
	public Pack pack;

	public PackUnlockedMessage(Pack pack)
	{
		this.pack = pack;
	}
}
