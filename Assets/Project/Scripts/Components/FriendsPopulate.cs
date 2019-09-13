using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects;
using UnityEngine.UI;
using GameFramework.Facebook.Components;
using GameFramework.GameStructure;
using GameFramework.Facebook.Messages;
using GameFramework.Messaging;
using System;
using Facebook.Unity;
using System.Linq;

public class FriendsPopulate : MonoBehaviour {

	public GameObject Prefab;
	public GameObject PagePrefab;
	public int ItemsPerPage = 2;
	private Dictionary <string, GameObject> users = new Dictionary<string, GameObject>();
	private int myIndex = 0;
	List<UserLevelInfo> listInfo;

	void Start() {
		GameManager.SafeAddListener<FacebookProfilePictureMessage> (FacebookProfilePictureHandler);
	}

	void OnDestroy() {
		GameManager.SafeRemoveListener<FacebookProfilePictureMessage> (FacebookProfilePictureHandler);
	}

	bool FacebookProfilePictureHandler(BaseMessage message) {
		FacebookProfilePictureMessage m = message as FacebookProfilePictureMessage;

		if ( !users.ContainsKey (m.UserId) ) {
			return true;
		}

		GameObject item = users [m.UserId];

		GameObject avatar = GameObjectHelper.GetChildNamedGameObject (item, "Avatar", true);
		avatar.GetComponent <RawImage> ().texture = m.Texture;

		return true;
	}

	public void ScrollToFirst() {
		GameObject parent = transform.parent.gameObject;
		CustomHorizontalScrollSnap h = parent.GetComponent<CustomHorizontalScrollSnap> ();

		h.GoToScreen (0);
	}

	public void ScrollToMe(bool animating = true) {
		GameObject parent = transform.parent.gameObject;
		CustomHorizontalScrollSnap h = parent.GetComponent<CustomHorizontalScrollSnap> ();

		int page = (int)Math.Ceiling((float)myIndex / (float)ItemsPerPage) - 1;

		if ( myIndex == 0 ) {
			page = 0;
		}

		if (animating) {
			h.GoToScreen (page);
		} else {
			h.CurrentPage = page;
			h.UpdateLayout ();
		}
	}

	public void Populate(List<UserLevelInfo> list, int level) {
		listInfo = list;

		//

		GameObject parent = transform.parent.gameObject;
		CustomHorizontalScrollSnap h = parent.GetComponent<CustomHorizontalScrollSnap> ();

		int count = 0;
		int pages = 0;
		GameObject page = null;
		int TotalItems = list.Count;
		foreach ( UserLevelInfo info in list ) {
			if (count == 0 || count % ItemsPerPage == 0) {
				pages++;

                string pageName = string.Format("FriendsPage-{0}", pages);

				page = GameObjectHelper.GetChildNamedGameObject (gameObject, pageName, true);

				if (page) {
					page = null;
					count++;
					continue;
				}

				page = Instantiate (PagePrefab, transform);
				page.name = pageName;
				page.transform.localScale = new Vector3 (1, 1, 1);
				page.transform.position = new Vector3 (0, 0, transform.position.z);

				RectTransform rectTransform = page.transform as RectTransform;
				RectTransform selfRectTransform = transform as RectTransform;

				rectTransform.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top, 10, selfRectTransform.sizeDelta.y - 20);
				rectTransform.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left, 10, selfRectTransform.sizeDelta.x - 20);

				h.AddChild (page);
			}

			if ( !page ) {
				count++;
				continue;
			}

			GameObject newObject = Instantiate(Prefab);
			newObject.transform.SetParent(page.transform, false);

			if ( info.me ) {
				myIndex = count;
			}

			string[] n = info.displayName.Split(null);

			GameObject name = GameObjectHelper.GetChildNamedGameObject (newObject, "Name", true);

			string nameText;
			if (n.Length > 0) {
				nameText = n [0];
			} else {
				nameText = info.displayName;
			}

			name.GetComponent <Text> ().text = string.Format ("{0}. {1}", (count+1), nameText);

			if ( info.time > 0 ) {
				TimeSpan timeSpan = TimeSpan.FromSeconds (info.time);

				string timeText = string.Format ("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

				GameObject time = GameObjectHelper.GetChildNamedGameObject (newObject, "Time", true);
				GameObjectHelper.SafeSetActive (time, true);
				time.GetComponent <Text>().text = timeText;

				if ( count == 0 ) { // first item only, if time > 0
					GameObject NumberOne = GameObjectHelper.GetChildNamedGameObject (transform.parent.parent.parent.gameObject, "NumberOne", true);
					GameObjectHelper.SafeSetActive (NumberOne, true);

					GameObject Me = GameObjectHelper.GetChildNamedGameObject (transform.parent.parent.parent.gameObject, "Me", true);
					GameObjectHelper.SafeSetActive (Me, true);
				}
			}

			if ( info.score > 0 ) {
				GameObject time = GameObjectHelper.GetChildNamedGameObject (newObject, "Time", true);
				GameObjectHelper.SafeSetActive (time, true);
				time.GetComponent <Text>().text = info.score.ToString ();

				if ( count == 0 ) { // first item only, if time > 0
					GameObject NumberOne = GameObjectHelper.GetChildNamedGameObject (transform.parent.parent.parent.gameObject, "NumberOne", true);
					GameObjectHelper.SafeSetActive (NumberOne, true);

					GameObject Me = GameObjectHelper.GetChildNamedGameObject (transform.parent.parent.parent.gameObject, "Me", true);
					GameObjectHelper.SafeSetActive (Me, true);
				}
			}

			if (info.me) {
				GameObject avatar = GameObjectHelper.GetChildNamedGameObject (newObject, "Avatar", true);
				avatar.GetComponent <RawImage> ().texture = FacebookManager.Instance.ProfilePicture;
			} else {
				FacebookRequests.Instance.LoadProfileImages (info.FBUserId);
			}

            if (info.FBUserId != null && !users.ContainsKey(info.FBUserId)) {
				users.Add (info.FBUserId, newObject);
			}

			count++;
		}

		ScrollToMe (false);
	}

	void sendLife(int index) {
		UserLevelInfo info = listInfo [index];

		List<string> l = new List<string>{ info.FBUserId };

		FacebookRequests.Instance.SendLifes (l);
	}
}
