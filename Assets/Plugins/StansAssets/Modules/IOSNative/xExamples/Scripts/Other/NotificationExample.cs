////////////////////////////////////////////////////////////////////////////////
//  
// @module IOS Native Plugin for Unity3D 
// @author Osipov Stanislav (Stan's Assets) 
// @support stans.assets@gmail.com 
//
////////////////////////////////////////////////////////////////////////////////


using System;
using UnityEngine;
using System.Collections;

using SA.IOSNative.UserNotifications;



public class NotificationExample : BaseIOSFeaturePreview {
	
	
	private int lastNotificationId = 0;
	
	//--------------------------------------
	// INITIALIZE
	//--------------------------------------
	
	
	void Awake() {
		


		ISN_LocalNotificationsController.OnLocalNotificationReceived += HandleOnLocalNotificationReceived;




        NotificationCenter.OnWillPresentNotification += (NotificationRequest obj) => {
            Debug.Log("OnWillPresentNotification: " + obj.Content);
        };

        var launchNotification = NotificationCenter.LaunchNotification;
        if(launchNotification.Content != null) {
            IOSMessage.Create("Launch Notification", "Messgae: " + launchNotification.Content + "\nNotification ID: " + launchNotification.Id);
        }

       


		//Checking for a local launch notification
		if(ISN_LocalNotificationsController.Instance.LaunchNotification != null) {
			ISN_LocalNotification notification = ISN_LocalNotificationsController.Instance.LaunchNotification;

			IOSMessage.Create("Launch Notification", "Messgae: " + notification.Message + "\nNotification Data: " + notification.Data);
		}


		//Checking for a remote launch notification
		if(ISN_RemoteNotificationsController.Instance.LaunchNotification != null) {
			ISN_RemoteNotification notification = ISN_RemoteNotificationsController.Instance.LaunchNotification;

			IOSMessage.Create("Launch Remote Notification", "Body: " + notification.Body);
		}
	}


	
	//--------------------------------------
	//  PUBLIC METHODS
	//--------------------------------------
	
