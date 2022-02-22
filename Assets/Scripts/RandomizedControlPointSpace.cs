// RandomizedControlPointSpace
// @author: amelim

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Game.Map;


public class RandomizedControlPointSpace : MonoBehaviour
{ 
    [SerializeField]
    protected GameObject[] layers;

    // Start is called before the first frame update
    void Start()
    {

        Battlespace objBattleSpace = gameObject.GetComponentInParent<Battlespace>();
        if(objBattleSpace == null){
            Debug.Log("RCPS: Failed to get Battlespace for map, obj null");
        }

        Debug.LogFormat("RCPS: Loaded Battlespace: {0} GUID: {1}", objBattleSpace.MapName, objBattleSpace.MapKey);
        
        // We need to overload the Distributed Objectives List with a subset of pre-laid objectives
        if(layers.Length > 0){
            // Randomly select a layer to overload the Battlespace Vector3[]
            int randLayer = UnityEngine.Random.Range(0, layers.Length);
            Debug.LogFormat("RCPS: Selecting Layer {0} out of {1} potential layers", randLayer, layers.Length);
            Vector3[] layerArray;
            ControlPointLayer layer;
            if(layers[randLayer].TryGetComponent<ControlPointLayer>(out layer)){
                layerArray = layer.GetDistributedControlPoints();

                // Need to set value of field via reflection since the variable is private
                Type battlespaceType = objBattleSpace.GetType(); // Should be Battlespace type

                FieldInfo distributedPoints = battlespaceType.GetField(
                    "_distributedObjectivePositions", BindingFlags.NonPublic | BindingFlags.Instance);
                if(distributedPoints == null){
                    Debug.LogError("RCPS: Failed to get the info for _distributedObjectivePositions");
                    return;
                }
                // Returns the value of _distributedControlPoints from the Battlespace script
                Vector3[] localBattleSpaceArray = (Vector3[])distributedPoints.GetValue(objBattleSpace);
                /*if(localBattleSpaceArray.Length != layerArray.Length){
                    Debug.LogFormat(
                        "RCPS: The Control Point count {0} in the Battlespace does not match the Layer count {1}", localBattleSpaceArray.Length, 
                        layerArray.Length);
                }*/
                
                // Override the control points
                for(int i=0; i<layerArray.Length; i++){
                    Debug.LogFormat("RCPS: Setting control point {0} to value {1},{2},{3}", i, 
                        layerArray[i].x, layerArray[i].y, layerArray[i].z);
                }

                distributedPoints.SetValue(objBattleSpace, layerArray);
            }
        }
    }


    private void OnDrawGizmos() {
        foreach(GameObject layerGO in layers){
            ControlPointLayer layer;
            bool gotLayer = layerGO.TryGetComponent<ControlPointLayer>(out layer);
                if(gotLayer){
                    foreach(Vector3 vector in layer.GetDistributedControlPoints()){
                        DebugExtension.DebugPoint(
                            vector,
                            layer.GetDebugColor(), 
                            100f, 0f, true);
                    }
            }            
        }
    }
}

