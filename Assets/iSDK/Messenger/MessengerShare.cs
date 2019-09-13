//#if FACEBOOK_SDK
//using System.Runtime.Remoting.Messaging;

namespace iSDK.Messenger.Scripts
{

	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using UnityEngine;
	using Facebook.Unity;
	using Facebook.MiniJSON;

	// Install: Add "UnityiSDKPlugin" GameObject with DontDestroyOnLoad(); Add MessengerShare as Component<>()

	public class MessengerShare : MonoBehaviour
	{
		public static MessengerShare Instance { get; private set; }

		private CallbackManager CallbackManager;

		public enum FacebookHelperResultType
		{
			ERROR,
			CANCELLED,
			OK}

		;

		private Action doneCallback;

		void Awake ()
		{
			if (Instance != null) {
				if (Instance != this)
					Destroy (gameObject);
				return;             // return is my addition so that the inspector in unity still updates
			}

			// Here we save our singleton instance
			Instance = this;

			DontDestroyOnLoad (gameObject);
		}

		void Start ()
		{
			this.CallbackManager = new CallbackManager ();
		}

		public bool ShareDialog (Uri contentURL = null, 
		                         Uri photoURL = null, 
		                         string contentTitle = null, 
		                         string contentDescription = null,
		                         Action doneCallback = null)
		{
			this.doneCallback = doneCallback;

			FacebookDelegate<IShareResult> callback = ShareMessengerCallback;

			MessengerShare.ShareMessenger (this.AddCallback (callback),
				contentURL != null ? contentURL.AbsoluteUrlOrEmptyString () : null,
				contentTitle,
				contentDescription,
				photoURL != null ? photoURL.AbsoluteUrlOrEmptyString () : null);

			return true;
		}

		// Response from Objective-C
		void OnShareMessageComplete(string jsonString) {
			var responseObject = Json.Deserialize (jsonString) as Dictionary<string, object>;

			object obj = 0;
			if (responseObject.TryGetValue ("didComplete", out obj)) {
				if (doneCallback != null) {
					doneCallback ();
				}
			}
		}

		void ShareMessengerCallback (IShareResult result)
		{
			FacebookHelperResultType resultType = FacebookHelperResultType.ERROR;

			if (result != null) {
				if (result.Error == null || result.Error == "") {
					var responseObject = Json.Deserialize (result.RawResult) as Dictionary<string, object>;
					object obj = 0;
					if (responseObject.TryGetValue ("cancelled", out obj)) {
						resultType = FacebookHelperResultType.CANCELLED;
					} else if (responseObject.TryGetValue ("id", out obj)) {
						resultType = FacebookHelperResultType.OK;

						if (doneCallback != null) {
							doneCallback ();
						}
					}
				}
			}
		}

		private int AddCallback<T> (FacebookDelegate<T> callback) where T : IResult
		{
			string asyncId = this.CallbackManager.AddFacebookDelegate (callback);
			return Convert.ToInt32 (asyncId);
		}
			
		#if UNITY_IPHONE
		public static bool ShareMessenger (int requestId, string contentURL, string contentTitle, string contentDescription, string photoURL)
		{
			MessengerShare.IOSShareMessenger (requestId, contentURL, contentTitle, contentDescription, photoURL);

			return true;
		}

		[DllImport ("__Internal")]
		private static extern void IOSShareMessenger (
			int requestId,
			string contentURL,
			string contentTitle,
			string contentDescription,
			string photoURL);
		#elif UNITY_ANDROID
		public static bool ShareMessenger(int requestId, string contentURL, string contentTitle,
			string contentDescription, string photoURL)
		{
			var intent = new AndroidJavaClass("android.content.Intent");
			var intentObject = new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>("setAction", intent.GetStatic<string>("ACTION_SEND"));
			intentObject.Call<AndroidJavaObject>("putExtra", intent.GetStatic<string>("EXTRA_TEXT"),
				contentDescription);
			intentObject.Call<AndroidJavaObject>("setType", "text/plain");
			intentObject.Call<AndroidJavaObject>("setPackage", "com.facebook.orca");

			var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

			try
			{
				activity.Call("startActivity", intentObject);
				return true;
			}
			catch (AndroidJavaException e)
			{
				Debug.Log(e.StackTrace);
			}
			return false;
		}

#else
		public static bool ShareMessenger(int requestId, string contentURL, string contentTitle, string contentDescription, string photoURL) {
			return false;
		}
#endif
	}
}

//#endif