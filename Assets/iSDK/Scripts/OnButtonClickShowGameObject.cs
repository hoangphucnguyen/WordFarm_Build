using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace iSDK.Scripts {

	[RequireComponent(typeof(Button))]
	[AddComponentMenu("iSDK/UI/Buttons/OnButtonClickShowGameObject")]
	public class OnButtonClickShowGameObject : MonoBehaviour {

		public GameObject AttachedGameObject;

		// Use this for initialization
		void Start () {
			gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
		}
		
		void OnClick()
		{
			AttachedGameObject.SetActive (true);
		}
	}

}