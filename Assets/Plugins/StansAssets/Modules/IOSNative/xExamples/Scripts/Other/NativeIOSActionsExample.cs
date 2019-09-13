using UnityEngine;
using System.Collections;
using System;

using SA.IOSNative.UIKit;
using SA.IOSNative.Privacy;

public class NativeIOSActionsExample : BaseIOSFeaturePreview {

	public Texture2D hello_texture;
	public Texture2D drawTexture = null;
	private DateTime time;

	void Awake() {
		time = new DateTime (1997, 05, 11);

//		IOSNative.UIKit.DateTimePicker.OnDateChanged += OnDateChanged;
//		IOSNative.UIKit.DateTimePicker.OnDateChanged += OnPickerClosed;
//		IOSDateTimePicker.Instance.OnDateChanged 
//		IOSDateTimePicker.Instance.OnPickerClosed 


		Debug.Log("Subscribed");

		ISN_GestureRecognizer.Instance.OnSwipe +=  delegate(ISN_SwipeDirection direction) {
			Debug.Log("Swipe: " + direction);
		};


		SA.IOSNative.Gestures.ForceTouch.Instance.Setup (0.5f, 1f, 2.5f);

		SA.IOSNative.Gestures.ForceTouch.Instance.OnForceTouchStarted += () => {
			Debug.Log ("OnForceTouchStarted");
		};

		SA.IOSNative.Gestures.ForceTouch.Instance.OnForceChanged += (SA.IOSNative.Gestures.ForceInfo info) => {
			Debug.Log ("OnForceChanged: " + info.Force + " / " + info.MaxForce);
		};

		SA.IOSNative.Gestures.ForceTouch.Instance.OnForceTouchFinished += () => {
			Debug.Log ("OnForceTouchFinished");
		};



		Debug.Log (SA.IOSNative.Gestures.ForceTouch.AppOpenshortcutItem);

		SA.IOSNative.Gestures.ForceTouch.Instance.OnAppShortcutClick += (string action) => {
			Debug.Log ("Menu Item With action: " + action + " was clicked");
		};
			
/*
		SA.IOSNative.Gestures.ForceTouch.OnForceChanged += (info) {
			Debug.Log("Force: " + info.Force);
		};*/
	}



	void OnGUI() {
		UpdateToStartPos();


		GUI.Label(new Rect(StartX, StartY, Screen.width, 40), "System Utils", style);

		//SA.IOSNative.

		StartY+= YLableStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Check if FB App exists")) {
			bool result = SA.IOSNative.System.SharedApplication.CheckUrl("fb://");

			if(result) {
				IOSMessage.Create("Success", "Facebook App is installed on current device" );
			} else {
				IOSMessage.Create("ERROR", "Facebook App is not installed on current device");
			}
		}
		
		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Open FB Profile")) {
			SA.IOSNative.System.SharedApplication.OpenUrl("fb://profile");
		}
		
		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Open App Store")) {
			SA.IOSNative.System.SharedApplication.OpenUrl("itms-apps://");
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Get IFA")) {

			IOSMessage.Create("Identifier Loaded", ISN_Device.CurrentDevice.AdvertisingIdentifier);
		}

		StartX = XStartPos;
		StartY+= YButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Set App Bages Count")) {
			IOSNativeUtility.SetApplicationBagesNumber(10);
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Clear Application Bages")) {
			IOSNativeUtility.SetApplicationBagesNumber(0);
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Show Device Info")) {
			ShowDevoceInfo();
		}


		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;

		if (GUI.Button (new Rect (StartX, StartY, buttonWidth, buttonHeight), "Pick Contacts UI")) {
			SA.IOSNative.Contacts.ContactStore.Instance.ShowContactsPickerUI ((result) => {

				if(result.IsSucceeded) {
					Debug.Log("Picked " + result.Contacts.Count + " contacts");
					IOSMessage.Create("Success", "Picked " + result.Contacts.Count + " contacts");

					foreach(var contact in result.Contacts) {
					
						Debug.Log ("contact.GivenName: " + contact.GivenName );
						if(contact.PhoneNumbers.Count > 0) {
							Debug.Log("contact.PhoneNumber: " + contact.PhoneNumbers[0].Digits);
						}

						if(contact.Emails.Count > 0) {
							Debug.Log("contact.Email: " + contact.Emails[0]);
						}
					}

				} else {
					IOSMessage.Create("Error", result.Error.Code + " / " + result.Error.Message);
				}

			});
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Load Contacts")) {
			SA.IOSNative.Contacts.ContactStore.Instance.RetrievePhoneContacts((result) => {

				if(result.IsSucceeded) {
					Debug.Log("Loaded " + result.Contacts.Count + " contacts");
					IOSMessage.Create("Success", "Loaded " + result.Contacts.Count + " contacts");

					foreach(var contact in result.Contacts) {
						if(contact.PhoneNumbers.Count > 0) {
							Debug.Log(contact.GivenName + " / " + contact.PhoneNumbers[0].Digits);
						}

					}

				} else {
					IOSMessage.Create("Error", result.Error.Code + " / " + result.Error.Message);
				}

			});
		}



		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;
		
		
		
		GUI.Label(new Rect(StartX, StartY, Screen.width, 40), "Date Time Picker", style);

		StartY+= YLableStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Time")) {
			DateTimePicker.Show(DateTimePickerMode.Time, (dateTime) => {

			});
