using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Utils {
	public static Vector3 RandomCircle ( Vector3 center, float radius, int angle, int index = 0){
		Vector3 pos;

		pos.x = center.x + radius * Mathf.Sin((angle*index) * Mathf.Deg2Rad);
		pos.y = center.y + radius * Mathf.Cos((angle*index) * Mathf.Deg2Rad);
		pos.z = center.z;

		return pos;
	}

	public static Vector3 StringToVector3(string sVector)
	{
		// Remove the parentheses
		if (sVector.StartsWith("(") && sVector.EndsWith(")"))
		{
			sVector = sVector.Substring(1, sVector.Length - 2);
		}

		// split the items
		string[] sArray = sVector.Split(',');

		// store as a Vector3
		Vector3 result = new Vector3(
			float.Parse(sArray[0]),
			float.Parse(sArray[1]),
			float.Parse(sArray[2]));

		return result;
	}
}
