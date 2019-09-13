using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure.GameItems.ObjectModel;

[CreateAssetMenu(fileName = "Pack_x", menuName = "Game Framework/Pack")]
public class Pack : GameItem
{

	/// <summary>
	/// A unique identifier for this type of GameItem
	/// </summary>
	public override string IdentifierBase { get { return "Pack"; }}

	/// <summary>
	/// A unique shortened version of IdentifierBase to save memory.
	/// </summary>
	public override string IdentifierBasePrefs { get { return "PK"; } }

	/// <summary>
	/// A field that you can optionally use for recording the progress. Typically this should be in the range 0..1
	/// </summary>
	public float Progress { get; set; }


	/// <summary>
	/// Provides a simple method that you can overload to do custom initialisation in your own classes.
	/// This is called after ParseLevelFileData (if loading from resources) so you can use values setup by that method. 
	/// 
	/// If overriding from a base class be sure to call base.CustomInitialisation()
	/// </summary>
	public override void CustomInitialisation()
	{
		Progress = GetSettingFloat("Progress", 0);
	}

	/// <summary>
	/// Update PlayerPrefs with setting or preferences for this item.
	/// Note: This does not call PlayerPrefs.Save()
	/// 
	/// If overriding from a base class be sure to call base.ParseGameData()
	/// </summary>
	public override void UpdatePlayerPrefs()
	{
		base.UpdatePlayerPrefs();

        SetSettingFloat("Progress", Progress);
	}

}
