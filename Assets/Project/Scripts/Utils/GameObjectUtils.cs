using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

internal class GameObjectUtils {

	public static int ClosestAngle(float angle) {
		List<int> angles = new List<int>{ 0, 45, 90, 135, 180, 225, 270, 315, 360};

		return angles.Aggregate((x,y) => Math.Abs(x-angle) < Math.Abs(y-angle) ? x : y);
	}

    public static void MoveObjectToAtIndex(GameObject obj, GameObject to, int childIndex = -1)
    {
        MoveObjectTo(obj, to);

        if (childIndex > -1)
        {
            obj.transform.SetSiblingIndex(childIndex);
        }
    }

    public static void MoveObjectTo(GameObject obj, GameObject to) {
		Vector2 o = RectTransformUtility.WorldToScreenPoint (Camera.main, obj.transform.position);

		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (to.transform as RectTransform, 
			o, 
			null, 
			out localPoint);

		obj.transform.SetParent (to.transform);
		obj.transform.localScale = new Vector3(1, 1, 1);
		obj.transform.localPosition = localPoint;
	}

    public static void MoveObjectTo(GameObject obj, GameObject to, Vector3 position = default(Vector3)) {		
		obj.transform.SetParent (to.transform);
		obj.transform.localScale = new Vector3(1, 1, 1);
		obj.transform.position = position;
	}

	public static GameObject[] GetChildWithComponentGameObject<TComponent>(GameObject thisGameObject, bool includeInactive = false)
	{
		Transform[] components = thisGameObject.GetComponentsInChildren<Transform>(includeInactive);

		List<GameObject> c = new List<GameObject>();

		foreach (Transform component in components)
		{
			if (component.GetComponent<TComponent>() != null)
			{
				c.Add(component.gameObject);
			}
		}

		return c.ToArray();
	}

	public static GameObject[] GetChildWithTagGameObject(GameObject thisGameObject, string tag, bool includeInactive = false)
	{
		Transform[] components = thisGameObject.GetComponentsInChildren<Transform>(includeInactive);

		List <GameObject> c = new List<GameObject> ();

		foreach ( Transform component in components ) {
			if ( component.gameObject.tag.Equals(tag) ) {
				c.Add(component.gameObject);
			}
		}

		return c.ToArray();
	}

	public static GameObject[] GetChildWithNameGameObject(GameObject thisGameObject, string name, bool includeInactive = false)
	{
		Transform[] components = thisGameObject.GetComponentsInChildren<Transform>(includeInactive);

		List <GameObject> c = new List<GameObject> ();

		foreach ( Transform component in components ) {
			if ( component.gameObject.name.Equals(name) ) {
				c.Add(component.gameObject);
			}
		}

		return c.ToArray();
	}

	public static GameObject GetParentWithComponentNamedGameObject<TComponent>(GameObject thisGameObject)
	{
		while (true)
		{
			if (thisGameObject.GetComponent <TComponent>() != null) return thisGameObject;
			if (thisGameObject.transform.parent == null) return null;
			thisGameObject = thisGameObject.transform.parent.gameObject;
		}
	}

	public static void MakeHidden(GameObject thisGameObject, bool hidden) {

		float scale = 0.0f;

		if ( !hidden ) {
			scale = 1.0f;
		}

		thisGameObject.transform.localScale = new Vector3(scale, scale, scale);
	}
}
