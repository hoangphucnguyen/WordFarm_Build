using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;

public class iOSPostProcessor {
	// https://gonzaloquero.com/blog/2015/04/06/postprocessing-scripts/

	[PostProcessBuild(10)] // We should try to run last
	public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
	{
		#if UNITY_IPHONE
		if (buildTarget != BuildTarget.iOS) {
			return;
		}

		string _projectPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

		PBXProject _project = new PBXProject ();
		_project.ReadFromFile (_projectPath);

		string _target = _project.TargetGuidByName ("Unity-iPhone");

		_project.SetBuildProperty (_target, "ENABLE_BITCODE", "NO");
        _project.SetBuildProperty(_target, "CLANG_ENABLE_MODULES", "YES");

        _project.AddCapability(_target, PBXCapabilityType.InAppPurchase);
        _project.AddCapability(_target, PBXCapabilityType.PushNotifications);

		_project.WriteToFile (_projectPath);

		UnityEngine.Debug.Log("iOSPostProcessor: Disable BITCODE");
        UnityEngine.Debug.Log("iOSPostProcessor: Enable CLANG MODULES");

		// Get plist
		string plistPath = buildPath + "/Info.plist";
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));

		// Get root
		PlistElementDict rootDict = plist.root;

		// Change value of CFBundleVersion in Xcode plist
		var buildKey = "UIBackgroundModes";
		rootDict.CreateArray (buildKey).AddString ("remote-notification");

		UnityEngine.Debug.Log("iOSPostProcessor: Enable background modes: remote-notification");

        rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        UnityEngine.Debug.Log("iOSPostProcessor: Set Export compliance status");

		rootDict.SetString ("NSCalendarsUsageDescription", "App using calendar");

		UnityEngine.Debug.Log("iOSPostProcessor: Set NSCalendarsUsageDescription");

		// Write to file
		File.WriteAllText(plistPath, plist.WriteToString());

		#endif
	}
}