	void OnGUI() {
		
		UpdateToStartPos();
		
		UnityEngine.GUI.Label(new UnityEngine.Rect(StartX, StartY, UnityEngine.Screen.width, 40), "Local and Push Notifications", style);


		StartY+= YLableStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Request Permissions")) {
			ISN_LocalNotificationsController.Instance.RequestNotificationPermissions();
		}


		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Print Notification Settings")) {
			CheckNotificationSettings ();
		}

		
		StartY+= YButtonStep;
		StartX = XStartPos;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Schedule Notification Silent")) {
			ISN_LocalNotificationsController.OnNotificationScheduleResult += OnNotificationScheduleResult;

			ISN_LocalNotification notification =  new ISN_LocalNotification(DateTime.Now.AddSeconds(5),"Your Notification Text No Sound", false);
			notification.SetData("some_test_data");
			notification.Schedule();

			lastNotificationId = notification.Id;
		}
		
		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Schedule Notification")) {
			ISN_LocalNotificationsController.OnNotificationScheduleResult += OnNotificationScheduleResult;

			ISN_LocalNotification notification =  new ISN_LocalNotification(DateTime.Now.AddSeconds(5),"Your Notification Text", true);
			notification.SetData("some_test_data");
			notification.SetSoundName("purchase_ok.wav");
			notification.SetBadgesNumber(1);
			notification.Schedule();

			lastNotificationId = notification.Id; 
		}
		
		
		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Cancel All Notifications")) {
			ISN_LocalNotificationsController.Instance.CancelAllLocalNotifications();
			IOSNativeUtility.SetApplicationBagesNumber(0);
		}
		
		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Cansel Last Notification")) {


			ISN_LocalNotificationsController.Instance.CancelLocalNotificationById(lastNotificationId);
		}


		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;

		UnityEngine.GUI.Label(new UnityEngine.Rect(StartX, StartY, UnityEngine.Screen.width, 40), "Local and Push Notifications", style);
	

		StartY+= YLableStep;
		StartX = XStartPos;

		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Reg Device For Push Notif. ")) {

			//ISN_RemotelNotifications.Instance.RegisterForRemoteNotifications (0);

			ISN_RemoteNotificationsController.Instance.RegisterForRemoteNotifications ((ISN_RemoteNotificationsRegistrationResult res) => {

				Debug.Log ("ISN_RemoteNotificationsRegistrationResult: " + res.IsSucceeded);
				if(!res.IsSucceeded) {
					Debug.Log (res.Error.Code + " / " + res.Error.Message);
				} else {
					Debug.Log (res.Token.DeviceId);
				}


			});
			
		}
		
		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Show Game Kit Notification")) {
			ISN_LocalNotificationsController.Instance.ShowGmaeKitNotification("Title", "Message");
		}






		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;

		UnityEngine.GUI.Label(new UnityEngine.Rect(StartX, StartY, UnityEngine.Screen.width, 40), "User Notifications", style);


		StartY+= YLableStep;
		StartX = XStartPos;

		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Reg Device For User Notif. ")) {

			SA.IOSNative.UserNotifications.NotificationCenter.RequestPermissions ((result) => {
				ISN_Logger.Log("RequestPermissions callback" + result.ToString());

			});
		}

		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Schedule for 5 sec")) {

			var content = new SA.IOSNative.UserNotifications.NotificationContent ();
			content.Title = "Title_";
			content.Subtitle = "Subtitle_";
			content.Body = "Body_";
			content.Badge = 1;
            content.Sound = "beep.mp3";
			content.UserInfo ["404"] = "test User Info";


			var trigger = new SA.IOSNative.UserNotifications.TimeIntervalTrigger (5);
			var request = new SA.IOSNative.UserNotifications.NotificationRequest ("some0id0", content, trigger);
			ISN_Logger.Log("request Schedule for 5 sec");
			SA.IOSNative.UserNotifications.NotificationCenter.AddNotificationRequest (request, (result) => {
				ISN_Logger.Log("request callback");
				ISN_Logger.Log(result.ToString());
			});
		}

		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Schedule by Calendar - Date Components")) {

			var content = new SA.IOSNative.UserNotifications.NotificationContent ();
			content.Title = "Calendar - Date Components";
			content.Subtitle = "Subtitle_";
			content.Body = "Body_";
			content.Badge = 1;
			content.UserInfo ["404"] = "test User Info";

			var dateComponents = new SA.IOSNative.UserNotifications.DateComponents () {
				Second = 32,
					// 			You can use any of this fields or their combination; In this example, trigger will fire every 32nd second 

	//			public int? Year;
	//			public int? Month;
	//			public int? Day;
	//			public int? Hour;
	//			public int? Minute;
	//			public int? Second;
	//			public int? Weekday; (1 - Sunday, 7 - Saturday)
	//			public int? Quarter;
			};

			var trigger = new SA.IOSNative.UserNotifications.CalendarTrigger (dateComponents);
			trigger.SetRepeat (true);
			var request = new SA.IOSNative.UserNotifications.NotificationRequest ("some0id1", content, trigger);

			SA.IOSNative.UserNotifications.NotificationCenter.AddNotificationRequest (request, (result) => {
				ISN_Logger.Log("request callback");
				ISN_Logger.Log(result.ToString());
			});
		}

		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Schedule by Calendar - Date")) {

			var content = new SA.IOSNative.UserNotifications.NotificationContent ();
			content.Title = "Calendar - Date";
			content.Subtitle = "Subtitle_";
			content.Body = "Body_";
			content.Badge = 1;
			content.UserInfo ["404"] = 1;

			var dateComponents = new SA.IOSNative.UserNotifications.DateComponents () {
				Second = 32,
				Year = 2017,
				Month = 6,
				Day = 6,
				Hour = 13,
				Minute = 01
					// 			You can use any of this fields or their combination; In this example, trigger will fire every Friday, every 32nd second 

					//			public int? Year;
					//			public int? Month;
					//			public int? Day;
					//			public int? Hour;
					//			public int? Minute;
					//			public int? Second;
					//			public int? Weekday; (1 - Sunday, 7 - Saturday)
					//			public int? Quarter;
			};

			var trigger = new SA.IOSNative.UserNotifications.CalendarTrigger (dateComponents);
			var request = new SA.IOSNative.UserNotifications.NotificationRequest ("some0id2", content, trigger);

			SA.IOSNative.UserNotifications.NotificationCenter.AddNotificationRequest (request, (result) => {
				ISN_Logger.Log("request callback");
				ISN_Logger.Log(result.ToString());
			});
		}

		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Cancel All User Notifications")) {

			SA.IOSNative.UserNotifications.NotificationCenter.CancelAllNotifications ();
		}

		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Cancel some0id2")) {

			SA.IOSNative.UserNotifications.NotificationCenter.CancelUserNotificationById("some0id2");
		}

		StartX += XButtonStep;
		if(UnityEngine.GUI.Button(new UnityEngine.Rect(StartX, StartY, buttonWidth, buttonHeight), "Get Pending UserNotifications")) {

			SA.IOSNative.UserNotifications.NotificationCenter.GetPendingNotificationRequests(requests => {
				for (int i = 0; i < requests.Count; i++) {
					ISN_Logger.Log(requests[i].Content.Title);	
				}
			});
		}
		
		
	}

	public void CheckNotificationSettings() {

		int avaliableTypes = ISN_LocalNotificationsController.AllowedNotificationsType;
		Debug.Log ("AllowedNotificationsType: " + avaliableTypes);


		if((avaliableTypes & ISN_NotificationType.Sound) != 0) {
			Debug.Log ("Sound avaliable");
		}


		if((avaliableTypes & ISN_NotificationType.Badge) != 0) {
			Debug.Log ("Badge avaliable");
		}


		if((avaliableTypes & ISN_NotificationType.Alert) != 0) {
			Debug.Log ("Alert avaliable");
		}


	}
		
	
	//--------------------------------------
	//  GET/SET
	//--------------------------------------
	
	//--------------------------------------
	//  EVENTS
	//--------------------------------------


	void HandleOnLocalNotificationReceived (ISN_LocalNotification notification) {
		IOSMessage.Create("Notification Received", "Messgae: " + notification.Message + "\nNotification Data: " + notification.Data);
	}

	private void OnNotificationScheduleResult (SA.Common.Models.Result res) {
		ISN_LocalNotificationsController.OnNotificationScheduleResult -= OnNotificationScheduleResult;
		
		
		
		string msg = string.Empty;
		
		if(res.IsSucceeded) {
			msg += "Notification was successfully scheduled\n allowed notifications types: \n";

		} else {
			msg += "Notification scheduling failed";
		}
		
		
		IOSMessage.Create("On Notification Schedule Result", msg);
	}
	
	//--------------------------------------
	//  PRIVATE METHODS
	//--------------------------------------
	
	//--------------------------------------
	//  DESTROY
	//--------------------------------------
	
	
}
