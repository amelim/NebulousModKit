using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NebulousModKitWindow : EditorWindow
{
    private bool showEnvConfig = true;
    private string loadDllButton = "Load DLLs";
    private string clearDllButton = "Clear Cache";
    private string buildAssets = "Build AssetBundles";
    private string deployAssets = "Deploy AssetBundle";
    private string runNebulous = "Run Nebulous";
    private string newBundle = "Create Mod Manifest";
    private bool compressToggle = false;
    private string buildMapTemplate = "Create Map Template";
    private string mapName = "Default Map";
    private int playerCount = 4;
    // private bool randomizedControlPoints = false;
    // private bool hdrpVolumetricFog = false;
    private int selectedBundle = 0;
    private string[] bundleLabels = {};
    private Dictionary<string, NebulousModBundleData> bundles 
        = new Dictionary<string, NebulousModBundleData>();


    public void OnFocus(){
        // If bundle folders already exist, we likely need to populate our local vars
        foreach(string folder in Directory.EnumerateDirectories(NebulousModBundle.bundleDir)){
            List<string> splitDir = new List<string>(folder.Split('/'));
            string label = splitDir[splitDir.Count-1];
            if(!bundles.ContainsKey(label)){
                NebulousModBundleData cachedBundle = new NebulousModBundleData(label);
                cachedBundle.LoadFromManifest(folder);
                bundles.Add(label, cachedBundle);
            }
        }
    }


    [MenuItem("Window/Nebulous")]
    public static void ShowWindow(){
        //Show existing window instance. If one doesn't exist, make it
        EditorWindow.GetWindow(typeof(NebulousModKitWindow), false, "Nebulous");
    }

    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
        r.height = thickness;
        r.y+=padding/2;
        r.x-=2;
        r.width +=6;
        EditorGUI.DrawRect(r, color);
    }

    void OnGUI(){
        GUILayout.Label("Nebulous Modkit - Version: " + NebulousModTools.version, EditorStyles.largeLabel);
        // ----- Modkit Configuration ----- //
        //GUILayout.Label("Modkit Configuration", EditorStyles.foldoutHeader);
        showEnvConfig = EditorGUILayout.BeginFoldoutHeaderGroup(showEnvConfig, "Env Configuration");
        if(showEnvConfig){
            NebulousModTools.installPath = 
                EditorGUILayout.TextField(
                    "Nebulous Install Location", NebulousModTools.installPath);

            // Call the loading function
            if(GUILayout.Button(loadDllButton)){
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
            if(GUILayout.Button(clearDllButton)){
                foreach (string file in NebulousModTools.DllWhiteList){
                    if(File.Exists(NebulousModTools.LibDirectory + file)){
                        File.Delete(NebulousModTools.LibDirectory + file);
                        File.Delete(NebulousModTools.LibDirectory + file+".meta");
                    }
                }
                File.Delete(NebulousModTools.CachePath);
                File.Delete(NebulousModTools.CachePath+".meta");
                NebulousModTools.UnloadDefine(NebulousModTools.NebDefine);
                AssetDatabase.Refresh();
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        DrawUILine(new Color ( 0.5f,0.5f,0.5f, 1 ));
        GUILayout.Label("Mod Configuration", EditorStyles.boldLabel);
        bundleLabels = AssetDatabase.GetAllAssetBundleNames();
        selectedBundle = EditorGUILayout.Popup("Asset Label", selectedBundle, bundleLabels);
        string selectedLabel = "";
        if(selectedBundle < bundleLabels.Length){
            selectedLabel = bundleLabels[selectedBundle];
        }
        if(bundles.ContainsKey(selectedLabel)){
            NebulousModBundleData dataRef;
            if(bundles.TryGetValue(selectedLabel, out dataRef)){
                dataRef.DrawGUI();
            }
        }

        // amelim: Don't think we need this...
        if(GUILayout.Button(newBundle) && selectedLabel != ""){
            if(!bundles.ContainsKey(selectedLabel)){
                Debug.Log("NMW Mod manifest not found for:" + selectedLabel);
                bundles.Add(
                    selectedLabel, 
                    new NebulousModBundleData(selectedLabel));
                bundles[selectedLabel].WriteToManifest();
            }else{
                bundles[selectedLabel].WriteToManifest();
            }

        }

        // ----- AssetBundles ----- //
        DrawUILine(new Color ( 0.5f,0.5f,0.5f, 1 ));
        GUILayout.Label("Asset Bundles", EditorStyles.boldLabel);
        if(GUILayout.Button(buildAssets)){
            NebulousModTools.BuildBundles(compressToggle);
        }
        compressToggle = EditorGUILayout.Toggle("Enable Build Compression", compressToggle);
        if(GUILayout.Button(deployAssets)){
            // Do something
            Debug.Log("NMK Deploying Mod: " + selectedLabel);
            NebulousModBundle.DeployMod(bundles[selectedLabel]);
        }
        if(GUILayout.Button(runNebulous) && NebulousModTools.installPath != ""){
            System.Diagnostics.Process.Start(NebulousModTools.installPath+"/Nebulous.exe");
        }


        // ----- Mapping ----- //
        DrawUILine(new Color ( 0.5f,0.5f,0.5f, 1 ));
        GUILayout.Label("Mapping", EditorStyles.boldLabel);
        mapName = EditorGUILayout.TextField("Map Name", mapName);
        int prevplayerCount = playerCount;
        playerCount = EditorGUILayout.IntField("Number of players: ", playerCount);
        // We don't want to generate invalid maps
        if(playerCount < 2){
            playerCount = prevplayerCount;
        }
        if(GUILayout.Button(buildMapTemplate)){
#if NEBULOUS_LOADED
            TemplateMapGenerator.GenerateMapTemplate(mapName, playerCount);
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
