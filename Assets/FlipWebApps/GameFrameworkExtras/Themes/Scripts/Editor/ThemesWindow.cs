//----------------------------------------------
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

using GameFramework.Debugging.Components;
using GameFramework.FreePrize.Components;
using GameFramework.GameObjects;
using GameFramework.GameStructure;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrameworkExtras.Themes.Scripts.Editor {

    /// <summary>
    /// Component for allowing various cheat functions to be called such as increasing score, resetting prefs etc..
    /// </summary>
    public class ThemesWindow : EditorWindow
    {
        Font _font;

        // Add menu item
        [MenuItem("Window/Game Framework/Themes Window (experimental)", priority = 3)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow window = EditorWindow.GetWindow(typeof(ThemesWindow));
            window.titleContent.text = "Themes";
        }

        void OnSelectionChange()
        {
            Repaint();
        }

        void OnGUI()
        {
            // rebranding
            GUILayout.Label("Rebranding", new GUIStyle() { fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5) });
            GUILayout.Label("Select a root gameobject in your scene and then use the functions below to rebrand with updated values. Use with caution!");


            GUILayout.BeginHorizontal();
            GUILayout.Label("Font", new GUIStyle() { fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5) });
            _font = (Font)EditorGUILayout.ObjectField(_font, typeof(Font), true);
            GUI.enabled = Selection.activeObject is GameObject;
            if (GUILayout.Button("Rebrand", GUILayout.Width(100)))
            {
                var gameObject = Selection.activeObject as GameObject;
                if (gameObject != null)
                {
                    var textComponents = gameObject.GetComponentsInChildren<Text>(true);
                    string updatedObjects = "";
                    foreach (var textComponent in textComponents)
                    {
                        textComponent.font = _font;
                        updatedObjects += textComponent.gameObject.GetPath() + "\n";
                    }
                    Debug.Log("Updated font on :\n" + updatedObjects);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}