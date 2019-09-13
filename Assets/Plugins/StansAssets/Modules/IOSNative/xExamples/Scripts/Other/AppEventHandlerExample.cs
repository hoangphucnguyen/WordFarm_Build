////////////////////////////////////////////////////////////////////////////////
//  
// @module IOS Native Plugin for Unity3D 
// @author Osipov Stanislav (Stan's Assets) 
// @support stans.assets@gmail.com 
//
////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using SA.IOSNative.Core;


public class AppEventHandlerExample : MonoBehaviour {

	//--------------------------------------
	// INITIALIZE
	//--------------------------------------

	void Awake() {

		//Action use example

		AppController.Subscribe();
		AppController.OnApplicationDidReceiveMemoryWarning += OnApplicationDidReceiveMemoryWarning;
		AppController.OnApplicationDidBecomeActive += HandleOnApplicationDidBecomeActive;


		SA.IOSNative.Core.AppController.OnOpenURL += OnOpenURL;
		SA.IOSNative.Core.AppController.OnContinueUserActivity += OnContinueUserActivity;

		Invoke ("TryDetectURL", 2f);





	}

	private void TryDetectURL() {
		var url = SA.IOSNative.Core.AppController.LaunchUrl;
		if(!url.IsEmpty) {
			IOSMessage.Create("Open URL Detecetd", url.AbsoluteUrl);
		}


		var link = SA.IOSNative.Core.AppController.LaunchUniversalLink;
		if(!link.IsEmpty) {
			IOSMessage.Create("Launch Universal Link Detecetd", link.AbsoluteUrl);
		}



	}

	void OnContinueUserActivity (SA.IOSNative.Models.UniversalLink link) {
		IOSMessage.Create("Universal Link Detecetd", link.AbsoluteUrl);
	} 

	void OnDestroy() {
		SA.IOSNative.Core.AppController.OnApplicationDidReceiveMemoryWarning -= OnApplicationDidReceiveMemoryWarning;
		SA.IOSNative.Core.AppController.OnApplicationDidBecomeActive -= HandleOnApplicationDidBecomeActive;


		SA.IOSNative.Core.AppController.OnOpenURL -= OnOpenURL;
	}


	//--------------------------------------
	// EVENTS
	//--------------------------------------

	void OnOpenURL(SA.IOSNative.Models.LaunchUrl url) {
		IOSMessage.Create("Open URL Detecetd", url.AbsoluteUrl);
	}


	void HandleOnApplicationDidBecomeActive () {
		ISN_Logger.Log("Caught OnApplicationDidBecomeActive event");
	}


	private void OnApplicationDidReceiveMemoryWarning() {
		//Called when the application receives a memory warning from the system.

		ISN_Logger.Log ("Caught OnApplicationDidReceiveMemoryWarning event");
	}


	//--------------------------------------
	// PRIVATE METHODS
	//--------------------------------------
	
	//--------------------------------------
	// DESTROY
	//--------------------------------------
}
