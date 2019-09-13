using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Messaging;

public class VideoAdShowingMessage : BaseMessage {
	public readonly bool Showing;

	public VideoAdShowingMessage(bool showing)
	{
		Showing = showing;
	}
}
