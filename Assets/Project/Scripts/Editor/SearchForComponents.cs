using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
 
public class SearchForComponents : EditorWindow {
   [MenuItem( "Tools/Our/Search For Components" )]
   static void Init () {
     SearchForComponents window = (SearchForComponents) EditorWindow.GetWindow( typeof( SearchForComponents ) );
     window.Show();
     window.position = new Rect( 20, 80, 400, 300 );
   }
 
 
    string[] modes = new string[] { "Search for component usage", "Search for missing components" };
 
   List<string> listResult;
   int editorMode, editorModeOld;
   MonoScript targetComponent, lastChecked;
    Component _targetComponent, _lastCheckedComponent;
   string componentName = "";
   Vector2 scroll;
 
   void OnGUI () {
     GUILayout.Space( 3 );
     int oldValue = GUI.skin.window.padding.bottom;
     GUI.skin.window.padding.bottom = -20;
     Rect windowRect = GUILayoutUtility.GetRect( 1, 17 );
     windowRect.x += 4;
     windowRect.width -= 7;
     editorMode = GUI.SelectionGrid( windowRect, editorMode, modes, 2, "Window" );
     GUI.skin.window.padding.bottom = oldValue;
 
     if ( editorModeOld != editorMode ) {
       editorModeOld = editorMode;
       listResult = new List<string>();
       componentName = targetComponent == null ? "" : targetComponent.name;
       lastChecked = null;
     }
 
     switch ( editorMode ) {
       case 0:
         targetComponent = (MonoScript) EditorGUILayout.ObjectField( targetComponent, typeof( MonoScript ), false );
 
         if ( targetComponent != lastChecked ) {
           lastChecked = targetComponent;
           componentName = targetComponent.name;
           AssetDatabase.SaveAssets();
           string targetPath = AssetDatabase.GetAssetPath( targetComponent );
           string[] allPrefabs = GetAllPrefabs();
           listResult = new List<string>();
           foreach ( string prefab in allPrefabs ) {
             string[] single = new string[] { prefab };
             string[] dependencies = AssetDatabase.GetDependencies( single );
             foreach ( string dependedAsset in dependencies ) {
               if ( dependedAsset == targetPath ) {
                 listResult.Add( prefab );
               }
             }
           }
         }
         break;
       case 1:
         if ( GUILayout.Button( "Search!" ) ) {
           string[] allPrefabs = GetAllPrefabs();
           listResult = new List<string>();
           foreach ( string prefab in allPrefabs ) {
             UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath( prefab );
             GameObject go;
             try {
               go = (GameObject) o;
               Component[] components = go.GetComponentsInChildren<Component>( true );
               foreach ( Component c in components ) {
                 if ( c == null ) {
                   listResult.Add( prefab );
                 }
               }
             } catch {
               Debug.Log( "For some reason, prefab " + prefab + " won't cast to GameObject" );
 
             }
           }
         }
         break;
     }
 
     if ( listResult != null ) {
       if ( listResult.Count == 0 ) {
         GUILayout.Label( editorMode == 0 ? ( componentName == "" ? "Choose a component" : "No prefabs use component " + componentName ) : ( "No prefabs have missing components!\nClick Search to check again" ) );
       } else {
         GUILayout.Label( editorMode == 0 ? ( "The following prefabs use component " + componentName + ":" ) : ( "The following prefabs have missing components:" ) );
         scroll = GUILayout.BeginScrollView( scroll );
         foreach ( string s in listResult ) {
           GUILayout.BeginHorizontal();
           GUILayout.Label( s, GUILayout.Width( position.width / 2 ) );
           if ( GUILayout.Button( "Select", GUILayout.Width( position.width / 2 - 10 ) ) ) {
             Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( s );
           }
           GUILayout.EndHorizontal();
         }
         GUILayout.EndScrollView();
       }
     }
   }
 
   public static string[] GetAllPrefabs () {
     string[] temp = AssetDatabase.GetAllAssetPaths();
     List<string> result = new List<string>();
     foreach ( string s in temp ) {
       if ( s.Contains( ".prefab" ) ) result.Add( s );
     }
     return result.ToArray();
   }
}