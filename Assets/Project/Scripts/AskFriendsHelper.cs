using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameFramework.Preferences;
using UnityEngine;
using GameFramework.GameObjects;
using GameFramework.Localisation;

public class AskFriendsHelper : MonoBehaviour {
    public static void SetRewardText(GameObject parent) {
        Text text = GameObjectHelper.GetChildComponentOnNamedGameObject<Text>(parent, "Text", true);

        if ( text == null ) {
            return;
        }

        text.text = string.Format("+ {0}", LocaliseText.Format("Text.NumberCoins", Constants.AskFriendsCoins));
    }

    public static bool WillReward()
    {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyAskFriendsLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last invite was yesterday
            return true;
        }

        int totalInvites = PreferencesFactory.GetInt(Constants.KeyAskFriendsTotal);

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalInvites < Constants.AskFriendsPerDay)
        {
            return true;
        }

        return false;
    }

    public static bool BonusCoins()
    {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyAskFriendsLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last invite was yesterday, reset all data
            PreferencesFactory.SetString(Constants.KeyAskFriendsLastDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));
            PreferencesFactory.SetInt(Constants.KeyAskFriendsTotal, 0);

            dateTime = UnbiasedTime.Instance.Now();
        }

        bool addedCoins = false;

        int totalInvites = PreferencesFactory.GetInt(Constants.KeyAskFriendsTotal);

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalInvites < Constants.AskFriendsPerDay)
        {
            GameObject animatedCoins = GameObject.Find("AddCoinsAnimated");

            GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
            AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

            addCoins.AnimateCoinsAdding(Constants.AskFriendsCoins, rect: GameObject.Find("BoardContainer").transform as RectTransform, showAnimation: false);

            addedCoins = true;
        }

        PreferencesFactory.SetInt(Constants.KeyAskFriendsTotal, totalInvites + 1);

        return addedCoins;
    }
}