//			IOSDateTimePicker.Instance.Show
		}


		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Date")) {
			DateTimePicker.Show(DateTimePickerMode.Date, (dateTime) => {

			});
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Date And Time")) {
			DateTimePicker.Show(DateTimePickerMode.DateAndTime, (dateTime) => {

			});
//			IOSDateTimePicker.Instance.Show(IOSDateTimePickerMode.DateAndTime);
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Countdown Timer")) {
			DateTimePicker.Show(DateTimePickerMode.CountdownTimer, (dateTime) => {

			});
		}

		StartX = XStartPos;
		StartY+= YButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Set Date Without UI")) {
			DateTimePicker.Show(DateTimePickerMode.Date, time, (dateTime) => {

			});
		}

		StartX = XStartPos;
		StartY+= YButtonStep;

		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Show Calendar Picker")) {
			Calendar.PickDate ((dateTime) => {
				ISN_Logger.Log("OnDateChanged: " + dateTime.ToShortDateString());
			}, 1989);

			SA.IOSNative.Privacy.PermissionsManager.RequestPermission(Permission.NSPhotoLibrary, (permissionStatus) => {
				Debug.Log("PermissionsManager.RequestPermission: " + Permission.NSPhotoLibrary + permissionStatus.ToString());
			});

			SA.IOSNative.Privacy.PermissionsManager.RequestPermission(Permission.NSLocationWhenInUse, (permissionStatus) => {
				Debug.Log("PermissionsManager.RequestPermission: " + Permission.NSLocationWhenInUse + permissionStatus.ToString());
			});

			SA.IOSNative.Privacy.PermissionsManager.RequestPermission(Permission.NSMicrophone, (permissionStatus) => {
				Debug.Log("PermissionsManager.RequestPermission: " + Permission.NSMicrophone + permissionStatus.ToString());
			});



		}


		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;





		GUI.Label(new Rect(StartX, StartY, Screen.width, 40), "Video", style);
		
		StartY+= YLableStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Player Streamed video")) {
			IOSVideoManager.Instance.PlayStreamingVideo("https://dl.dropboxusercontent.com/u/83133800/Important/Doosan/GT2100-Video.mov");
		}
		
		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Open YouTube Video")) {
			IOSVideoManager.Instance.OpenYouTubeVideo("xzCEdSKMkdU");
		}


		
		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;



		
		GUI.Label(new Rect(StartX, StartY, Screen.width, 40), "Camera Roll", style);
		
		StartY+= YLableStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth + 10, buttonHeight), "Save Screenshot To Camera Roll")) {
			IOSCamera.OnImageSaved += OnImageSaved;
			IOSCamera.Instance.SaveScreenshotToCameraRoll();
		}


		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Save Texture To Camera Roll")) {
			IOSCamera.OnImageSaved += OnImageSaved;
			IOSCamera.Instance.SaveTextureToCameraRoll(hello_texture);
		}


		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Get Image From Camera")) {
			IOSCamera.OnImagePicked += OnImage;
			IOSCamera.Instance.PickImage(ISN_ImageSource.Camera);
			
		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Get Image From Album")) {
			IOSCamera.OnImagePicked += OnImage;
			IOSCamera.Instance.PickImage(ISN_ImageSource.Album);

		}

		StartX += XButtonStep;
		if(GUI.Button(new Rect(StartX, StartY, buttonWidth, buttonHeight), "Load Multiple Images")) {
	

			ISN_FilePicker.MediaPickFinished += delegate(ISN_FilePickerResult res) {
				Debug.Log("Picked " + res.PickedImages.Count + " images");

				if(res.PickedImages.Count == 0) return;
				//destroying old texture
				Destroy(drawTexture);

				//applaying new texture
				drawTexture = res.PickedImages[0];
			};
			ISN_FilePicker.Instance.PickFromCameraRoll();

		}



		StartX = XStartPos;
		StartY+= YButtonStep;
		StartY+= YLableStep;
		GUI.Label(new Rect(StartX, StartY, Screen.width, 40), "PickedImage", style);
		StartY+= YLableStep;

		if(drawTexture != null) {
			GUI.DrawTexture(new Rect(StartX, StartY, buttonWidth, buttonWidth), drawTexture);
		}
	

	}

	private void ShowDevoceInfo() {
		ISN_Device device = ISN_Device.CurrentDevice;

		IOSMessage.Create("Device Info", "Name: "  + device.Name + "\n"
		                  + "System Name: " + device.SystemName + "\n"
		                  + "Model: " + device.Model + "\n"
		                  + "Localized Model: " + device.LocalizedModel + "\n"
		                  + "System Version: " + device.SystemVersion + "\n"
		                  + "Major System Version: " + device.MajorSystemVersion + "\n"
						  + "Preferred Language Code: " + device.PreferredLanguageCode + "\n"
						  + "Preferred Language_ISO639_1: " + device.PreferredLanguage_ISO639_1 + "\n"
		                  + "User Interface Idiom: " + device.InterfaceIdiom + "\n"
		                  + "GUID in Base64: " + device.GUID.Base64String  );

		Debug.Log ("ISN_TimeZone.LocalTimeZone.Name: " + ISN_TimeZone.LocalTimeZone.Name);
		Debug.Log ("ISN_TimeZone.LocalTimeZone.SecondsFromGMT: " + ISN_TimeZone.LocalTimeZone.SecondsFromGMT);

		Debug.Log ("ISN_TimeZone.LocalTimeZone.Name: " + ISN_Build.Current.Version);
		Debug.Log ("ISN_TimeZone.LocalTimeZone.Name: " + ISN_Build.Current.Number);
	
	}

	void OnDateChanged (System.DateTime time) {
		ISN_Logger.Log("OnDateChanged: " + time.ToString());
	}

	void OnPickerClosed (System.DateTime time) {
		ISN_Logger.Log("OnPickerClosed: " + time.ToString());
	}

	private void OnImage (IOSImagePickResult result) {
		if(result.IsSucceeded) {

			//destroying old texture
			Destroy(drawTexture);

			//applaying new texture
			drawTexture = result.Image;
			IOSMessage.Create("Success", "Image Successfully Loaded, Image size: " + result.Image.width + "x" + result.Image.height);
		} else {
			IOSMessage.Create("ERROR", "Image Load Failed");
		}

		IOSCamera.OnImagePicked -= OnImage;
	}

	private void OnImageSaved (SA.Common.Models.Result result) {
		IOSCamera.OnImageSaved -= OnImageSaved;
		if(result.IsSucceeded) {
			IOSMessage.Create("Success", "Image Successfully saved to Camera Roll");
		} else {
			IOSMessage.Create("ERROR", "Image Save Failed");
		}
	}

}
