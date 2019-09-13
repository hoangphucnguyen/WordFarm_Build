﻿using System.Collections;
using System.Collections.Generic;
using GameFramework.Display.Other;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonClickDelay : MonoBehaviour {
    private Button _button;
    private Button.ButtonClickedEvent _originalButtonEvent;
    private Button.ButtonClickedEvent _buttonEvent;
    private bool _disabled = false;
    private Animator _animator;

	void Start () {
        _button = GetComponent<Button>();
        _animator = GetComponent<Animator>();

        // add delay because when button is Instantiated from prefab there is delay to add listeners
        StartCoroutine(CoRoutines.DelayedCallback(0.5f, () =>
        {
            int _persistentCount = _button.onClick.GetPersistentEventCount();

            // first disable all persistent clicks
            for (int i = 0; i < _persistentCount; i++)
            {
                _button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            _originalButtonEvent = _button.onClick;

            _buttonEvent = new Button.ButtonClickedEvent();
            _buttonEvent.AddListener(Click);

            _button.onClick = _buttonEvent;
        }));
	}
	
	void Click () {
        if ( _disabled ) {
            return;
        }

        _disabled = true;

        if ( _animator != null ) {
            if (Constants.DelayButtonClickAction > 0.05f)
            {
                StartCoroutine(CoRoutines.DelayedCallback(Constants.DelayButtonClickAction, ClickAction));
            } else {
                ClickAction();
            }
        } else {
            ClickAction();
        }
	}

    void ClickAction() {
		_button.onClick = _originalButtonEvent;

		int _persistentCount = _button.onClick.GetPersistentEventCount();

		for (int i = 0; i < _persistentCount; i++)
		{
			_button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
		}

		_button.onClick.Invoke();

		// disable again
		for (int i = 0; i < _persistentCount; i++)
		{
			_button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
		}

        if (gameObject.activeInHierarchy == false)
        {
			_disabled = false;
			_button.onClick = _buttonEvent;
        }
        else
        {
            StartCoroutine(CoRoutines.DelayedCallback(0.25f, () =>
            {
                _disabled = false;
                _button.onClick = _buttonEvent;
            }));
        }
    }
}
