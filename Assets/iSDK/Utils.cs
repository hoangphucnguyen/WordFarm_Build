using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace iSDK {
	public class Utils {

		public static int AndroidSDKVersion() {
			#if UNITY_EDITOR
			return 26;
			#endif

			using (var version = new AndroidJavaClass("android.os.Build$VERSION")) {
				return version.GetStatic<int>("SDK_INT");
			}

			return -1;
		}

		public static void OpenSettings() {
			#if !UNITY_EDITOR && UNITY_IOS
			_openSettings();
			#endif
		}

		[DllImport("__Internal")]
		extern static private void _openSettings ();
	}
}
