using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageUtils {

	public static IEnumerator LoadImage(string url, RawImage image) {
		var www = new WWW(url);

		yield return www;

		if (www.texture != null)
		{
			image.texture = www.texture;
		}
	}
}
