using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;

namespace iSDK.Messenger.Scripts {
	
	[RequireComponent(typeof(Button))]
	[AddComponentMenu("iSDK/Facebook/OnButtonClickMessenger")]
	public class OnButtonClickMessenger : MonoBehaviour
	{
		public string photoUrl;

		void Start()
		{
			gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
		}

		void OnClick()
		{
			#if FACEBOOK_SDK
			StartCoroutine (TakeScreenShot ());
			#else
			Debug.Log("OnButtonClickShareLink only works if you enable and setup the Facebook SDK");
			#endif
		}

		string fileName(int width, int height)
		{
			return string.Format("screen_{0}x{1}_{2}.png",
				width, height,
				System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}

		public IEnumerator TakeScreenShot()
		{
			yield return new WaitForEndOfFrame();

			GameObject OVcamera = GameObject.FindGameObjectWithTag("MainCamera");
			Camera camOV = OVcamera.GetComponent<Camera>();

			RenderTexture currentRT = RenderTexture.active;

			RenderTexture.active = camOV.targetTexture;
			camOV.Render();
			Texture2D imageOverview = new Texture2D(500, 500, TextureFormat.RGB24, false);
			imageOverview.ReadPixels(new Rect(0, 0, 500, 500), 0, 0);
			imageOverview.Apply();
			RenderTexture.active = currentRT;


			// Encode texture into PNG
			byte[] bytes = imageOverview.EncodeToPNG();

			// save in memory
			string filename = fileName(Convert.ToInt32(imageOverview.width), Convert.ToInt32(imageOverview.height));
			string path = Application.persistentDataPath + "/" + filename;

			System.IO.File.WriteAllBytes(path, bytes);

			MessengerShare.Instance.ShareDialog(null, new Uri(path), "Share score", "Hey, view my score :)");
		}
	}

}
