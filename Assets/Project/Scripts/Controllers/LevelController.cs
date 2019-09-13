using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components;
using GameFramework.Billing.Messages;
using GameFramework.GameStructure;
using GameFramework.Preferences;
using GameFramework.GameStructure.GameItems.ObjectModel;
using System;
using UnityEngine.Assertions;
using GameFramework.GameStructure.Levels.ObjectModel;
using GameFramework.Helper;

public class ItemsRange {
	public int From;
	public int To;

	public ItemsRange(int f, int t) {
		this.From = f;
		this.To = t;
	}
}

public class LevelController : Singleton<LevelController> {

	protected override void GameSetup() {
		
	}

	public override void SaveState()
	{
		
	}

	public void Reload() {
		
	}

    public int PointsForPack(Pack pack)
    {
        ItemsRange range = PackLevelsRange(pack);

        int points = 0;

        for (int i = range.From; i < range.To + 1; i++)
        {
            Level l = GameManager.Instance.Levels.GetItem(i);

            if (l != null)
            {
                points += l.HighScore;
            }
        }

        return points;
    }

    // mark this pack as completed
    public void PackProgressCompleted(Level level) {
        Pack pack = PackForLevel(level);

        if ( pack == null ) {
            return;
        }

        if ( pack.JsonData == null ) {
            pack.LoadData();
        }

        ItemsRange range = PackLevelsRange(pack);

        bool completed = true;
        for (int i = range.From; i < range.To+1; i++)
        {
            Level l = GameManager.Instance.Levels.GetItem(i);

            if ( l.ProgressBest < 1.0f ) {
                completed = false;
            }
        }

        if (completed && pack.Progress < 1.0f) {
            pack.Progress = 1.0f;
            pack.UpdatePlayerPrefs();

            UnlockNextPack(pack);
        }
    }

    public void UnlockNextPack(Pack pack) {
        if ( pack == null ) {
            return;
        }

        Pack nextPack = Packs().GetItem(pack.Number+1);

        if ( nextPack == null ) {
            return;
        }

        UnlockPack(nextPack);
    }

	public Pack PackForLevel(Level level) {
		for (int i = 0; i < TotalPacks (); i++) {
			Pack p = Packs ().GetItem (i+1);
			ItemsRange range = PackLevelsRange (p);
			
			if ( level.Number >= range.From && level.Number <= range.To ) {
				return p;
			}
		}

		return null;
	}

    public Rank RankForPack(Pack pack) {
        for (int i = 0; i < TotalRanks(); i++)
		{
            Rank r = Ranks().GetItem(i + 1);
			ItemsRange range = RankPacksRange(r);

			if (pack.Number >= range.From && pack.Number <= range.To)
			{
				return r;
			}
		}

		return null;
    }

	public int PackCoins(Pack pack) {
		return 0;
	}

	public int PackUnlockScores(Pack pack) {
        if ( pack == null ) {
            return 0;
        }

		if ( pack.JsonData == null ) {
			pack.LoadData ();
		}

		int score = (int)pack.JsonData.GetNumber ("unlock_scores");

		return score;
	}

	public static ItemsRange PackLevelsRange(Pack pack) {
        if ( pack == null ) {
            return new ItemsRange(0, 0);
        }

		if ( pack.JsonData == null ) {
			pack.LoadData ();
		}

		JSONObject levelObject = pack.JsonData.GetObject ("levels");
        int f = (int)levelObject.GetNumber ("from");
		int t = (int)levelObject.GetNumber ("to");

		return new ItemsRange (f, t);
	}

	public static ItemsRange RankPacksRange(Rank rank) {
		if ( rank.JsonData == null ) {
			rank.LoadData ();
		}

        JSONObject rankObject = rank.JsonData.GetObject ("packs");
		int f = (int)rankObject.GetNumber ("from");
		int t = (int)rankObject.GetNumber ("to");

		return new ItemsRange (f, t);
	}

	public bool ShouldUnlockPack(Pack pack) {
        if ( pack == null ) {
            return false;
        }

		if ( GameManager.Instance.Player.Score >= PackUnlockScores(pack) ) {
			return true;
		}

		return false;
	}

	public void UnlockPack(Pack pack) {
        if ( pack == null ) {
            return;
        }

		pack.IsUnlocked = true;
		pack.UpdatePlayerPrefs ();

		GameManager.SafeQueueMessage (new PackUnlockedMessage(pack));
		ItemsRange range = PackLevelsRange (pack);

		// unlock first level for this pack
		UnlockLevel (range.From);

		SaveState ();
	}

	public void CheckAndUnlockPacks() {
		for ( int i = 0; i < TotalPacks(); i++ ) {
			Pack pack = Packs ().GetItem (i + 1);

			if ( pack.IsUnlocked == false && ShouldUnlockPack(pack) ) {
				UnlockPack (pack);
			}
		}
	}

