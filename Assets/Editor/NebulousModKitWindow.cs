using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NebulousModKitWindow : EditorWindow
{
    [MenuItem("Window/Nebulous")]
    public static void ShowWindow(){
        //Show existing window instance. If one doesn't exist, make it
        EditorWindow.GetWindow(typeof(NebulousModKitWindow), false, "Nebulous");
    }

    void OnGUI(){
        GUILayout.Label("Modkit Configuration", EditorStyles.boldLabel);
        NebulousModTools.installPath = 
            EditorGUILayout.TextField(
                "Nebulous Install Location", NebulousModTools.installPath);
    }
}
