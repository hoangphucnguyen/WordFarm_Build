using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ListUtils {
	public static List<T> Shuffle<T>(List<T> list) {
		List<T> ts = new List <T> (list);

		var count = ts.Count;
		var last = count - 1;
		for (var i = 0; i < last; ++i) {
			var r = UnityEngine.Random.Range(i, count);
			var tmp = ts[i];
			ts[i] = ts[r];
			ts[r] = tmp;
		}

		return ts;
	}
}
