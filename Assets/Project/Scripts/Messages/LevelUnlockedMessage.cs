using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Messaging;

public class LevelUnlockedMessage : BaseMessage {
	public int level;

	public LevelUnlockedMessage(int level) {
		this.level = level;
	}
}
