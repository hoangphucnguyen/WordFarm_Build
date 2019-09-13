using UnityEngine;
using System.Collections;
using GameFramework.GameStructure;
using System.Collections.Generic;

public class ButtonUtils
{
    public static Dictionary<string, AudioClip> _preloaded = new Dictionary<string, AudioClip>();

    public static void PlayClickSound()
    {
        PlaySound("Button");
    }

    public static void PlayAlreadySound()
    {
        PlaySound("AlreadyAnswer");
    }

    public static void PlayCorrectSound()
    {
        PlaySound("CorrectAnswer");
    }

    public static void PlayWrongSound()
    {
        PlaySound("Jump");
    }

    public static void PlayEndLevelSound()
    {
        PlaySound("EndLevel");
    }

    public static void PlayPointsSound()
    {
        PlaySound("Scores");
    }

	public static void PlayFantasticSound()
	{
		PlaySound("Fantastic");
	}

    public static void PlayHintsSound() 
    {
        PlaySound("Star");
    }

    public static void PlayWinSound() 
    {
        PlaySound("PositiveFinal");
    }

	public static void PlayLoseSound()
	{
        PlaySound("Jump");
	}

	public static void PlayYourTurnSound()
	{
		PlaySound("YourTurn");
	}

	public static void PlayGameBombSound()
	{
		PlaySound("GameBomb");
	}

    static void PlaySound(string sound)
    {
        AudioClip audio = null;

        if (_preloaded.ContainsKey(sound))
        {
            audio = _preloaded[sound];
        }
        else
        {
            audio = Resources.Load<AudioClip>(string.Format("Audio/{0}", sound));
            _preloaded.Add(sound, audio);
        }

        GameManager.Instance.PlayEffect(audio);
    }
}

