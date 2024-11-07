using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Splines;

public class TrackMeshGenerator : MonoBehaviour
{
    private Vector2[] positions;
    [SerializeField, Range(0,10)]
    private float distPerQuad;
    [SerializeField]
    private TrainSplineBuilder builder;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GeneratePositions(Spline spline, float length)
    {
        int sampleAmount = Mathf.CeilToInt(length / distPerQuad);
        positions = new Vector2[sampleAmount];
        float sampleIntervals = 1 / sampleAmount;
        for (int i = 0; i < sampleAmount; i++)
        {
            positions[i] = (Vector3)spline.EvaluatePosition(i * sampleIntervals);
            
        }
    }
}
