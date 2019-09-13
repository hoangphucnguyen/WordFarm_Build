using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class CustomVerticalScrollSnap : VerticalScrollSnap {

	public new void AddChild(GameObject GO)
	{
		GO.transform.SetParent(_screensContainer, false); // manual set GO to parent to update ChildObjects[] else crash
		UpdateChild ();
		AddChild (GO, false);
	}

	private void UpdateChild() {
		int childCount = _screensContainer.childCount;
		base.ChildObjects = new GameObject[childCount];
		for (int i = 0; i < childCount; i++)
		{
			ChildObjects[i] = _screensContainer.transform.GetChild(i).gameObject;
			if (MaskArea && ChildObjects[i].activeSelf)
			{
				ChildObjects[i].SetActive(false);
			}
		}
	}
}
