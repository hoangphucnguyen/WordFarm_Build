using UnityEngine;
using System.Collections;

public class TvOsCloudExample : MonoBehaviour {

	void Start() {
		Debug.Log("iCloudManager.Instance.init()");


		iCloudManager.OnCloudDataReceivedAction += OnCloudDataReceivedAction;


		iCloudManager.Instance.SetString("Test", "test");



        iCloudManager.Instance.RequestDataForKey ("Test", (iCloudData data) => {
            Debug.Log("Internal callback");
            if (data.IsEmpty) {
                Debug.Log(data.Key + " / " + "data is empty");
            } else {
                Debug.Log(data.Key + " / " + data.StringValue);
            }
        });
	}



	private void OnCloudDataReceivedAction (iCloudData data) {
		Debug.Log("OnCloudDataReceivedAction");
		if(data.IsEmpty) {
            Debug.Log(data.Key + " / " + "data is empty");
		} else {
            Debug.Log(data.Key + " / " + data.StringValue);
		}
	}	
}
