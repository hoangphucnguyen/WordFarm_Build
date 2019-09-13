using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.GameItems.Components.AbstractClasses;
using GameFramework.GameStructure.GameItems.ObjectModel;

[AddComponentMenu("Game Framework/GameStructure/Packs/PackButton")]
public class PackButton : GameItemButton<Pack>
{
	

	/// <summary>
	/// Pass static parametres to base class.
	/// </summary>
	public PackButton() : base("Pack") { }

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
	protected override GameItemManager<Pack, GameItem> GetGameItemManager()
	{
		return ((CustomGameManager)CustomGameManager.Instance).Packs;
	}
}
