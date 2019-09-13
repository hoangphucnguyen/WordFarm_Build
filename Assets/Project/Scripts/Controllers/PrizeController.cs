using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeController : MonoBehaviour {

	public void ClaimButton() {
		((CustomFreePrizeManager)CustomFreePrizeManager.Instance).ClaimButton ();
	}

	public void CloseButton() {
		((CustomFreePrizeManager)CustomFreePrizeManager.Instance).CloseButton();
	}

	public void DoubleButton() {
		((CustomFreePrizeManager)CustomFreePrizeManager.Instance).DoubleCoins ();
	}
}
