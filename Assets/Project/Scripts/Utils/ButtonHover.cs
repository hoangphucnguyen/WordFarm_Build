using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameFramework.GameObjects;

public class ButtonHover : MonoBehaviour {
	[SerializeField]
	private Color textHoverColor;
	[SerializeField]
	private Sprite imageHoverSprite;
	[SerializeField]
	private Color imageHoverColor;
	[SerializeField]
	private Text text;
	[SerializeField]
	private Image image;

	private Color defaultTextColor;
	private Sprite defaultImage;
	private Color defaultImageColor;

	private Sprite defaultButtonImage;

	private bool _selected;
	public bool selected {
		get { return _selected; }
		set { 
			_selected = value; 

			if ( _selected ) {
				Select ();
			} else {
				Deselect ();
			}
		}
	}

	void Awake() {
		if ( text != null ) {
			defaultTextColor = text.color;
		}

		if ( image != null ) {
			defaultImage = image.sprite;
			defaultImageColor = image.color;
		}

		Image button = GetComponent <Image> ();
		defaultButtonImage = button.sprite;
	}

	void Select() {
		if (text != null) {
			text.color = textHoverColor;
		}

		if ( image != null ) {
			image.sprite = imageHoverSprite;

			if ( imageHoverColor != null ) {
				image.color = imageHoverColor;
			}
		}

		Button button = GetComponent <Button> ();
		GetComponent <Image>().sprite = button.spriteState.highlightedSprite;
	}

	void Deselect() {
		if (text != null) {
			text.color = defaultTextColor;
		}

		if ( image != null ) {
			image.sprite = defaultImage;

			if ( imageHoverColor != null ) {
				image.color = defaultImageColor;
			}
		}

		Button button = GetComponent <Button> ();
		GetComponent <Image>().sprite = defaultButtonImage;
	}
}