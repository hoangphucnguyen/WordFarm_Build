using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.GameItems.Components.AbstractClasses;
using GameFramework.GameStructure.GameItems.ObjectModel;

[AddComponentMenu("Game Framework/GameStructure/Ranks/RankButton")]
public class RankButton : GameItemButton<Rank>
{


	/// <summary>
	/// Pass static parametres to base class.
	/// </summary>
	public RankButton() : base("Rank") { }

	public new void Awake()
	{
		base.Awake();
	}

	protected new void OnDestroy()
	{
		base.OnDestroy();
	}


	/// <summary>
	/// Additional display setup functionality
	/// </summary>
	public override void SetupDisplay()
	{
		base.SetupDisplay();
	}


	/// <summary>
	/// Returns the GameItemManager that holds Levels
	/// </summary>
	/// <returns></returns>
	protected override GameItemManager<Rank, GameItem> GetGameItemManager()
	{
		return ((CustomGameManager)CustomGameManager.Instance).Ranks;
	}
}
