using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.GameObjects;
using UnityEngine.UI;
using FlipWebApps.BeautifulTransitions.Scripts.Transitions.Components.GameObject;
using GameFramework.GameObjects.Components;
using GameFramework.GameStructure;
using GameFramework.Billing.Messages;
using System;
using GameFramework.Preferences;
using System.Globalization;
using GameFramework.UI.Dialogs.Components;
using GameFramework.Billing.Components;
using UnityEngine.Purchasing;
using GameFramework.Localisation;
using UnityEngine.Advertisements;
using GameFramework.Display.Other;
using GameFramework.Messaging;

public class MarketController : Singleton <MarketController> {
	[SerializeField]
	private GameObject ItemsContainer;
	private GameObject[] items;
	private bool active;

	void Start () {
		items = GameObjectUtils.GetChildWithNameGameObject (ItemsContainer, "BuyItem", true);

		PaymentProductCoins[] Coins = CustomPaymentManager.Coins;

		for ( int i = 0; i < items.Length; i++ ) {
			GameObject item = items [i];

			if ( i >= Coins.Length ) {
				item.SetActive (false);
				continue;
			}
				
			PaymentProductCoins coin = Coins [i];
            CustomPaymentManager payment = CustomPaymentManager.Instance as CustomPaymentManager;

			Product product = payment.Product(coin.Product.Name);

			Text price = GameObjectHelper.GetChildNamedGameObject (item, "Price", true).GetComponent<Text>() as Text;

			if (product != null) {
				price.text = product.metadata.localizedPriceString;
			} else {
                price.text = string.Format("{0} {1}", coin.Currency, coin.Price);
			}

			Text title = GameObjectHelper.GetChildNamedGameObject (item, "Name", true).GetComponent<Text> () as Text;

			if (coin.Coins > 0) {
				title.text = coin.Coins.ToString ();
			} else {
				title.gameObject.SetActive (false);
			}
				
			Text descr = GameObjectHelper.GetChildNamedGameObject (item, "Description", true).GetComponent<Text>() as Text;
			descr.text = coin.Description;

            if (coin.Coins == 0)
            {
                descr.gameObject.SetActive(true);
            }
		}

		Show ();
	}

    protected override void GameDestroy() {
        
    }

	public void BuyCoins(int index) {
		PaymentProductCoins[] Coins = CustomPaymentManager.Coins;

		if ( index >= Coins.Length ) {
			return;
		}

		PaymentProductCoins coin = Coins [index];

		if (Debug.isDebugBuild) {
			GameManager.SafeQueueMessage(new ItemPurchasedMessage(coin.Product.Name));
		} else {
            ((CustomPaymentManager)CustomPaymentManager.Instance).BuyProductId (coin.Product.Name);
		}
	}

	public void Restore() {
		DialogManager.Instance.Show (prefabName: "ConfirmDialog", 
			title: LocaliseText.Get ("Button.Restore"), 
			text: LocaliseText.Get ("Market.RestoreDescription"), 
			doneCallback: RestoreCallback,
			dialogButtons: DialogInstance.DialogButtonsType.OkCancel);
	}

	void RestoreCallback(DialogInstance dialogInstance) {
		if (dialogInstance.DialogResult == DialogInstance.DialogResultType.Ok) {
            ((CustomPaymentManager)CustomPaymentManager.Instance).RestorePurchases ();
		}
	}

	void Update() {
		if ( this.active && Input.GetKeyDown(KeyCode.Escape) ) {
			Close ();
		}
	}

	public void Close() {
		this.active = false;

        GameObject CloseButton = GameObjectHelper.GetChildNamedGameObject(gameObject, "Close", true);
        Button _button = CloseButton.GetComponent<Button>();

        _button.onClick.Invoke();

        Loading.Instance.Hide();
	}

	public void Show() {
		this.active = true;

		if (!Debug.isDebugBuild) {
			Fabric.Answers.Answers.LogContentView ("Market", "Dialog");
		}
	}
}
