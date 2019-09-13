using UnityEngine;
using GameFramework.GameObjects.Components;
using System;
using GameFramework.Preferences;
using GameFramework.Localisation;
using PaperPlaneTools;
using System.Globalization;
using GameFramework.GameStructure;
using System.Collections.Generic;
using SA.IOSNative.UserNotifications;

public class LocalNotifications : SingletonPersistant<LocalNotifications> {

	protected override void GameSetup()
	{
		base.GameSetup ();

		#if UNITY_IPHONE
		SA.IOSNative.Core.AppController.OnApplicationDidBecomeActive += OnApplicationDidBecomeActive;

		IOSNativeUtility.SetApplicationBagesNumber(0);
		#endif

		CheckNotificationsPermission ();
	}

	private void OnApplicationDidBecomeActive() {
		IOSNativeUtility.SetApplicationBagesNumber(0);
	}

	public static bool Allowed() {
		#if UNITY_EDITOR
		return true;
		#endif

		#if UNITY_IPHONE
		UnityEngine.iOS.NotificationType type = UnityEngine.iOS.NotificationServices.enabledNotificationTypes;

		if ( type != UnityEngine.iOS.NotificationType.None ) {
			return true;
		}

		return false;
		#endif

		#if UNITY_ANDROID
		return true;
		#endif
	}

	public static void AllowDialog(Action ignoreAction = null) {
		string text = LocaliseText.Get ("Notifications.DeniedText");

		Alert alert = new Alert ("", text)
			.SetPositiveButton (LocaliseText.Get ("Notifications.AllowText"), () => {
				iSDK.Utils.OpenSettings ();
			})
			.SetNeutralButton (LocaliseText.Get ("Notifications.IgnoreText"), () => {
				if ( ignoreAction != null ) {
					ignoreAction();
				}
			})
			.AddOptions (new AlertIOSOptions () {
				PreferableButton = Alert.ButtonType.Positive
			});

		alert.Show ();
	}

	public static int RegisterNotification(int id, DateTime fireDate, string body) {
		if ( fireDate < UnbiasedTime.Instance.Now()  ) { // fire date is in past
			return -1;
		}

		// notifications are disabled
		if ( PreferencesFactory.GetInt (Constants.KeyNotificationsAllowed, 1) == 0 ) {
			return -1;
		}

#if UNITY_IPHONE
        NotificationCenter.RequestPermissions((result) =>
        {
            PreferencesFactory.SetInt(Constants.KeyNotificationsAllowed, result.IsSucceeded ? 1 : 0);

            //Debug.Log("RequestPermissions callback: err: " + (result.HasError ? result.Error.Message : null) + "; success: " + result.IsSucceeded + "; failed: " + result.IsFailed);
        });

        var content = new NotificationContent();
        content.Body = body;
        content.Badge = 1;

        var dateComponents = new DateComponents() { 
            Year = fireDate.Year,
            Month = fireDate.Month,
            Day = fireDate.Day,
            Hour = fireDate.Hour,
            Minute = fireDate.Minute,
            Second = fireDate.Second
        };

        var trigger = new CalendarTrigger(dateComponents);
        var request = new NotificationRequest(id.ToString(), content, trigger);

        NotificationCenter.AddNotificationRequest(request, (result) => {
            //Debug.Log("AddNotificationRequest callback: err: " + (result.HasError ? result.Error.Message : null) + "; success: " + result.IsSucceeded + "; failed: " + result.IsFailed);
        });
		#endif

		#if UNITY_ANDROID
		TimeSpan delay = fireDate - UnbiasedTime.Instance.Now();
		AndroidNotificationBuilder builder = new AndroidNotificationBuilder(id, 
																			GameManager.Instance.GameName, 
																			body, 
																			(int)delay.TotalSeconds);

		AndroidNotificationManager.Instance.ScheduleLocalNotification(builder);
		#endif

		return id;
	}

	public static void CancelNotification(int id) {
		#if UNITY_IPHONE
        NotificationCenter.CancelUserNotificationById(id.ToString());
		#endif

		#if UNITY_ANDROID
		AndroidNotificationManager.Instance.CancelLocalNotification (id);
		#endif
	}

	public static void CancelAllNotifications() {
		#if UNITY_IPHONE
        NotificationCenter.CancelAllNotifications();
		#endif

		#if UNITY_ANDROID
		AndroidNotificationManager.Instance.CancelAllLocalNotifications ();
		#endif
	}

	void CheckNotificationsPermission() {
		int CountMainMenuColdStart = PreferencesFactory.GetInt (Constants.KeyCountMainMenuColdStart);
		DateTime now = UnbiasedTime.Instance.Now ();

		// not in first launch
		if ( CountMainMenuColdStart > 0 && !Allowed() ) {
			// the date when we detected that notifications are denied
			if ( !PreferencesFactory.HasKey (Constants.KeyNoNotificationPermissionDeniedDate) ) {
				PreferencesFactory.SetString (Constants.KeyNoNotificationPermissionDeniedDate, now.ToString (CultureInfo.InvariantCulture));

				return;
			}

			DateTime dateOfDenying = DateTime.Parse (PreferencesFactory.GetString (Constants.KeyNoNotificationPermissionDeniedDate, now.ToString (CultureInfo.InvariantCulture)));
			float minutes3days = 3 * 24 * 60;
			float minutes10days = 10 * 24 * 60;

			if ( Debug.isDebugBuild ) {
				minutes3days = 1f; // 30 sec
				minutes10days = 2f; // 1 min
			}

			// 3 days before show alert for first time
			if ( now.Date < dateOfDenying.AddMinutes (minutes3days).Date ) {
				return;
			}

			DateTime postponeDate = DateTime.Parse(PreferencesFactory.GetString(Constants.KeyNoNotificationPermissionDate, now.ToString(CultureInfo.InvariantCulture)));

			// 10 days to show alert again if user postpone it
			if ( PreferencesFactory.HasKey (Constants.KeyNoNotificationPermissionDate) 
				&& now.Date < postponeDate.AddMinutes (minutes10days).Date ) 
			{
				return;
			}

			PreferencesFactory.DeleteKey (Constants.KeyNoNotificationPermissionDate);

			string text = LocaliseText.Get ("Notifications.DeniedText");

			Alert alert = new Alert ("", text)
				.SetPositiveButton (LocaliseText.Get ("Notifications.AllowText"), () => {
					iSDK.Utils.OpenSettings ();
				})
				.SetNeutralButton (LocaliseText.Get ("Notifications.IgnoreText"), () => {
					PreferencesFactory.SetString (Constants.KeyNoNotificationPermissionDate, now.ToString (CultureInfo.InvariantCulture));
				})
				.AddOptions (new AlertIOSOptions () {
					PreferableButton = Alert.ButtonType.Positive
				});

			alert.Show ();
		}
	}
}
