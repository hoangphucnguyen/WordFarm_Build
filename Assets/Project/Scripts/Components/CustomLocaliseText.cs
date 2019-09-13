using System;
using System.Globalization;
using GameFramework.Messaging.Components.AbstractClasses;
using UnityEngine;
using GameFramework.Localisation.Messages;
using UnityEngine.Events;
using GameFramework.GameStructure;
using GameFramework.Localisation.Components;

namespace GameFramework.Localisation.Components
{
    [System.Serializable]
    public class CustomLocaliseTextOnPreLocaliseEvent : UnityEvent<CustomLocaliseText>
    {
    }

    /// <summary>
    /// Localises a Text field based upon the given Key
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    [AddComponentMenu("Game Framework/Localisation/Localise Text")]
    [HelpURL("http://www.flipwebapps.com/unity-assets/game-framework/localisation/")]
    public class CustomLocaliseText : RunOnMessage<LocalisationChangedMessage>
    {

        public enum ModifierType { None, LowerCase, UpperCase, Title, FirstCapital }

        /// <summary>
        /// Localization key.
        /// </summary>
        public string Key;

        /// <summary>
        /// A modifier to apply
        /// </summary>
        public ModifierType Modifier = ModifierType.None;

        /// <summary>
        /// Callback that allows for modification of the localisation string.
        /// </summary>
        public CustomLocaliseTextOnPreLocaliseEvent OnPreLocalise;

        /// <summary>
        /// Manually change the value of whatever the localization component is attached to.
        /// </summary>
        public string Value
        {
            set
            {
                if (string.IsNullOrEmpty(value)) return;

                _textComponent.text = value;
            }
        }

        /// <summary>
        /// The localised value that can be referenced and modified by a PreLocaise callback before the display is updated.
        /// </summary>
        public string PreLocaliseValue { get; set; }

        UnityEngine.UI.Text _textComponent;

        /// <summary>
        /// setup
        /// </summary>
        public override void Awake()
        {
            _textComponent = GetComponent<UnityEngine.UI.Text>();

            Localise();
            base.Awake();
        }


        /// <summary>
        /// Update the display with the localise text
        /// </summary>
        void Localise()
        {
            // added by YouLocal team, 27.08.2017
            _textComponent.font = GeneralUtils.FontForCurrentLanguage(_textComponent.fontStyle);

            // If we don't have a key then don't change the value
            if (string.IsNullOrEmpty(Key)) return;

            PreLocaliseValue = Localisation.LocaliseText.Get(Key);

            // Run any callback to modify the term.
            OnPreLocalise.Invoke(this);

            // apply any modifier
            if (!string.IsNullOrEmpty(PreLocaliseValue))
            {
                switch (Modifier)
                {
                    case ModifierType.LowerCase:
                        PreLocaliseValue = PreLocaliseValue.ToLower();
                        break;
                    case ModifierType.UpperCase:
                        PreLocaliseValue = PreLocaliseValue.ToUpper();
                        break;
                    case ModifierType.Title:
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(PreLocaliseValue);
                        break;
                    case ModifierType.FirstCapital:
                        var characters = PreLocaliseValue.ToLower().ToCharArray();
                        characters[0] = char.ToUpper(PreLocaliseValue[0]);
                        PreLocaliseValue = new string(characters);
                        break;
                }
            }

            // set the value
            Value = PreLocaliseValue;
        }


        /// <summary>
        /// Called whenever the localisation changes.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool RunMethod(LocalisationChangedMessage message)
        {
            Localise();
            return true;
        }
    }
}