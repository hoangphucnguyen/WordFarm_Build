using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.GameItems.Components.AbstractClasses;
using GameFramework.GameStructure.GameItems.ObjectModel;

/// <summary>
/// Creates button instances for all Levels using a referenced prefab.
/// </summary>
[AddComponentMenu("Game Framework/GameStructure/Ranks/CreateRankButtons")]
public class CreateRankButtons : CreateGameItemButtons<RankButton, Rank>
{
	public CreateRankButtons()
	{
		ClickUnlockedSceneToLoad = "";
	}

	/// <summary>
	/// Return a GameItemManager that this works upon.
	/// </summary>
	/// <returns></returns>
	protected override GameItemManager<Rank, GameItem> GetGameItemManager()
	{
		return ((CustomGameManager)CustomGameManager.Instance).Ranks;
	}
}
