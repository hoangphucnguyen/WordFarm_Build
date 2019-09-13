using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameFramework.Localisation.Components;

public class ResultItem {
    public string path;
    public Transform transform;
    public GameObject prefab;
}

public class ButtonsEditorScript : ScriptableObject
{
	[MenuItem("Tools/Our/Find Missing Audio")]
	static void FindMissingAudio()
	{
		string _search = "ButtonClickAudio";
		List<ResultItem> listResult = TraverseAllPrefabsForComponent(_search);

		Debug.Log(string.Format("Found prefab items '{0}': {1}", _search, listResult.Count));

		foreach (ResultItem trans in listResult)
		{
			ButtonClickAudio _comp = trans.transform.gameObject.GetComponent<ButtonClickAudio>();

            if (_comp != null && _comp.audioClip == null )
			{
				Debug.Log(trans.transform.gameObject.transform.parent.gameObject + " is missing AudioClip in " + trans.path);
			}
		}

		listResult = TraverseAllScenesForComponent(_search);

		Debug.Log(string.Format("Found scene items '{0}': {1}", _search, listResult.Count));

		foreach (ResultItem trans in listResult)
		{
            ButtonClickAudio _comp = trans.transform.gameObject.GetComponent<ButtonClickAudio>();

			if (_comp != null && _comp.audioClip == null)
			{
				Debug.Log(trans.transform.gameObject.transform.parent.gameObject + " is missing AudioClip in " + trans.path);
			}
		}

		EditorApplication.MarkSceneDirty();
	}

    [MenuItem("Tools/Our/Find Prefab Component")]
    static void FindPrefabComponent() {
        string _search = "Text";
        List<ResultItem> listResult = TraverseAllPrefabsForComponent(_search);

		Debug.Log(string.Format("Found prefab items '{0}': {1}", _search, listResult.Count));

		foreach (ResultItem trans in listResult)
		{
			Text text = trans.transform.gameObject.GetComponent<Text>();
            LocaliseText lText = trans.transform.gameObject.GetComponent<LocaliseText>();
            LocaliseTextFormat lFormatText = trans.transform.gameObject.GetComponent<LocaliseTextFormat>();

			if (text != null && (lText == null && lFormatText == null))
			{
                Debug.Log(trans.transform.gameObject.transform.parent.gameObject + " is missing Localisation in " + trans.path);
			}
		}
    }

	[MenuItem("Tools/Our/Change font")]
	static void ChangeFont()
	{
		List<ResultItem> listResult = TraverseAllPrefabsForComponent("Text");

		Debug.Log(string.Format("Found prefab items '{0}': {1}", "Text", listResult.Count));

		foreach (ResultItem trans in listResult)
		{
            Text text = trans.transform.gameObject.GetComponent<Text>();
			if ( text != null)
			{
                ChangeFontProcess(text, trans);
                EditorUtility.SetDirty(trans.prefab);
			}
		}
		
		AssetDatabase.SaveAssets();

		listResult = TraverseAllScenesForComponent("Text");

		Debug.Log(string.Format("Found scene items '{0}': {1}", "Text", listResult.Count));

		foreach (ResultItem trans in listResult)
		{
			Text text = trans.transform.gameObject.GetComponent<Text>();
			if (text != null)
			{
				ChangeFontProcess(text, trans);
			}
		}

		EditorApplication.MarkSceneDirty();
	}

    static void ChangeFontProcess(Text text, ResultItem trans) {
		Font oldFont = text.font;

		if (oldFont == null)
		{
			Debug.Log(trans.transform.gameObject + " missing font in " + trans.path);

			Font font = Resources.Load<Font>("Amaranth-Bold");

			if (font != null)
			{
				text.font = font;
			}
		}
		else
		{
			if (text.font.name.ToLower().Equals("Amaranth-Regular".ToLower())
				|| text.font.name.ToLower().Equals("Amaranth-Bold".ToLower()))
			{
                return;
			}

			if (text.font.name.ToLower().Equals("Saint Peter".ToLower())
				|| text.font.name.ToLower().Equals("Berlin Sans FB Regular".ToLower()))
			{
				Font font = Resources.Load<Font>("Amaranth-Regular");

				if (font != null)
				{
					text.font = font;
				}
			}

			if (text.font.name.ToLower().Equals("Berlin Sans FB Demi Bold".ToLower())
				|| text.font.name.ToLower().Equals("Intro".ToLower())
				|| text.font.name.ToLower().Equals("Arial".ToLower()))
			{
				Font font = Resources.Load<Font>("Amaranth-Bold");

				if (font != null)
				{
					text.font = font;
				}
			}

            if ( text.font.name.ToLower().Equals("Amaranth-Regular".ToLower()) 
                && text.fontStyle == FontStyle.Bold ) 
            {
				Font font = Resources.Load<Font>("Amaranth-Bold");

				if (font != null)
				{
					text.font = font;
				}
            }
		}

		Debug.Log(trans.transform.gameObject + " old font: " + (oldFont == null ? " missing" : oldFont.name) + "; new: " + text.font.name + " in " + trans.path);

	}

