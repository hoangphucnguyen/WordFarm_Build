using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameStructure;
using GameFramework.GameStructure.Players.Messages;
using GameFramework.Messaging;

public class CoinsAnimating : MonoBehaviour {

	[SerializeField]
	GameObject addCoinsItem;
	[SerializeField]
	GameObject removeCoinsItem;

	void Start () {
		GameManager.SafeAddListener<PlayerCoinsChangedMessage>(PlayerCoinsChanged);
	}

	void OnDestroy() {
		GameManager.SafeRemoveListener<PlayerCoinsChangedMessage>(PlayerCoinsChanged);
	}

	public bool PlayerCoinsChanged(BaseMessage message)
	{
		PlayerCoinsChangedMessage m = message as PlayerCoinsChangedMessage;

		if ( m.NewCoins < m.OldCoins ) { // user loose coins
            GameObject removeCoinsClone = Instantiate(removeCoinsItem, removeCoinsItem.transform.parent);

			RemoveCoinsAnimated removeCoins = removeCoinsClone.GetComponent <RemoveCoinsAnimated> ();
			removeCoins.AnimateCoinsRemoving (m.OldCoins-m.NewCoins);
		} else {
			// TODO - make adding
		}

		return true;
	}
}
