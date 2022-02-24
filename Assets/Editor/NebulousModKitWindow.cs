using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NebulousModKitWindow : EditorWindow
{
    private const string Version = "0.0.1";
    private string LoadDllButton = "Load DLLs";
    private string ClearDllButton = "Clear Cache";
    private string BuildAssets = "Build AssetBundles";
    private string DeployAssets = "Deploy AssetBundle";
    private bool CompressToggle = false;
    private string BuildMapTemplate = "Create Map Template";
    private string MapName = "Default Map";
    private int PlayerCount = 4;
    private bool RandomizedControlPoints = false;
    private bool HDRPVolumetricFog = false;


    [MenuItem("Window/Nebulous")]
    public static void ShowWindow(){
        //Show existing window instance. If one doesn't exist, make it
        EditorWindow.GetWindow(typeof(NebulousModKitWindow), false, "Nebulous");
    }

    void OnGUI(){
        GUILayout.Label("Nebulous Modkit - Version: " + Version, EditorStyles.boldLabel);
        // ----- Modkit Configuration ----- //
        GUILayout.Label("Modkit Configuration", EditorStyles.boldLabel);
        NebulousModTools.installPath = 
            EditorGUILayout.TextField(
                "Nebulous Install Location", NebulousModTools.installPath);

        // Call the loading function
        if(GUILayout.Button(LoadDllButton)){
            bool success = NebulousModTools.LoadGameDlls();
            if(success){
                // Cache the install location as it's correct
                NebulousModTools.CacheAddress(NebulousModTools.installPath);
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

        // ----- Mapping ----- //
        MapName = EditorGUILayout.TextField("Map Name", MapName);

        int prevPlayerCount = PlayerCount;
        PlayerCount = EditorGUILayout.IntField("Number of players: ", PlayerCount);
        // We don't want to generate invalid maps
        if(PlayerCount < 2){
            PlayerCount = prevPlayerCount;
        }

        GUILayout.Label("Mapping", EditorStyles.boldLabel);
        if(GUILayout.Button(BuildMapTemplate)){
#if NEBULOUS_LOADED
            TemplateMapGenerator.GenerateMapTemplate(PlayerCount);
#else
            // Display a warning if we don't have Nebulous dlls loaded
            EditorUtility.DisplayDialog(
                "Nebulous Mod Kit", 
                "Please ensure the dlls are loaded from the main game before trying to generate a map template", 
                "Ok");
#endif
        }




    }
}
