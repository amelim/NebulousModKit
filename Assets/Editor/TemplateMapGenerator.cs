using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Only want this to work if we have the dlls
#if NEBULOUS_LOADED
using Game.Map;
#endif

/// <summary> Template for generating map templates
public static class TemplateMapGenerator 
{
    /// <summary> Generates playerCount children to groupName
    private static bool genIndividualSpawns(string groupName, int playerCount){
#if NEBULOUS_LOADED
        GameObject spawnParent = GameObject.Find(groupName);
        if(spawnParent == null){
            return false;
        }
        SpawnGroup parentGroup = spawnParent.GetComponent<SpawnGroup>();
        List<SpawnPoint> points = new List<SpawnPoint>();

        for(int i=0; i<playerCount; i++){
            GameObject spawn = new GameObject(groupName + "_" + i);
            spawn.transform.parent = spawnParent.transform;
            spawn.AddComponent<SpawnPoint>();
            points.Add(spawn.GetComponent<SpawnPoint>());
        }

        Type spawnGroupType = parentGroup.GetType();
        FieldInfo spawnList = 
            spawnGroupType.GetField("_spawns", BindingFlags.NonPublic | BindingFlags.Instance);
        if(spawnList == null){
            Debug.LogError("TemplateMapGenerator: Failed to get the info for _spawns");
        }
        spawnList.SetValue(parentGroup, points.ToArray());
        return true;
#else
        return false;
#endif
    }

    public static bool GenerateMapTemplate(string mapName, int playerCount){
#if NEBULOUS_LOADED
        // Create Map Center
        GameObject mapCenter = new GameObject("Map Center");
        mapCenter.AddComponent<SpacePartitioner>();
        mapCenter.AddComponent<SpacePartitionerBuilder>();
        // Create Battlespace script
        GameObject battleSpace = new GameObject("Battlespace");
        battleSpace.name = mapName;
        battleSpace.transform.parent = mapCenter.transform;
        battleSpace.AddComponent<Battlespace>();
        Battlespace bsScript = battleSpace.GetComponent<Battlespace>();
        Type battlespaceType = bsScript.GetType();

        // Create a default list for the distributed positions otherwise null error will pop
        FieldInfo distributedPoints = battlespaceType.GetField(
                    "_distributedObjectivePositions", BindingFlags.NonPublic | BindingFlags.Instance);
        
        List<Vector3> defaultPoints = 
            new List<Vector3>{new Vector3(500,0,0), new Vector3(0,500,0), new Vector3(0,0,500)};
        distributedPoints.SetValue(bsScript, defaultPoints.ToArray());
        // Do a bunch of configuration here....

        // Create Team A Spawns
        GameObject teamASpawns = new GameObject("Team A Spawns");
        teamASpawns.transform.parent = battleSpace.transform;
        teamASpawns.AddComponent<SpawnGroup>();
        genIndividualSpawns("Team A Spawns", playerCount);
        teamASpawns.transform.position += new Vector3(0,0,-600); // Some initial location
        FieldInfo aSpawns = 
            battlespaceType.GetField("_teamASpawns", BindingFlags.NonPublic | BindingFlags.Instance);
        aSpawns.SetValue(bsScript, teamASpawns.GetComponent<SpawnGroup>());

        // Create Team B Spawns
        GameObject teamBSpawns = new GameObject("Team B Spawns");
        teamBSpawns.transform.parent = battleSpace.transform;
        teamBSpawns.AddComponent<SpawnGroup>();
        genIndividualSpawns("Team B Spawns", playerCount);
        teamBSpawns.transform.position += new Vector3(0,0,600); // Some initial location
        FieldInfo bSpawns = 
            battlespaceType.GetField("_teamBSpawns", BindingFlags.NonPublic | BindingFlags.Instance);
        bSpawns.SetValue(bsScript, teamBSpawns.GetComponent<SpawnGroup>());

        // Create Terrain
        GameObject terrain = new GameObject("Terrain");
        terrain.transform.parent = battleSpace.transform;
        // Create Example Sphere
        // Create Center Objective
        GameObject centerPoint = new GameObject("Center Objective");
        centerPoint.transform.parent = battleSpace.transform;
        FieldInfo centralPosition = 
            battlespaceType.GetField("_centralObjectivePosition", BindingFlags.NonPublic | BindingFlags.Instance);
        centralPosition.SetValue(bsScript, centerPoint);


        // Optional: Create Volumetrics

        // Create Bundle Folder
        // Create manifest.xml
        // Create ModInfo.xml

#endif
        return false;
    }
}