	public bool LastLevelInPack(Level level) {
		Pack pack = PackForLevel (level);

        if ( pack == null ) {
            return false;
        }

		ItemsRange range = PackLevelsRange (pack);

		if ( range.To == level.Number ) {
			return true;
		}

		return false;
	}

	public bool UnlockNextPack() { // find first locked pack and unlock it
		for ( int i = 0; i < TotalPacks(); i++ ) {
			Pack pack = Packs ().GetItem (i + 1);

            if ( pack != null && pack.IsUnlocked == false ) {
				UnlockPack (pack);

				return true;
			}
		}

		return false;
	}

    public Rank RankForLevel(Level level) {
        Pack pack = PackForLevel(level);
        Rank rank = RankForPack(pack);

        return rank;
    }

	public bool LastLevelInRank(Level level)
	{
        if ( LastLevelInPack(level) == false ) {
            return false;
        }

		Pack pack = PackForLevel(level);
        Rank rank = RankForPack(pack);

        ItemsRange range = RankPacksRange(rank);

		if (range.To == pack.Number)
		{
			return true;
		}

		return false;
	}

	public static PackGameItemManager Packs() {
		return ((CustomGameManager)CustomGameManager.Instance).Packs;
	}

	public static RankGameItemManager Ranks() {
		return ((CustomGameManager)CustomGameManager.Instance).Ranks;
	}

	public static int TotalPacks() {
		return Packs().Items.Length;
	}

    public static int TotalRanks() {
        return Ranks().Items.Length;
    }

	public static int TotalLevelsForPack(Pack pack) {
		ItemsRange range = PackLevelsRange(pack);
		// 10 - 6 = 4
		// 5 - 1 = 4
		return (range.To - range.From) + 1;
	}

	public static int TotalLevels() {
		return GameManager.Instance.Levels.Items.Length;
	}

	public static void UnlockLevel(int index) {
		Level level = GameManager.Instance.Levels.GetItem (index);

		if ( level != null && !level.IsUnlocked ) {
			level.IsUnlocked = true;
			level.UpdatePlayerPrefs();

			GameManager.SafeQueueMessage (new LevelUnlockedMessage(index));
		}
	}

    public static Level FirstUnplayedLevel() {
        Level _level = null;
        for (int i = GameManager.Instance.Levels.Items.Length; i > 0; i--){
            Level level = GameManager.Instance.Levels.GetItem(i);

            if (level.IsUnlocked && level.ProgressBest < 0.9f ) {
                _level = level;
                break;
            }
        }

        return _level;
    }

    public static bool NeedToGenerateMoreLevels()
    {
        int levels = GameManager.Instance.Levels.Items.Length;

        Level level = GameManager.Instance.Levels.Selected;

        // we are at last level
        if (level != null && level.Number == levels )
        {
            return true;
        }

        return false;
    }

    public static void GenerateMoreLevels()
    {
        int NumberOfAdditionalCreatedLevels = PreferencesFactory.GetInt(Constants.KeyNumberOfAdditionalCreatedLevels);
        int StartupLevels = ((CustomGameManager)CustomGameManager.Instance).StartupLevels;

#if UNITY_EDITOR
        Debug.Log("GenerateMoreLevels: StartupLevels: " + StartupLevels);
        Debug.Log("GenerateMoreLevels: NumberOfAdditionalCreatedLevels before: " + NumberOfAdditionalCreatedLevels);
#endif

        NumberOfAdditionalCreatedLevels += 1;
        GameManager.Instance.Levels.Load(1, StartupLevels + NumberOfAdditionalCreatedLevels, 0, true);

#if UNITY_EDITOR
        Debug.Log("GenerateMoreLevels: NumberOfAdditionalCreatedLevels after: " + NumberOfAdditionalCreatedLevels);
        Debug.Log("GenerateMoreLevels: Total: " + (StartupLevels + NumberOfAdditionalCreatedLevels));
#endif

        PreferencesFactory.SetInt(Constants.KeyNumberOfAdditionalCreatedLevels, NumberOfAdditionalCreatedLevels);
        PreferencesFactory.Save();
    }

    public static void LoadLevelFromServer(Action<JSONArray> callback = null) {
        GameSparksManager.Instance.GetWords((JSONArray _arr) => {
            PreferencesFactory.SetString(Constants.KeyRandomLevelWords, _arr.ToString());

            if ( callback != null ) {
                callback(_arr);
            }
        });
    }

    public static JSONArray GetWordsFromServer() {
        string str = PreferencesFactory.GetString(Constants.KeyRandomLevelWords);

        if ( str == "" ) {
            return null;
        }

        return JSONArray.Parse(str);
    }

    public void UnlockAll()
    {
        foreach (Pack pack in Packs())
        {
            pack.IsUnlocked = true;
            pack.UpdatePlayerPrefs();
        }

        foreach (Level level in GameManager.Instance.Levels.Items)
        {
            level.IsUnlocked = true;
            level.UpdatePlayerPrefs();
        }

        Debug.Log("Unlocking entire game");

        SaveState();
    }
}
