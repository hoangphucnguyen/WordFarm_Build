using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Xml;

public class iOSLanguagePostProcessor {
    static string[] kProjectLocalizations = { "en", "bg_BG", "ru", "fr", "it", "pt_PT", "pt_BR", "de", "es", "ms", "ko", "ja", "zh_Hans", "hr", "sk", "sr", "sr_ME" };

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			string infoPList = System.IO.Path.Combine(path, "Info.plist");

			if (File.Exists(infoPList) == false)
			{
				Debug.LogError("Could not add localizations to Info.plist file.");
				return;
			}

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(infoPList);

			XmlNode pListDictionary = xmlDocument.SelectSingleNode("plist/dict");
			if (pListDictionary == null)
			{
				Debug.LogError("Could not add localizations to Info.plist file.");
				return;
			}

			XmlElement localizationsKey = xmlDocument.CreateElement("key");
			localizationsKey.InnerText = "CFBundleLocalizations";
			pListDictionary.AppendChild(localizationsKey);

			XmlElement localizationsArray = xmlDocument.CreateElement("array");
			foreach (string localization in kProjectLocalizations)
			{
				XmlElement localizationElement = xmlDocument.CreateElement("string");
				localizationElement.InnerText = localization;
				localizationsArray.AppendChild(localizationElement);
			}

			pListDictionary.AppendChild(localizationsArray);

			xmlDocument.Save(infoPList);
		}
	}
}
