using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameFramework.Preferences;
using UnityEngine;
using UnityEngine.UI;

public class CustomRateWindow : MonoBehaviour {

    [SerializeField]
    private GameObject _awardView;

	void Start () {
        if ( WillReward() ) {
            _awardView.GetComponent<Text>().text = string.Format("+{0}", Constants.RateRewardCoins);
            _awardView.SetActive(true);
        }
	}

    public static bool WillReward() {
        if ( PreferencesFactory.GetInt(Constants.KeyRateMaxRewardsTime, 0) >= Constants.RateMaxRewardsTime ) {
            return false;
        }

        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyRateRewardLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        int totalRates = PreferencesFactory.GetInt(Constants.KeyRateRewardTotal);

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last rate was yesterday
            return true;
        }

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalRates < 1)
        {
            return true;
        }

        return false;
    }

    public static void RewardRate() {
        DateTime dateTime = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyRateRewardLastDate, UnbiasedTime.Instance.Now().AddDays(-1).ToString(CultureInfo.InvariantCulture)));

        if (dateTime.Date < UnbiasedTime.Instance.Now().Date)
        { // last rate was yesterday, reset all data
            PreferencesFactory.SetString(Constants.KeyRateRewardLastDate, UnbiasedTime.Instance.Now().ToString(CultureInfo.InvariantCulture));
            PreferencesFactory.SetInt(Constants.KeyRateRewardTotal, 0);

            dateTime = UnbiasedTime.Instance.Now();
        }

        int totalRates = PreferencesFactory.GetInt(Constants.KeyRateRewardTotal);

        if (dateTime.Date == UnbiasedTime.Instance.Now().Date && totalRates < 1)
        {
            GameObject animatedCoins = GameObject.Find("AddCoinsAnimated");

            GameObject addCoinsClone = Instantiate(animatedCoins, animatedCoins.transform.parent);
            AddCoinsAnimated addCoins = addCoinsClone.GetComponent<AddCoinsAnimated>();

            RectTransform tr = null;
            GameObject _c = GameObject.Find("BoardContainer");

            if ( _c != null ) {
                tr = _c.transform as RectTransform;
            }

            addCoins.AnimateCoinsAdding(Constants.RateRewardCoins, rect: tr, showAnimation: false);
        }

        PreferencesFactory.SetInt(Constants.KeyRateRewardTotal, totalRates + 1);
        PreferencesFactory.SetInt(Constants.KeyRateMaxRewardsTime, PreferencesFactory.GetInt(Constants.KeyRateMaxRewardsTime, 0) + 1);
    }
}
