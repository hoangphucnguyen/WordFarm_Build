﻿//----------------------------------------------
// Flip Web Apps: Game Framework
// Copyright © 2016 Flip Web Apps / Mark Hewitt
//
// Please direct any bugs/comments/suggestions to http://www.flipwebapps.com
// 
// The copyright owner grants to the end user a non-exclusive, worldwide, and perpetual license to this Asset
// to integrate only as incorporated and embedded components of electronic games and interactive media and 
// distribute such electronic game and interactive media. End user may modify Assets. End user may otherwise 
// not reproduce, distribute, sublicense, rent, lease or lend the Assets. It is emphasized that the end 
// user shall not be entitled to distribute or transfer in any way (including, without, limitation by way of 
// sublicense) the Assets in any other way than as integrated components of electronic games and interactive media. 

// The above copyright notice and this permission notice must not be removed from any files.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//----------------------------------------------

using UnityEngine;
using GameFramework.EditorExtras.Editor;
using UnityEditor;
using GameFramework.Localisation.ObjectModel;

namespace GameFramework.Localisation.Editor
{
    [CustomPropertyDrawer(typeof(LocalisableText))]
    public class LocalisableTextDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var dataProperty = property.FindPropertyRelative("_data");
            var isLocalisedProperty = property.FindPropertyRelative("_isLocalised");
            label = EditorGUI.BeginProperty(position, label, property);
            var contentPosition = EditorGUI.PrefixLabel(position, label);
            var rowPosition = new Rect(contentPosition) {height = EditorGUIUtility.singleLineHeight};

            // Don't make child fields indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var dataPosition = new Rect(rowPosition) {width = 16};
            var isLocalised = isLocalisedProperty.boolValue = EditorGUI.Toggle(dataPosition, new GUIContent("", "Lets you toggle whether this is a fixed or localised test"), isLocalisedProperty.boolValue, GuiStyles.LocalisationToggleStyle);

            dataPosition.x += 20;
            dataPosition.xMax = rowPosition.xMax - (isLocalised ? 18 : 0);
            EditorGUI.PropertyField(dataPosition, dataProperty, GUIContent.none);

            if (isLocalised)
            {
                dataPosition.x = rowPosition.xMax - 16;
                dataPosition.width = 16;
                if (GUI.Button(dataPosition, new GUIContent("+", "Add a new localisation string")))
                {
                    LocalisationEditorWindow.ShowWindowNew(dataProperty.stringValue);
                }
            }

            if (isLocalisedProperty.boolValue)
            {
                rowPosition.y += EditorGUIUtility.singleLineHeight + 2;
                var localisedText = LocaliseText.Exists(dataProperty.stringValue) ? LocaliseText.Get(dataProperty.stringValue) : "<Key not found in localisation file>";
                EditorGUI.LabelField(rowPosition, localisedText);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isLocalisedProperty = property.FindPropertyRelative("_isLocalised");
            if (isLocalisedProperty.boolValue)
                return EditorGUIUtility.singleLineHeight * 2 + 2;
            else
                return EditorGUIUtility.singleLineHeight;
        }
    }
}