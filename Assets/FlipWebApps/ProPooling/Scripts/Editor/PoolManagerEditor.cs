//----------------------------------------------
// Flip Web Apps: Game Framework
// Copyright © 2016-2017 Flip Web Apps / Mark Hewitt
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

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProPooling.Editor
{
    [CustomEditor(typeof(PoolManager))]
    public class PoolManagerEditor : UnityEditor.Editor
    {
        SerializedProperty _persistBetweenScenesProperty;
        SerializedProperty _poolsProperty;
        PoolManager _poolManager;

        #region GUI Styles

        static Texture2D MakeColoredTexture(Color color)
        {
            var texture = new Texture2D(1, 1) {hideFlags = HideFlags.HideAndDontSave};
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        static GUIStyle DropAreaStyle
        {
            get
            {
                if (_dropAreaStyle != null) return _dropAreaStyle;

                _dropAreaStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = {background = MakeColoredTexture(new Color(1f, 1f, 1f, 0.6f)) },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14
                };
                return _dropAreaStyle;
            }
        }
        static GUIStyle _dropAreaStyle;

        static GUIStyle ButtonStyle
        {
            get
            {
                if (_poolBoxStyle != null) return _poolBoxStyle;

                _poolBoxStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = MakeColoredTexture(new Color(1f, 1f, 1f, 0.4f)) },
                    padding = new RectOffset(0,0,0,0),
                    fontSize = 10
                };
                return _poolBoxStyle;
            }
        }
        static GUIStyle _poolBoxStyle;

        #endregion GUI Styles


        void OnEnable()
        {
            _poolManager = target as PoolManager;
            _persistBetweenScenesProperty = serializedObject.FindProperty("PersistBetweenScenes");
            _poolsProperty = serializedObject.FindProperty("Pools");
        }


        /// <summary>
        /// Clean up all resources for immediate GC
        /// </summary>
        void OnDisable()
        {
            if (_dropAreaStyle != null)
            {
                DestroyImmediate(_dropAreaStyle.normal.background);
                DestroyImmediate(_dropAreaStyle.hover.background);
                _dropAreaStyle = null;
            }
        }

        public override void OnInspectorGUI()
        {
            // Pull all the information from the target into the serializedObject.
            serializedObject.Update();

            EditorGUILayout.PropertyField(_persistBetweenScenesProperty);

            GUILayout.Space(10f);
            var addPoolDropRect = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
            GUI.Box(addPoolDropRect, "Drag a Prefab here to create a new pool", DropAreaStyle);
            GUILayout.Space(10f);

            if (_poolsProperty.arraySize > 0)
            {
                GUILayout.Label("Pools", EditorStyles.boldLabel);

                for (var i = 0; i < _poolsProperty.arraySize; i++)
                {
                    var poolProperty = _poolsProperty.GetArrayElementAtIndex(i);
                    var prefabProperty = poolProperty.FindPropertyRelative("Prefab");
                    var deleted = false;
                    EditorGUILayout.BeginVertical("Box");

                    // indent
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    var name = prefabProperty.objectReferenceValue == null ? "<missing prefab>" : prefabProperty.objectReferenceValue.name;
                    poolProperty.isExpanded = EditorGUILayout.Foldout(poolProperty.isExpanded, name);
                    if (GUILayout.Button("X", ButtonStyle, GUILayout.Width(12), GUILayout.Height(12)) &&
                        EditorUtility.DisplayDialog("Delete Pool?", "Are you sure you want to delete this pool?", "Yes",
                            "No"))
                    {
                        _poolsProperty.DeleteArrayElementAtIndex(i);
                        deleted = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!deleted && poolProperty.isExpanded)
                    {
                        EditorGUILayout.PropertyField(prefabProperty);
                        EditorGUILayout.PropertyField(poolProperty.FindPropertyRelative("PreInitialiseCount"));
                        EditorGUILayout.PropertyField(poolProperty.FindPropertyRelative("InitialCapacity"));
                        EditorGUILayout.PropertyField(poolProperty.FindPropertyRelative("MaxCapacity"));
                        EditorGUILayout.PropertyField(poolProperty.FindPropertyRelative("InactiveParent"));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5f);

                }
            }

            // process drag and drop
            CheckDragAndDrop(addPoolDropRect);

            // Push the information back from the serializedObject to the target.
            serializedObject.ApplyModifiedProperties();
        }


        void CheckDragAndDrop(Rect dropArea)
        {
            var currentEvent = Event.current;

            if (!dropArea.Contains(currentEvent.mousePosition))
                return;

            switch (currentEvent.type)
            {
                // is dragging
                case EventType.DragUpdated:

                    // changing the visual mode of the cursor and hence whether a drop can be performed based on the IsDragValid function.
                    DragAndDrop.visualMode = IsDragValid() ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

                    // Consume the event so it isn't used by anything else.
                    currentEvent.Use();
                    break;

                // was dragging and has now dropped
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();

                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        var gameobject = draggedObject as GameObject;
                        if (gameobject && PrefabUtility.GetPrefabType(gameobject) != PrefabType.None)
                            AddNewPool(gameobject);
                    }

                    // Consume the event so it isn't used by anything else.
                    currentEvent.Use();
                    break;
            }
        }


        void AddNewPool(GameObject gameobject)
        {
            foreach (var pool in _poolManager.Pools)
            {
                if (pool.Prefab && pool.Prefab.name == gameobject.name)
                {
                    EditorUtility.DisplayDialog("Pro Pooling",
                        string.Format(
                            "PoolManager pools need to have unique names. There is already a pool named {0}.\n\nRename the prefab and try again!",
                            gameobject.name), "OK");
                    return;
                }
            }

            _poolsProperty.arraySize++;
            var newElement =
                _poolsProperty.GetArrayElementAtIndex(_poolsProperty.arraySize - 1);
            newElement.isExpanded = true;
            var propPrefab = newElement.FindPropertyRelative("Prefab");
            propPrefab.objectReferenceValue = gameobject;
            var propPreInitialiseCount = newElement.FindPropertyRelative("PreInitialiseCount");
            propPreInitialiseCount.intValue = 0;
            var propInitialCapacity = newElement.FindPropertyRelative("InitialCapacity");
            propInitialCapacity.intValue = 5;
            var propMaxCapacity = newElement.FindPropertyRelative("MaxCapacity");
            propMaxCapacity.intValue = 0;
            var propInactiveParent = newElement.FindPropertyRelative("InactiveParent");
            propInactiveParent.objectReferenceValue = null;
        }


        /// <summary>
        /// A drag is valid if it contains at least one prefab
        /// </summary>
        /// <returns></returns>
        static bool IsDragValid()
        {
            foreach (var draggedObject in DragAndDrop.objectReferences)
            {
                var gameobject = draggedObject as GameObject;
                if (gameobject && PrefabUtility.GetPrefabType(gameobject) != PrefabType.None) return true;
            }
            return false;
        }
    }
}