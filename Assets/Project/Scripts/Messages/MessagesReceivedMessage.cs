using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Messaging;
using GameFramework.Helper;

public class MessagesReceivedMessage : BaseMessage {
	public JSONArray messages;

	public MessagesReceivedMessage (JSONArray m) {
		this.messages = m;
	}
}