    [MenuItem("Tools/Our/Add button click audio")]
	static void AddButtonClickAudio()
	{
        List<ResultItem> listResult = TraverseAllPrefabsForComponent("Button");

		Debug.Log(string.Format("Found prefab items '{0}': {1}", "Button", listResult.Count));

        foreach (ResultItem trans in listResult)
        {
            if (trans.transform.gameObject.GetComponent<ButtonClickAudio>() == null)
        	{
                  Debug.Log(trans.transform.gameObject + " is missing ButtonClickAudio in " + trans.path);

                  ButtonClickAudio _audio = trans.transform.gameObject.AddComponent <ButtonClickAudio>();
                  _audio.audioClip = Resources.Load<AudioClip>("Audio/Button");

                  AssetDatabase.SaveAssets();
        	}
        }

        listResult = TraverseAllScenesForComponent("Button");

        Debug.Log(string.Format("Found scene items '{0}': {1}", "Button", listResult.Count));

        foreach (ResultItem trans in listResult)
        {
            if (trans.transform.gameObject.GetComponent<ButtonClickAudio>() == null) {
                Debug.Log(trans.transform.gameObject + " is missing ButtonClickAudio in " + trans.path);

				ButtonClickAudio _audio = trans.transform.gameObject.AddComponent<ButtonClickAudio>();
				_audio.audioClip = Resources.Load<AudioClip>("Audio/Button");
            }
        }

        EditorApplication.MarkSceneDirty();
	}

    [MenuItem("Tools/Our/Add button click delay")]
    static void AddButtonClickDelay() {
        List<ResultItem> listResult = TraverseAllPrefabsForComponent("Button");

        Debug.Log(string.Format("Found prefab items '{0}': {1}", "Button", listResult.Count));

        foreach (ResultItem trans in listResult)
        {
            if (trans.transform.gameObject.GetComponent<ButtonClickDelay>() == null)
            {
                Debug.Log(trans.transform.gameObject + " is missing ButtonClickDelay in " + trans.path);

                trans.transform.gameObject.AddComponent<ButtonClickDelay>();

                AssetDatabase.SaveAssets();
            }
        }

		listResult = TraverseAllScenesForComponent("Button");

		Debug.Log(string.Format("Found scene items '{0}': {1}", "Button", listResult.Count));

		foreach (ResultItem trans in listResult)
		{
			if (trans.transform.gameObject.GetComponent<ButtonClickDelay>() == null)
			{
				Debug.Log(trans.transform.gameObject + " is missing ButtonClickDelay in " + trans.path);

                trans.transform.gameObject.AddComponent<ButtonClickDelay>();
			}
		}

        EditorApplication.MarkSceneDirty();
    }

    static List<ResultItem> TraverseAllScenesForComponent(string component) {
        List<ResultItem> listResult = new List<ResultItem>();

        for (int n = 0; n < SceneManager.sceneCount; ++n)
        {
            Scene scene = SceneManager.GetSceneAt(n);
            GameObject[] _roots = scene.GetRootGameObjects();

            foreach ( GameObject _go in _roots ) {
                List<ResultItem> _found = RecursiveFind("Scene " + n, _go, _go, component);
                listResult.AddRange(_found);
            }
        }

        return listResult;
    }

    static List<ResultItem> TraverseAllPrefabsForComponent(string component) {
        string[] allPrefabs = SearchForComponents.GetAllPrefabs();

		List<ResultItem> listResult = new List<ResultItem>();
		foreach (string prefab in allPrefabs)
		{
			if (!prefab.Contains("Project"))
			{
				continue;
			}

			GameObject _prefab = AssetDatabase.LoadAssetAtPath(prefab, typeof(Object)) as GameObject;

			List<ResultItem> _found = RecursiveFind(prefab, _prefab, _prefab, component);

			listResult.AddRange(_found);
		}

        return listResult;
    }

    static List <ResultItem> RecursiveFind(string path, GameObject prefab, GameObject src, string component) {
        List<ResultItem> _found = new List<ResultItem>();

        Component src_components = src.GetComponent(component);

        if (src_components != null) {
            ResultItem _item = new ResultItem();
            _item.path = path;
            _item.transform = src.transform;
            _item.prefab = prefab;

            _found.Add(_item);
        }

        foreach (Transform child_transform in src.transform)
        {
            GameObject child = child_transform.gameObject;

            List<ResultItem> _f = RecursiveFind(path, prefab, child, component);

            if (_f.Count > 0)
            {
                _found.AddRange(_f);
            }
        }

        return _found;
    }
}
