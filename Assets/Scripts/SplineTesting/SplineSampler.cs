using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class SplineSampler : MonoBehaviour
{
    [SerializeField]
    private SplineContainer splineContainer;

    [SerializeField]
    private int splineIndex;
    [SerializeField]
    private float time;

    [SerializeField]
    float3 position;
    float3 tangent;
    float3 upVector;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        splineContainer.Evaluate(splineIndex,time,out position,out tangent, out upVector);
    }

}
