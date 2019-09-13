using UnityEngine;
using System.Collections;
using GameFramework.Localisation;

public class GeneralUtils
{
    public static float ScreenAspectRatio() {
        if ( Input.deviceOrientation == DeviceOrientation.LandscapeLeft 
             || Input.deviceOrientation == DeviceOrientation.LandscapeRight )
        {
            return (float)Screen.width / (float)Screen.height;
        } else {
            return (float)Screen.height / (float)Screen.width;
        }
    }

    public static bool IsRequiredAspectRatio (float requiredAspect) {
        float deviceAspectRatio = ScreenAspectRatio();

		if (deviceAspectRatio > requiredAspect - 0.01 && deviceAspectRatio < requiredAspect + 0.01)
		{
            return true;
		}

        return false;
    }

    public static Font FontForCurrentLanguage(FontStyle fontStyle) {
        Font font;

        switch (LanguageUtils.RealLanguage(LocaliseText.Language)) {
            case "Bulgarian":
            case "Russian":
            case "Serbian":
                if (fontStyle == FontStyle.Bold || fontStyle == FontStyle.BoldAndItalic)
                {
                    font = Resources.Load<Font>("RifficFree-Bold");
                } else {
                    font = Resources.Load<Font>("RifficFree-Bold");
                }
            break;

            default:
                if (fontStyle == FontStyle.Bold || fontStyle == FontStyle.BoldAndItalic)
                {
                    font = Resources.Load<Font>("Amaranth-Bold");
                } else {
                    font = Resources.Load<Font>("Amaranth-Regular");
                }
                break;
        }

        return font;
    }
}
