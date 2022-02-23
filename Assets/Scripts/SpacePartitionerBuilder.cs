using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if NEBULOUS_LOADED
using Game.Map;
[RequireComponent(typeof(SpacePartitioner))]
public class SpacePartitionerBuilder : MonoBehaviour {

    private SpacePartitioner _tree {
        get {
            if (__tree == null)
                __tree = GetComponent<SpacePartitioner>();
            return __tree;
        }
    }
    private SpacePartitioner __tree;

    [ContextMenu("Build Tree")]
    public void Build() {
        _tree.Editor_Build();

    }

    [ContextMenu("Clear Tree")]
    public void ClearTree() {
        _tree.Editor_ClearTree();
    }

    [ContextMenu("Convert Tree To Graph")]
    public void BuildGraph() {
        _tree.Editor_BuildGraph();
    }

#if UNITY_EDITOR
    [ContextMenu("Save Tree to Asset")]
    public void SaveTreeToAsset() {
        string path = EditorUtility.SaveFilePanel("Save Nav Tree", "Assets/", "Map Navigation Tree", "asset");
        if (string.IsNullOrEmpty(path))
            return;

        path = FileUtil.GetProjectRelativePath(path);

        _tree.SerializeTreeToAsset(path);
    }
#endif
}
#endif