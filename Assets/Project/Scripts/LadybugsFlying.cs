using System.Collections;
using System.Collections.Generic;
using GameFramework.Display.Other;
using UnityEngine;
using DG.Tweening;

public class LadybugsFlying : MonoBehaviour {

    [SerializeField]
    private GameObject _ladybugOne;
	[SerializeField]
	private GameObject _ladybugTwo;
	[SerializeField]
	private GameObject _ladybugThree;

	// Use this for initialization
	void Start () {
        Animate(_ladybugOne, Random.Range(1f, 5f));
        Animate(_ladybugTwo, Random.Range(1f, 8f));
        Animate(_ladybugThree, Random.Range(1f, 15f));
	}

    void Animate(GameObject _ladybug, float delay = 0.0f) {
		StartCoroutine(CoRoutines.DelayedCallback(delay, () =>
		{
			Animate(_ladybug);
		}));
    }

	void Animate(GameObject _ladybug)
	{
		Vector3 pos = RandomPosition();

		_ladybug.transform.DORotateQuaternion(Quaternion.LookRotation(Vector3.forward, pos - _ladybug.transform.position), 0.5f).SetEase(Ease.Linear);

		Animator anim = _ladybug.transform.GetChild(0).GetComponent<Animator>();

		StartCoroutine(CoRoutines.DelayedCallback(0.5f, () =>
		{
			anim.SetTrigger("Prepare");
		}));

		StartCoroutine(CoRoutines.DelayedCallback(1f, () => {
			anim.SetTrigger("Fly");

			_ladybug.transform.DOMove(pos, 2f).SetEase(Ease.Linear).OnComplete(() => {
				anim.SetTrigger("Idle");

				StartCoroutine(CoRoutines.DelayedCallback(2.5f, () => {
					Animate(_ladybug, Random.Range(1f, 10f));
				}));
			});
		}));
	}

    Vector3 RandomPosition() {
        int w = Random.Range(0, Screen.width);
        int h = Random.Range(0, (int)(Screen.height/2));

        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(w, h, 0));

        return new Vector3(pos.x, pos.y, 0);
    }
	
}
