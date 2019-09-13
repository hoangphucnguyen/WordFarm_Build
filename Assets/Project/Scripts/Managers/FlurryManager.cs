using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flurry;

public class FlurryManager : MonoBehaviour {

	[Header("Flurry Settings")]
	[SerializeField] private string _iosApiKey = string.Empty;
	[SerializeField] private string _androidApiKey = string.Empty;

	private void Awake()
	{
		// For Flurry Android only:
        FlurryAndroid.SetLogEnabled(false);

		// For Flurry iOS only:
		FlurryIOS.SetDebugLogEnabled(false);

		IAnalytics service = Flurry.Flurry.Instance;

		service.StartSession(_iosApiKey, _androidApiKey);

		service.LogUserID (GameSparksManager.UserID ());
	}
}
