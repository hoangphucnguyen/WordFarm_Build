﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.GameItems.ObjectModel;

public class PackGameItemManager : GameItemManager<Pack, GameItem>
{
	/// <summary>
	/// Called when the current selection changes. Override this in any base class to provide further handling such as sending out messaging.
	/// </summary>
	/// <param name="newSelection"></param>
	/// <param name="oldSelection"></param>
	/// You may want to override this in your derived classes to send custom messages.
	public override void OnSelectedChanged(Pack newSelection, Pack oldSelection)
	{
//		GameManager.SafeQueueMessage(new LevelChangedMessage(newSelection, oldSelection));
	}
}
