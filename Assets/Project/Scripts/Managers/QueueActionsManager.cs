using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.GameObjects.Components;
using UnityEngine;

public class QueueActionsManager : Singleton<QueueActionsManager> {

    private Queue<Action> _queue = new Queue<Action>();

    public void AddAction(Action action) {
        _queue.Enqueue(action);
    }
	
	// Update is called once per frame
	void Update () {
        while( _queue.Count > 0 ) {
            Action _action = _queue.Dequeue();

            _action();
        }
	}
}
