// NebulousModTools
// @author: amelim

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

	/// <summary>
	///
	/// </summary>
public static class NebulousModTools {

  /// <summary> Whitelist of all game dlls needed to compile the modkit
	public static List<string> DllWhiteList = new List<string> {
			"Mirror.Authenticators.dll", "Mirror.Components.dll", 
			"UIExtensions.dll", "Facepunch.Steamworks.Win64.dll",
			"kcp2k.dll", "Mirror.dll", "Nebulous.dll", "Priority Queue.dll",
			"QFSW.QC.dll", "QuickGraph.All.dll", "QuickGraph.Serialization.dll",
			"RSG.Promise.dll", "Telepathy.dll", "Unity.Addressables.dll", 
			"Unity.ResourceManager.dll", "Vectrosity.dll", "where-allocations.dll",
			"QuickGraph.Core.dll", "QuickGraph.Data.dll", "QuickGraph.Graphviz.dll", 
			"XNode.dll", "ShapesRuntime.dll"};

	public const string version = "0.0.1";
	/// <summary> Folder for storing the whitelisted dlls
	public const string LibDirectory = "Assets/Lib/";
	/// <summary> Output folder for building assetbundles
	public const string AssetBundleDirectory = "Assets/AssetBundles";
	public static string bundleDir = "Assets/NebulousModKit/Mods/";
	/// <summary> Preprocessor define that gates code requiring whitelisted dlls
	public const string NebDefine = "NEBULOUS_LOADED";
	/// <summary> Default install location for Nebulous, may change
	public static string installPath = "";   
	public const string CachePath = "Assets/NebulousModKit/InstallCache.txt"; 



	// ------------------------------------------------------------------------------------ //
	// 																		Data Loading																	    //
	// ------------------------------------------------------------------------------------ //

	/// <summary>
	/// On intialization of this script, it will check to see if the dlls are present in the project
	/// If the files are present, add the NebDefine preprocessor flag to compile related code 
	/// </summary>
	[InitializeOnLoadMethod]
	private static void checkIfNebulousExists()
	{

		// Check to see if there is a cached install file
		string installCache = CheckForCache();
		if(installCache != ""){
			installPath = installCache;
			if(!CheckFilesAreLoaded()){ // We have a chache file, but the dlls are not loaded
				LoadGameDlls();
				LoadDefine(NebDefine);
			}
		}

		if(!CheckFilesAreLoaded()){ // If for some reason we don't have the dlls loaded, we need to prevent compilation
			UnloadDefine(NebDefine);
		}
		string defines = 
				PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

		Debug.LogFormat("NMK Preprocessor: {0}", defines);
	} 

	/// <summary>
	/// Adds a string to the Unity PlayerSettings DefineSymbols list. Defaults to selected build target group
	/// <param name="thisDefine"> The string which will get added to the symbols list
	/// </summary>
	public static void LoadDefine(string thisDefine){
		string defines = 
				PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		if(!defines.Contains(thisDefine)){
			Debug.LogFormat("NMK Preprocessor: {0} NOT LOADED BUT FOUND", thisDefine);
			if(defines.Length > 0){
				defines += ";";
			}
			defines += thisDefine;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
		}
	}

	/// <summary>
	/// Removes a string to the Unity PlayerSettings DefineSymbols list. Defaults to selected build target group
	/// <param name="thisDefine"> The string which will get removed to the symbols list
	/// </summary>
	public static void UnloadDefine(string thisDefine){
		string defines = 
				PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		if(defines.Contains(thisDefine)){
				Debug.LogFormat("NMK Preprocessor: {0} BUT MISSING", thisDefine);

				int index = defines.IndexOf(thisDefine);
				if(index >=0){
					if(index > 0){
						index -= 1; // Remove semicolon if this isn't the first element in the list
					}

					int wordLength = Mathf.Min(thisDefine.Length+1, defines.Length-index);
					defines = defines.Remove(index,wordLength);
					PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
				}
 			}
	}

	/// <summary>
	/// Checks the Asset/Lib/ folder to see if the white listed dlls are present
	/// <returns> Returns true if all the dlls are present, false if one is missing
	/// </summary>
	public static bool CheckFilesAreLoaded(){
		bool allFiles = true;
		foreach (string file in DllWhiteList){
			if(!File.Exists(LibDirectory + file)){
				Debug.LogError("NebModSDK: Unable to find: " + LibDirectory + file);
				allFiles = false;
			}
		}
		return allFiles;
	}

	/// <summary>
	/// Copy the whitelisted dlls from the pop up dialog to Asset/Lib/
	/// Will not copy any files if they are not the files specified in dllWhiteList
	/// <returns> Returns true if all files were copied correctly
	/// </summary>
	public static bool LoadGameDlls(){
		bool success = true;
		if(installPath == ""){
			// Check if we have a cache
			if(CheckForCache() == ""){
				installPath = EditorUtility.OpenFolderPanel("Nebulous Install Directory", "", "");
				// Create cache of this address
				CacheAddress(installPath);
			}
		}
		string dllDirectory = "\\Nebulous_Data\\Managed\\";

		foreach (string file in DllWhiteList){
			string fullWhitelistFile = installPath + dllDirectory + file;
			if(File.Exists(fullWhitelistFile)){
				File.Copy(fullWhitelistFile, LibDirectory+file);
			}else{
				Debug.LogError("Cannot find: " + fullWhitelistFile);
				success = false;
			}
		}
		return success;
	}

	/// <summary> Checks to see if a cached install location exists already
	private static string CheckForCache(){
		if(File.Exists(CachePath)){
			string path = "";
			using(StreamReader sr = File.OpenText(CachePath)){
				path = sr.ReadLine();
				if(path != null){
					return path;
				}
			}
		}
		return "";
	}

	/// <summary> Saves the install file to a cache file inside of Assets/NebulousModKit
	public static void CacheAddress(string address){
		if(!File.Exists(address)){
			using (StreamWriter sw = File.CreateText(CachePath)){
				sw.WriteLine(address);
			}
		}
	}

	// ------------------------------------------------------------------------------------ //
	// 																		Asset Bundles																	    //
	// ------------------------------------------------------------------------------------ //

	/// <summary>
	/// Calls the BuildPipeline BuildAssetBundles function with compression if requested.
	/// <param name="compressed"> True if building with compression enabled
	/// </summary>
	public static void BuildBundles(bool compressed) {
		if (!Directory.Exists(AssetBundleDirectory)) {
			Directory.CreateDirectory(AssetBundleDirectory);
		}

		BuildPipeline.BuildAssetBundles(AssetBundleDirectory, compressed ? BuildAssetBundleOptions.None : BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
	}


	public static void DeployBundle(string tag){

	}
}