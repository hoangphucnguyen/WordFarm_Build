using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Display.Other;
using GameFramework.GameObjects;
using UnityEngine.UI;

public class AnimationEvents : MonoBehaviour {

    void Start() {
        Button button = GetComponent<Button>();

        if ( button != null ) {
            button.onClick.AddListener(() => {
                CloseDialog();
            });
        }
    }

    private void OnEnable()
    {
		
    }

    public void HideObject() {
		GameObject dialog = GameObjectUtils.GetParentWithComponentNamedGameObject <DialogInstance> (gameObject);

		DialogInstance dialogInstance = null;

		if (dialog != null) {
			dialogInstance = dialog.GetComponent <DialogInstance> ();
		}

        StartCoroutine(CoRoutines.DelayedCallback(0.3f, () =>
        {
            if (dialogInstance != null)
            {
                dialogInstance.Done();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }));
	}

	public void CloseDialog() {
		GameObject dialog = GameObjectUtils.GetParentWithComponentNamedGameObject <DialogInstance> (gameObject);

		DialogInstance dialogInstance = dialog.GetComponent <DialogInstance>();

		dialogInstance.Target.GetComponent <Animator>().SetTrigger ("Close");
	}
}
