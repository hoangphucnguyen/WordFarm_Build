using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomUI_ScrollRectOcclusion : MonoBehaviour {

	//if true user will need to call Init() method manually (in case the contend of the scrollview is generated from code or requires special initialization)
	public bool InitByUser = false;
	[SerializeField]
	private ScrollRect _scrollRect;
	private ContentSizeFitter _contentSizeFitter;
	private VerticalLayoutGroup _verticalLayoutGroup;
	private HorizontalLayoutGroup _horizontalLayoutGroup;
	private GridLayoutGroup _gridLayoutGroup;
	private bool _isVertical = false;
	private bool _isHorizontal = false;
	private float _disableMarginX = 0;
	private float _disableMarginY = 0;
	private bool hasDisabledGridComponents = false;
	private List<RectTransform> items = new List<RectTransform>();

	void Awake()
	{
		if (InitByUser)
			return;

		Init();

	}

	public void Init()
	{
		if (_scrollRect != null)
		{
			_scrollRect.onValueChanged.AddListener(OnScroll);

			_isHorizontal = _scrollRect.horizontal;
			_isVertical = _scrollRect.vertical;

			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				items.Add(gameObject.transform.GetChild(i).GetComponent<RectTransform>());
			}
			if (gameObject.transform.GetComponent<VerticalLayoutGroup>() != null)
			{
				_verticalLayoutGroup = gameObject.transform.GetComponent<VerticalLayoutGroup>();
			}
			if (gameObject.transform.GetComponent<HorizontalLayoutGroup>() != null)
			{
				_horizontalLayoutGroup = gameObject.transform.GetComponent<HorizontalLayoutGroup>();
			}
			if (gameObject.transform.GetComponent<GridLayoutGroup>() != null)
			{
				_gridLayoutGroup = gameObject.transform.GetComponent<GridLayoutGroup>();
			}
			if (gameObject.transform.GetComponent<ContentSizeFitter>() != null)
			{
				_contentSizeFitter = gameObject.transform.GetComponent<ContentSizeFitter>();
			}

		}
		else
		{
			Debug.LogError("CustomUI_ScrollRectOcclusion => No ScrollRect component found");
		}
	}

	void DisableGridComponents()
	{
		if (_isVertical)
			_disableMarginY = _scrollRect.GetComponent<RectTransform>().rect.height / 2 + items[0].sizeDelta.y;

		if (_isHorizontal)
			_disableMarginX = _scrollRect.GetComponent<RectTransform>().rect.width / 2 + items[0].sizeDelta.x;

		if (_verticalLayoutGroup)
		{
			_verticalLayoutGroup.enabled = false;
		}
		if (_horizontalLayoutGroup)
		{
			_horizontalLayoutGroup.enabled = false;
		}
		if (_contentSizeFitter)
		{
			_contentSizeFitter.enabled = false;
		}
		if (_gridLayoutGroup)
		{
			_gridLayoutGroup.enabled = false;
		}
		hasDisabledGridComponents = true;
	}

	public void OnScroll(Vector2 pos)
	{

		if (!hasDisabledGridComponents)
			DisableGridComponents();

		for (int i = 0; i < items.Count; i++)
		{
			if (_isVertical && _isHorizontal)
			{
				if (_scrollRect.transform.InverseTransformPoint(items[i].position).y < -_disableMarginY || _scrollRect.transform.InverseTransformPoint(items[i].position).y > _disableMarginY
				|| _scrollRect.transform.InverseTransformPoint(items[i].position).x < -_disableMarginX || _scrollRect.transform.InverseTransformPoint(items[i].position).x > _disableMarginX)
				{
					items[i].gameObject.SetActive(false);
				}
				else
				{
					items[i].gameObject.SetActive(true);
				}
			}
			else
			{
				if (_isVertical)
				{
					if (_scrollRect.transform.InverseTransformPoint(items[i].position).y < -_disableMarginY || _scrollRect.transform.InverseTransformPoint(items[i].position).y > _disableMarginY)
					{
						items[i].gameObject.SetActive(false);
					}
					else
					{
						items[i].gameObject.SetActive(true);
					}
				}

				if (_isHorizontal)
				{
					if (_scrollRect.transform.InverseTransformPoint(items[i].position).x < -_disableMarginX || _scrollRect.transform.InverseTransformPoint(items[i].position).x > _disableMarginX)
					{
						items[i].gameObject.SetActive(false);
					}
					else
					{
						items[i].gameObject.SetActive(true);
					}
				}
			}
		}
	}
}
