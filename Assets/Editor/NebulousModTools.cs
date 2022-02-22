// NebulousModTools
// @author: amelim

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class NebulousModTools {

	private static List<string> dllWhiteList = new List<string> {
			"Mirror.Authenticators.dll", "Mirror.Components.dll", 
			"UIExtensions.dll", "Facepunch.Steamworks.Win64.dll",
			"kcp2k.dll", "Mirror.dll", "Nebulous.dll", "Priority Queue.dll",
			"QFSW.QC.dll", "QuickGraph.All.dll", "QuickGraph.Serialization.dll",
			"RSG.Promise.dll", "Telepathy.dll", "Unity.Addressables.dll", 
			"Unity.ResourceManager.dll", "Vectrosity.dll", "where-allocations.dll",
			"QuickGraph.Core.dll", "QuickGraph.Data.dll", "QuickGraph.Graphviz.dll", 
			"XNode.dll", "ShapesRuntime.dll"};

	public static string LibDirectory = "Assets/Lib/";
	private static string AssetBundleDirectory = "Assets/AssetBundles";

	[MenuItem("Nebulous/Assets/Build AssetBundles")]
	public static void BuildAllAssetBundles() {
		BuildBundles(true);
	}

	[MenuItem("Nebulous/Assets/Build AssetBundles (Uncompressed)")]
	public static void BuildAllAssetBundlesUncompressed() {
		BuildBundles(false);
	}

	[MenuItem("Nebulous/Load Game Files")]
	public static void LoadAllGameFiles() {
		bool success = LoadGameDlls();
		if(success){
			AssetDatabase.Refresh();
			UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
		}
	}

	[MenuItem("Nebulous/Clear Game File Cache")]
	public static void ClearAllGameFiles() {
		foreach (string file in dllWhiteList){
			if(File.Exists(LibDirectory + file)){
				File.Delete(LibDirectory + file);
			}
		}
		AssetDatabase.Refresh();
	}

  // TODO: Function to deploy mod to mod folder?

	private static void BuildBundles(bool compressed) {
		if (!Directory.Exists(AssetBundleDirectory)) {
			Directory.CreateDirectory(AssetBundleDirectory);
		}

		BuildPipeline.BuildAssetBundles(AssetBundleDirectory, compressed ? BuildAssetBundleOptions.None : BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
	}

	public static bool CheckFilesAreLoaded(){
		bool allFiles = true;
		foreach (string file in dllWhiteList){
			if(!File.Exists(LibDirectory + file)){
				Debug.LogError("NebModSDK: Unable to find: " + LibDirectory + file);
				allFiles = false;
			}
		}
		return allFiles;
	}

	private static bool LoadGameDlls(){
		bool success = true;
		string folderLocation = EditorUtility.OpenFolderPanel("Nebulous Install Directory", "", "");
		List<string> files = new List<string>(Directory.GetFiles(folderLocation));

		foreach (string file in dllWhiteList){
			string fullWhitelistFile = folderLocation + "/" + file;
			if(File.Exists(fullWhitelistFile)){
				File.Copy(fullWhitelistFile, LibDirectory+file);
			}else{
				Debug.LogError("Cannot find: " + fullWhitelistFile);
				success = false;
			}
		}
		return success;
	}
}