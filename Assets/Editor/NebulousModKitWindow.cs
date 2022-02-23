using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NebulousModKitWindow : EditorWindow
{

    private string LoadDllButton = "Load DLLs";
    private string ClearDllButton = "Clear DLLs";
    private string BuildAssets = "Build AssetBundles";
    private string DeployAssets = "Deploy AssetBundle";
    private bool CompressToggle = false;

    [MenuItem("Window/Nebulous")]
    public static void ShowWindow(){
        //Show existing window instance. If one doesn't exist, make it
        EditorWindow.GetWindow(typeof(NebulousModKitWindow), false, "Nebulous");
    }

    void OnGUI(){
        // ----- Modkit Configuration ----- //
        GUILayout.Label("Modkit Configuration", EditorStyles.boldLabel);
        NebulousModTools.installPath = 
            EditorGUILayout.TextField(
                "Nebulous Install Location", NebulousModTools.installPath);

        // Call the loading function
        if(GUILayout.Button(LoadDllButton)){
            bool success = NebulousModTools.LoadGameDlls();
            if(success){
                NebulousModTools.LoadDefine(NebulousModTools.NebDefine);
                AssetDatabase.Refresh();
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }
        }

        // Call the clear cache button
        if(GUILayout.Button(ClearDllButton)){
            foreach (string file in NebulousModTools.DllWhiteList){
                if(File.Exists(NebulousModTools.LibDirectory + file)){
                    File.Delete(NebulousModTools.LibDirectory + file);
                }
		    }
            NebulousModTools.UnloadDefine(NebulousModTools.NebDefine);
            AssetDatabase.Refresh();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        // ----- AssetBundles ----- //
        GUILayout.Label("Asset Bundle Generation", EditorStyles.boldLabel);
        if(GUILayout.Button(BuildAssets)){
            NebulousModTools.BuildBundles(CompressToggle);
        }
        CompressToggle = EditorGUILayout.Toggle("Enable Compression", CompressToggle);

        if(GUILayout.Button(DeployAssets)){
            // Do something
        }
    }
}
