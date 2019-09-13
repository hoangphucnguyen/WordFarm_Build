using System.Collections;
using System.Collections.Generic;
using GameFramework.UI.Dialogs.Components;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayController : MonoBehaviour {

    [SerializeField]
    CustomHorizontalScrollSnap _scrollSnap;

    public void NextPage() {
        if ( _scrollSnap.CurrentPage == _scrollSnap.GetComponent<ScrollRect>().content.transform.childCount - 1 ) {
            
            DialogInstance dialogInstance = gameObject.GetComponent<DialogInstance>();

            dialogInstance.Target.GetComponent<Animator>().SetTrigger("Close");
            return;
        }

        _scrollSnap.NextScreen();
    }

    public void PrevPage() {
        _scrollSnap.PreviousScreen();
    }
}
