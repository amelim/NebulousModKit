using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if NEBULOUS_LOADED
using Game;
#endif

/// <summary> NebulousModBundleData is a data storage class that contains everything required for a valid mod
/// </summary>
public class NebulousModBundleData
{
    public int modMajRev = 0;
    public int modMinRev = 0;
    public int modPatchRev = 0;
    public string name = "NewMod";
    public string modVer = "0.0.0";
    public string assetLabel = "";
    public string modDesc = "Your description";

    public NebulousModBundleData(string label){
        // Default name to label, but this can be changed
        name = label;
        assetLabel = label;
    }

    public bool LoadFromManifest(string address){
        bool validNMKMod = false;
        if(File.Exists(address + "/manifest.xml")){
            Debug.Log("NMK: Found existing bundle at " + address);
            XmlReader reader = XmlReader.Create(address + "/manifest.xml");
            while(reader.Read()){
                if(reader.NodeType == XmlNodeType.Element){
                    if(reader.Name == "NebulousModKit"){
                        string version = reader.GetAttribute("version");
                        Debug.Log("NMK Version: " + version);
                        if(version.CompareTo(NebulousModTools.version)!=0){
                            Debug.LogWarning("NMK version mismatch with this bundle");
                        }
                        // Read the children
                        reader.ReadToFollowing("ModVersion");
                        string modVersion = reader.GetAttribute("version");
                        Debug.Log("NMK Mod Version: " + modVersion);
                        modVer = modVersion;
                        List<string> splitVersion = new List<string>(modVersion.Split('.'));
                        if(splitVersion.Count != 3){
                            Debug.LogWarning("NMK Invalid Mod Version format!");
                        }else{
                            modMajRev = System.Convert.ToInt32(splitVersion[0]);
                            modMinRev = System.Convert.ToInt32(splitVersion[1]);
                            modPatchRev = System.Convert.ToInt32(splitVersion[2]);
                        }
                    }
                    if(reader.Name == "Namespace"){
                        name = reader.ReadElementContentAsString();
                        Debug.Log("NMK Mod Name: " + name);
                    }
                    validNMKMod = true;
                }
            }
            reader.Close();
        }
        return validNMKMod;
    }

    public bool WriteToManifest(){

#if NEBULOUS_LOADED
        // Iterate through the assets in the label
        Dictionary<string, GameObject> mapAssets = new Dictionary<string, GameObject>();
        foreach(string assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(assetLabel)){
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if(asset != null){
                if(asset.GetComponent<Game.Map.Battlespace>()!=null){
                    // We have a map!
                    mapAssets.Add(assetPath, asset);
                }
            }
        }

        // Check to see if this is already created
        if(!Directory.Exists(NebulousModTools.bundleDir + assetLabel)){
            Directory.CreateDirectory(NebulousModTools.bundleDir + assetLabel);    
        }
        string manifestPath = NebulousModTools.bundleDir + assetLabel  + "/manifest.xml";
        if(File.Exists(manifestPath)){
            File.Delete(manifestPath);
        }

        if(!File.Exists(manifestPath)){
            // Create a new manifest
            XmlWriter writer = null;
            try{
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            
            writer = XmlWriter.Create(manifestPath, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("BundleManifest");
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xds", null, "http://www.w3.org/2001/XMLSchema");
            writer.WriteStartElement("NebulousModKit");
            writer.WriteAttributeString("version", NebulousModTools.version);
            writer.WriteStartElement("ModVersion");
            writer.WriteAttributeString("version", modVer);
            writer.WriteEndElement();//ModVersion
            writer.WriteEndElement();// NebulousModKit
            writer.WriteElementString("Namespace", name);
            //writer.WriteElementString("BasePath", NebulousModBundle.bundleDir+assetLabel);
            if(mapAssets.Count>0){
                writer.WriteStartElement("Maps");
                    foreach(KeyValuePair<string, GameObject> pair in mapAssets){
                        writer.WriteStartElement("Entry");
                        writer.WriteAttributeString("Name", pair.Value.name);
                        writer.WriteAttributeString("Address", pair.Key);
                        writer.WriteEndElement();
                    }
                writer.WriteEndElement();//maps
            }
            
            writer.WriteEndElement();// BundleManifest
            }
            finally{
                if(writer!=null){
                    writer.Close();
                }
            }
            AssetDatabase.Refresh();
            if(File.Exists(manifestPath)){
                //AssetImporter.GetAtPath(manifestPath).SetAssetBundleNameAndVariant(assetLabel, "");
                AssetImporter ai = AssetImporter.GetAtPath(manifestPath);
                if(ai == null){
                    Debug.LogError("null AI");
                }else{
                    ai.SetAssetBundleNameAndVariant(assetLabel, "");
                    ai.SaveAndReimport();
                }
                
            }
        }


        return true;
#else
        Debug.LogError("NMK Nebulous DLLs are not loaded, cannot continue making manifest");
        return false;
#endif
    }

    public void DrawGUI(){
        name = EditorGUILayout.TextField("Mod Namespace:", name);
        // Should probably sanitize this to 0.0.0 format
        modVer = EditorGUILayout.TextField("Mod Version:", modVer);
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        modDesc = EditorGUILayout.TextArea(modDesc, style);
    }
}

public static class NebulousModBundle {

    public static string bundleDir = "Assets/NebulousModKit/Mods/";

    public static bool CreateInitialBundle(string label){
        // Check to see if this is already created
        if(Directory.Exists(bundleDir+label)){
            return false;
        }
        Directory.CreateDirectory(bundleDir + label);
        AssetDatabase.Refresh();
        return true;
    }

    public static bool DeployMod(NebulousModBundleData mod){
        string manifestPath = NebulousModBundle.bundleDir + mod.assetLabel + "/manifest.xml";
        if(File.Exists(manifestPath)){
            string modFolder = NebulousModTools.installPath + "/Mods/" + mod.assetLabel;
            Debug.Log(modFolder);
            if(!Directory.Exists(modFolder)){
                Directory.CreateDirectory(modFolder);
            }
            File.Copy(NebulousModTools.AssetBundleDirectory+"/"+mod.assetLabel, 
                      modFolder+"/"+mod.assetLabel, true);

            // Genreate a new ModInfo.xml
            string modInfoPath = NebulousModTools.bundleDir + mod.assetLabel + "/ModInfo.xml";
            if(File.Exists(modInfoPath)){
                File.Delete(modInfoPath);
            }
            // Create a new manifest
            XmlWriter writer = null;
            try{
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                writer = XmlWriter.Create(modInfoPath, settings);
                writer.WriteStartDocument();
                writer.WriteStartElement("ModInfo");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xmlns", "xds", null, "http://www.w3.org/2001/XMLSchema");
                writer.WriteElementString("ModName", mod.name);
                writer.WriteElementString("ModDescription", mod.modDesc);
                writer.WriteElementString("ModVer", mod.modVer);
                writer.WriteStartElement("AssetBundles");
                writer.WriteElementString("string", mod.assetLabel);
                writer.WriteEndElement();//AssetBundles
                writer.WriteEndElement();//Modinfo
            }            
            finally{
                if(writer!=null){
                    writer.Close();
                }
            }
            if(File.Exists(modInfoPath)){
                File.Copy(modInfoPath, modFolder+"/ModInfo.xml", true);
            }else{
                Debug.LogWarning("NMK Unable to copy ModInfo.xml to Nebulous/Mods/"+mod.assetLabel);
            }
            
        }else{
            Debug.LogWarning("NMK No manifest found for mod: " + mod.assetLabel);
        }
        return true;
    }

}