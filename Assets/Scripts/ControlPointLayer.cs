using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointLayer : MonoBehaviour
{
    [SerializeField]
    protected Vector3[] distributedControlPoints;

    [SerializeField]
    private Color debugColor;

    void Reset() {
        debugColor = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f);
    }

    public Vector3[] GetDistributedControlPoints(){
        return distributedControlPoints;
    }

    public Color GetDebugColor(){
        return debugColor;
    }
}
