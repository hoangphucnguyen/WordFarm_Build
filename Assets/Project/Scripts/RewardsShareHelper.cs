using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameFramework.Preferences;
using UnityEngine;

public class RewardsShareHelper : MonoBehaviour {

    public static bool WillReward()
    {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyRewardShareLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last invite was yesterday
            return true;
        }

        int totalInvites = PreferencesFactory.GetInt(Constants.KeyRewardShareTotal);

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalInvites < Constants.RewardSharePerDay)
        {
            return true;
        }

        return false;
    }

    public static bool RewardShareCoins()
    {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyRewardShareLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last invite was yesterday, reset all data
            PreferencesFactory.SetString(Constants.KeyRewardShareLastDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));
            PreferencesFactory.SetInt(Constants.KeyRewardShareTotal, 0);

            dateTime = UnbiasedTime.Instance.Now();
        }

        bool addedCoins = false;

        int totalInvites = PreferencesFactory.GetInt(Constants.KeyRewardShareTotal);

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalInvites < Constants.RewardSharePerDay)
        {
            GameObject animatedCoins = GameObject.Find("AddCoinsAnimated");

            GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
            AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

            GameObject BoardContainer = GameObject.Find("BoardContainer");

            addCoins.AnimateCoinsAdding(Constants.RewardShareCoins, rect: (BoardContainer != null) ? BoardContainer.transform as RectTransform : null, showAnimation: false);

            addedCoins = true;
        }

        PreferencesFactory.SetInt(Constants.KeyRewardShareTotal, totalInvites + 1);

        return addedCoins;
    }
}
