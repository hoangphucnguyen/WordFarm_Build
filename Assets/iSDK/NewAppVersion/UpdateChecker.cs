using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Globalization;
using UnityEngine.EventSystems;

public class UpdateChecker : MonoBehaviour
{
	public static UpdateChecker Instance;

	public string AndroidUrl;
	public string iTunesAppName;
	public string iTunesCountry;

	int versionCode;
	bool stop;
	bool pause;

	[DllImport("__Internal")]
	extern static private int _harpyInitialize (string iTunesAppName, string country);

	[DllImport("__Internal")]
	extern static private int _harpyCheck ();

	void Awake() {
		Instance = this;
	}

	void Start() {
		string title = "Update Available";
		string message = "A new version is available. Please update now.";

	#if !UNITY_EDITOR
		#if UNITY_ANDROID
		if ( AndroidUrl != null && !AndroidUrl.Equals ("") ) {
			StartCoroutine (CheckForUpdate(AndroidUrl, title, message));
		}
		#endif

		#if UNITY_IOS
		CheckIOSForUpdate(iTunesAppName, iTunesCountry);
		#endif
	#endif
	}

	void CheckIOSForUpdate(string appName, string country) {
		_harpyInitialize (appName, country);
		_harpyCheck ();
	}

	void Update() {
		if ( stop ) {
			stop = false;

			StopCheckForUpdate ();
		}

		if ( pause ) {
			pause = false;

			PauseCheckForUpdate ();
		}
	}

	bool ShouldCheckForUpdate() {
		// потр. е натиснал Update без да е сигурно, че е обновил, спираме да го уведомяваме за тази версия
		int version = PlayerPrefs.GetInt ("CheckAppVersionLatestNumber", 0);

		if ( version >= versionCode ) {
			return false;
		}

		return true;
	}

	bool ExpiredDateCheckForUpdate() {
		DateTime dateTime = DateTime.Parse(PlayerPrefs.GetString ("CheckAppVersionLatestDate", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

		// минали са 24 часа от последната проверка
		if ( DateTime.Now > dateTime.AddDays(1) ) {
			return true;
		}

		return false;
	}

	void StopCheckForUpdate() {
		PlayerPrefs.SetInt ("CheckAppVersionLatestNumber", versionCode);
		PlayerPrefs.Save ();
	}

	void PauseCheckForUpdate() {
		// потр. е натиснал на "Close" спира да го уведомява за 24 часа
		PlayerPrefs.SetString ("CheckAppVersionLatestDate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
		PlayerPrefs.Save ();
	}

	public void Action(bool stop = false, bool pause = false) {
		this.stop = stop;
		this.pause = pause;
	}

	public static void PositiveClick() { // this is called from Android UI thread
		UpdateChecker.Instance.Action (true);
	}

	public static void NegativeClick() { // this is called from Android UI thread
		UpdateChecker.Instance.Action (false, true);
	}

    public IEnumerator<AsyncOperation> CheckForUpdate(string url, string dialogTitle, string dialogMessage)
    {
        var www = UnityWebRequest.Get(url);
        
		yield return www.Send();
        
		if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            var result = www.downloadHandler.text;
            Debug.Log(result);
            if (!string.IsNullOrEmpty(result))
            {
                var updateChecker = JsonUtility.FromJson<Model>(result);
                if (updateChecker != null)
                {
                    versionCode = updateChecker.versionCode;

                    var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    var currentActivity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

                    var packageInfo = currentActivity.Call<AndroidJavaObject>("getPackageManager")
                        .Call<AndroidJavaObject>("getPackageInfo", currentActivity.Call<string>("getPackageName"), 0);

                    var appVersion = packageInfo.Get<int>("versionCode");
                    try
                    {
                        Debug.Log("VERSION: " + versionCode + " APP VERSION: " + appVersion);

						bool check = ExpiredDateCheckForUpdate();

						if ( check ) {
							check = ShouldCheckForUpdate();
						}

						if (versionCode > appVersion && check)
                        {
                            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                            {
                                var alertDialog = new AndroidJavaClass("android/app/AlertDialog");
                                var alertDialogBuilder = new AndroidJavaObject("android/app/AlertDialog$Builder",
                                    currentActivity);

                                if (!string.IsNullOrEmpty(dialogTitle))
                                {
                                    alertDialogBuilder.Call<AndroidJavaObject>("setTitle", dialogTitle);
                                }
                                alertDialogBuilder.Call<AndroidJavaObject>("setMessage", dialogMessage);
                                alertDialogBuilder.Call<AndroidJavaObject>("setCancelable", true);
                                alertDialogBuilder.Call<AndroidJavaObject>("setPositiveButton", "Update",
										new PositiveClickListener());
                                alertDialogBuilder.Call<AndroidJavaObject>("setNegativeButton", "Close",
                                    new NegativeClickListener());

                                var dialog = alertDialogBuilder.Call<AndroidJavaObject>("create");
                                dialog.Call("show");
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("E: " + e.StackTrace);
                    }
                }
            }
        }
    }

    [Serializable]
    public class Model
    {
        public int versionCode;
        public string versionName;
        public string success;

        public int GetVersionCode()
        {
            return versionCode;
        }
    }

    public class PositiveClickListener : AndroidJavaProxy
    {
		public PositiveClickListener() : base("android.content.DialogInterface$OnClickListener")
        {
			
        }

        public void onClick(AndroidJavaObject dialog, int which)
        {
			UpdateChecker.PositiveClick ();

            var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

            var intent = new AndroidJavaClass("android.content.Intent");
            var uriClass = new AndroidJavaClass("android.net.Uri");
            var intentObject = new AndroidJavaObject ("android.content.Intent", intent.GetStatic<string>("ACTION_VIEW"),
                uriClass.CallStatic<AndroidJavaObject>("parse", "market://details?id=" + currentActivity.Call<string>("getPackageName")));
            try
            {
                currentActivity.Call("startActivity", intentObject);
            }
            catch (AndroidJavaException e)
            {
                Debug.Log(e.StackTrace);
            }
            dialog.Call("dismiss");
        }
    }

    public class NegativeClickListener : AndroidJavaProxy
    {
        public NegativeClickListener() : base("android.content.DialogInterface$OnClickListener")
        {
        }

        public void onClick(AndroidJavaObject dialog, int which)
        {
			UpdateChecker.NegativeClick ();

            dialog.Call("dismiss");
        }
    }
}