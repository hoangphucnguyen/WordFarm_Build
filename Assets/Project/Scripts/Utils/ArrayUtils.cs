using UnityEngine;
using System.Collections;

public class ArrayUtils
{
	public static void Shuffle<T>(T[] array)
	{
		System.Random _random = new System.Random();

		int n = array.Length;
		for (int i = 0; i < n; i++)
		{
			// NextDouble returns a random number between 0 and 1.
			// ... It is equivalent to Math.random() in Java.
			int r = i + (int)(_random.NextDouble() * (n - i));
			T t = array[r];
			array[r] = array[i];
			array[i] = t;
		}
	}
}
