using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Helper;
using System.IO;

public class JSONPrefs {
	static JSONObject jsonObject;
	static string path = Application.persistentDataPath + "/playerprefs.json";

	static JSONPrefs () {
		if ( !File.Exists (path) ) {
			using (StreamWriter sw = new StreamWriter(path)) {
				sw.Write("{}"); // empty
				sw.Close ();
			}
		}

		Load ();
	}

	static void Load() {
		string dataAsJson = File.ReadAllText(path);

		jsonObject = JSONObject.Parse(dataAsJson);
	}

	public static string Path () {
		return path;
	}

	public static string String() {
		return jsonObject.ToString ();
	}

	public static void Replace(string json) {
		jsonObject = JSONObject.Parse(json);
	}

	public static void DeleteAll () {
		jsonObject.Clear ();
	}

	public static void DeleteKey (string key) {
		jsonObject.Remove (key);
	}

	public static float GetFloat (string key) {
		return GetFloat (key, 0.0f);
	}

	public static float GetFloat (string key, float defaultValue) {
		if ( !HasKey (key) ) {
			return defaultValue;
		}

		return (float)jsonObject.GetNumber (key);
	}

	public static int GetInt (string key) {
		return GetInt (key, 0);
	}

	public static int GetInt (string key, int defaultValue) {
		if ( !HasKey (key) ) {
			return defaultValue;
		}

		return (int)jsonObject.GetNumber (key);
	}

	public static string GetString (string key) {
		return GetString (key, "");
	}

	public static string GetString (string key, string defaultValue) {
		if ( !HasKey (key) ) {
			return defaultValue;
		}

		return jsonObject.GetString (key);
	}

	public static bool HasKey (string key) {
        if ( jsonObject == null ) {
            return false;
        }

		return jsonObject.ContainsKey (key);
	}

	public static void Save () {
		using (StreamWriter sw = new StreamWriter (path)) {
			sw.Write(jsonObject);
			sw.Close ();
		}
	}

	public static void SetFloat (string key, float value) {
		jsonObject.Add (key, new JSONValue(value));
	}

	public static void SetInt (string key, int value) {
		jsonObject.Add (key, new JSONValue(value));
	}

	public static void SetString (string key, string value) {
		jsonObject.Add (key, new JSONValue(value));
	}
}
