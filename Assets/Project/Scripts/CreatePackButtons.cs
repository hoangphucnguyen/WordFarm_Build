using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.GameItems.Components.AbstractClasses;
using GameFramework.GameStructure.GameItems.ObjectModel;

/// <summary>
/// Creates button instances for all Levels using a referenced prefab.
/// </summary>
[AddComponentMenu("Game Framework/GameStructure/Packs/CreatePackButtons")]
public class CreatePackButtons : CreateGameItemButtons<PackButton, Pack>
{
	public CreatePackButtons()
	{
		ClickUnlockedSceneToLoad = "";
	}

	/// <summary>
	/// Return a GameItemManager that this works upon.
	/// </summary>
	/// <returns></returns>
	protected override GameItemManager<Pack, GameItem> GetGameItemManager()
	{
		return ((CustomGameManager)CustomGameManager.Instance).Packs;
	}
}
