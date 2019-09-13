using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects.Components;
using GameFramework.Helper;
using GameFramework.Facebook.Components;
using GameFramework.Localisation;
using Facebook.Unity;
using GameFramework.GameStructure;
using GameFramework.Facebook.Messages;
using GameFramework.Messaging;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Debugging;
using System;
using Facebook.MiniJSON;

public class FacebookRequests : Singleton<FacebookRequests> {

	private List<string> _invitedFBFriends = new List<string>();
	private List<string> _sentFBFriends = new List<string>();

	public bool UserAlreadyInvited(string ID) {
		return _invitedFBFriends.Contains (ID);
	}

	public bool UserAlreadySent(string ID) {
		return _sentFBFriends.Contains (ID);
	}

	public void AskForLife(List<string> users) {
		JSONObject data = new JSONObject ();
		data.Add ("type", "asklife");

		_invitedFBFriends.InsertRange (0, users);

		FacebookManager.Instance.AppRequest (LocaliseText.Get ("FacebookRequests.AskForLife"), 
			users.ToArray (), 
			null, 
			null, 
			null, 
			data.ToString ());
	}

	public void SendLifes(List<string> users) {
		JSONObject data = new JSONObject ();
		data.Add ("type", "sendlife");

		_sentFBFriends.InsertRange (0, users);

		FacebookManager.Instance.AppRequest (LocaliseText.Get ("FacebookRequests.SendLife"), 
			users.ToArray (), 
			null, 
			null, 
			null, 
			data.ToString ());
	}

	public void DeleteRequest(string ID) {
		FB.API (ID, HttpMethod.DELETE);
	}

	public void GetPendingRequests() {
		if (FacebookManager.Instance.IsLoggedIn) {
			FB.API ("/me/apprequests", HttpMethod.GET, AppRequestsCallBack);
		} else {
			GameManager.SafeRemoveListener<FacebookLoginMessage> (LoginHandler); // remove if have any previous
			GameManager.SafeAddListener<FacebookLoginMessage>(LoginHandler);
		}
	}

	public bool LoginHandler(BaseMessage message)
	{
		var facebookLoginMessage = message as FacebookLoginMessage;
		GameManager.SafeRemoveListener<FacebookLoginMessage>(LoginHandler);
		if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.OK)
		{
			GetPendingRequests ();
		}
		else
		{
			DialogManager.Instance.ShowError(textKey: "Facebook.Error.Login.Description");
		}
		return true;
	}

	private void AppRequestsCallBack(IGraphResult result)
	{
		MyDebug.Log ("AppRequestsCallBack: " + result.RawResult);

		// {"data":[{"application":{"category":"Games","link":"https:\/\/www.facebook.com\/games\/supertangramsaga\/?fbs=-1","name":"Tangram Saga HD - Puzzle Game","namespace":"supertangramsaga","id":"1795472404071895"},"created_time":"2017-04-28T10:03:23+0000","data":"{\"type\":\"sendlife\"}","from":{"name":"Dorothy Alaefbeababed Valtchanovsen","id":"112962892567881"},"message":"Here's a life! Have a great day!","to":{"name":"Mary Alafgjafiicac Huiberg","id":"106562559910467"},"id":"1895223444091310_106562559910467"}],"paging":{"cursors":{"before":"MTg5NTIyMzQ0NDA5MTMxMDoxMDAwMTY3MDE2OTkzMTMZD","after":"MTg5NTIyMzQ0NDA5MTMxMDoxMDAwMTY3MDE2OTkzMTMZD"}}}

        if (result != null && (result.Error == null || result.Error == "")) {
			JSONObject jsonObject = JSONObject.Parse (result.RawResult);
			JSONValue dataArray = jsonObject.GetArray ("data");

			JSONArray arr = new JSONArray ();

			foreach ( JSONValue v in dataArray.Array ) {
				if ( !v.Obj.ContainsKey ("data") ) {
					continue;
				}

				string d = v.Obj.GetString ("data").Replace ("\\", "");
				JSONObject data = JSONObject.Parse (d);

				if ( !data.ContainsKey ("type") ) {
					continue;
				}

				v.Obj.Add ("data", data);

				arr.Add (v);
			}

			GameManager.SafeQueueMessage (new MessagesReceivedMessage(arr));
		}
	}

	public void LoadProfileImages(string userId) {
		if (FacebookManager.Instance.ProfileImages.ContainsKey (userId)) {
			Texture pictureTexture = FacebookManager.Instance.ProfileImages [userId];

			GameManager.SafeQueueMessage(new FacebookProfilePictureMessage(userId, pictureTexture));
		} else {
			FacebookManager.Instance.LoadProfileImage (userId); 
		}
	}

    class FacebookShareLinkHandler
    {
        public string link = null;
        public string linkName;
        public string linkDescription;
        public Action <FacebookShareLinkMessage.ResultType> callback;

        public bool LoginHandler(BaseMessage message)
        {
            var facebookLoginMessage = message as FacebookLoginMessage;

            GameManager.SafeRemoveListener<FacebookLoginMessage>(LoginHandler);

            if (facebookLoginMessage.Result == FacebookLoginMessage.ResultType.OK)
            {
                FacebookRequests.Instance.FeedShare(link, linkName, linkDescription, callback);
            }
            else
            {
                DialogManager.Instance.ShowError(textKey: "Facebook.Error.Login.Description");
            }

            return true;
        }
    }

    public void FeedShare(string link, string linkName, string linkDescription, Action <FacebookShareLinkMessage.ResultType> callback) {
        if (!FacebookManager.Instance.IsLoggedIn)
        {
            FacebookShareLinkHandler postHandler = new FacebookShareLinkHandler() { 
                link = link, 
                linkName = linkName,
                linkDescription = linkDescription,
                callback = callback};
            
            GameManager.SafeAddListener<FacebookLoginMessage>(postHandler.LoginHandler);

            if (!FacebookManager.Instance.IsUserDataLoaded)
            {
                FacebookManager.Instance.Login();
            }
            else
            {
                postHandler.LoginHandler(new FacebookLoginMessage(FacebookLoginMessage.ResultType.OK));
            }
            
            return;
        }

        FB.FeedShare(link: new Uri(link), linkName: linkName, linkDescription:linkDescription, callback: (IShareResult result) => {
            FacebookShareLinkMessage.ResultType resultType = FacebookShareLinkMessage.ResultType.ERROR;

            if (result != null)
            {
                if (result.Error == null || result.Error == "")
                {
                    var responseObject = Json.Deserialize(result.RawResult) as Dictionary<string, object>;

                    object obj = 0;

                    if (responseObject.TryGetValue("cancelled", out obj))
                    {
                        resultType = FacebookShareLinkMessage.ResultType.CANCELLED;
                    }
                    else if (responseObject.TryGetValue("callback_id", out obj))
                    {
                        resultType = FacebookShareLinkMessage.ResultType.OK;
                    }
                }
            }

            if (callback != null)
            {
                callback(resultType);
            }
        }); 
    }
}
