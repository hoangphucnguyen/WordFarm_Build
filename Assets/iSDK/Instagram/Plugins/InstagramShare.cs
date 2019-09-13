namespace iSDK.Instagram.Scripts {

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using UnityEngine;
	using Facebook.Unity;

	public class InstagramShare : MonoBehaviour {
		public static InstagramShare Instance { get; private set; }

		static bool HasHandshook = false;

		void Awake()
		{
			if (Instance != null)
			{
				if (Instance != this)
					Destroy(gameObject);
				return;             // return is my addition so that the inspector in unity still updates
			}

			// Here we save our singleton instance
			Instance = this;

			DontDestroyOnLoad(gameObject);
		}

		public static void HandShake() 
		{
//			IOSShareInstagramHandshake();
			HasHandshook = true;
		}

		public bool ShareDialog (Uri photoURL = null) 
		{
			if (!HasHandshook) {
				HandShake ();
			}
			
			InstagramShare.ShareInstagram (photoURL != null ? photoURL.AbsoluteUrlOrEmptyString() : null);

			return true;
		}
		// TODO - make a copy of facebook AbsoluteUrlOrEmptyString

#if UNITY_IPHONE
		public static void ShareInstagram(string photoURL) {
			InstagramShare.IOSShareInstagram (null, photoURL);
		}

		[DllImport("__Internal")]
		static extern void IOSShareInstagramHandshake();

		[DllImport("__Internal")]
		public static extern void IOSShareInstagram(string message, string photoURL);
#elif UNITY_ANDROID
		public static void ShareInstagram(string photoURL) {
			AndroidJavaClass ajc = new AndroidJavaClass("gameframework.com.socials.InstagramShare");
			AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

			ajc.CallStatic("shareDialog", activity, photoURL);
		}
#else
		public static void ShareInstagram(string photoURL) {
		}
#endif
	}
}