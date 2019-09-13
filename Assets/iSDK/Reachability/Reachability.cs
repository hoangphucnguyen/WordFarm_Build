using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Reachability : MonoBehaviour {
	public static Reachability Instance;
	public string Domain = "www.google.com";

	void Awake() {
		Instance = this;

		#if !UNITY_EDITOR && UNITY_IOS
		_reachabilityInitialize(Domain);
		#endif
	}

	[DllImport("__Internal")]
	extern static private void _reachabilityInitialize (string domain);

	[DllImport("__Internal")]
	extern static private bool _isReachable ();

	public bool IsReachable() {
		#if UNITY_EDITOR
		return true;
		#endif

		#if UNITY_ANDROID
		var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		var activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
		var contextClass =  new AndroidJavaClass("android.content.Context");

		var connectivityManager = activity.Call<AndroidJavaObject>("getSystemService", contextClass.GetStatic<string>("CONNECTIVITY_SERVICE"));
		var networkInfo = connectivityManager.Call<AndroidJavaObject>("getActiveNetworkInfo");
		var hasNet = networkInfo != null && networkInfo.Call<bool>("isConnected");

		return hasNet;
		#endif

		#if UNITY_IOS
		return _isReachable();
		#endif
	}
}
