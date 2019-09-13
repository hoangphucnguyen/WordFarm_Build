using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other) {
		GameController.Instance.WordTouched (other.gameObject);
	}
}
