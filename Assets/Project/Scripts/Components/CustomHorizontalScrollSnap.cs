using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Extensions;

public class CustomHorizontalScrollSnap : HorizontalScrollSnap {

    [Serializable]
    public class ScrollSnapReady : UnityEvent { }

    [SerializeField]
    [Tooltip("Event fires when scroll snap is ready")]
    private ScrollSnapReady m_OnScrollSnapReadyEvent = new ScrollSnapReady();
    public ScrollSnapReady OnScrollSnapReadyEvent { get { return m_OnScrollSnapReadyEvent; } set { m_OnScrollSnapReadyEvent = value; } }


    void Start()
    {
        _isVertical = false;
        _childAnchorPoint = new Vector2(0, 1f);
        _currentPage = StartingScreen;
        UpdateLayout();

        OnScrollSnapReadyEvent.Invoke();
    }

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
