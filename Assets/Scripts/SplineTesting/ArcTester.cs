using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ArcTester : MonoBehaviour
{
    [SerializeField]
    private GameObject obj1, obj2, obj3, obj4;

    [SerializeField]
    private float A;

    [SerializeField]
    private Vector2 P0 = Vector2.zero;
    [SerializeField]
    private Vector2 P1 = Vector2.right * 10f;

    [SerializeField]
    private Vector2 T0;

    [SerializeField]
    private SplineContainer splines;
    [SerializeField]
    private BezierKnot knot1;
    [SerializeField]
    private BezierKnot knot2;

    [SerializeField]
    private float angle;

    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ContextMenu("attemptMath")]
    private void AttemptMath()
    {
        float a = Mathf.Deg2Rad * A;
        float d = Vector2.Distance(P0, P1);
        float R = d / (2 * (Mathf.Sin(a * 0.5f)));
        Vector2 t0 = Vector2.zero;
        Vector2 t1 = Vector2.zero;
        Vector2 V = P1 - P0;
        V = V.normalized;
        float theta = a * 0.5f;
        t0.x = V.x * Mathf.Cos(-theta) + V.y * Mathf.Sin(-theta);
        t0.y = -V.x * Mathf.Sin(-theta) + V.y * Mathf.Cos(-theta);
        t1.x = V.x * Mathf.Cos(theta) + V.y * Mathf.Sin(theta);
        t1.y = -V.x * Mathf.Sin(theta) + V.y * Mathf.Cos(theta);
        t0.Normalize();
        t1.Normalize();
        float L = ((4.0f * R) / 3.0f) * Mathf.Tan(a / 4.0f);
        Vector2[] controlPoints = { P0, P0 + L * t0, P1 - L * t1, P1 };
        obj1.transform.position = controlPoints[0];
        obj2.transform.position = controlPoints[1];
        obj3.transform.position = controlPoints[2];
        obj4.transform.position = controlPoints[3];
        knot1.Position = (Vector3)controlPoints[0];
        knot2.Position = (Vector3)controlPoints[3];
        knot1.TangentOut = (Vector3)(L * t0);
        knot2.TangentIn = -(Vector3)(L * t1);
        splines[0].Clear();
        splines[0].Add(knot1);
        splines[0].Add(knot2);
        Vector2 D = (P0 - P1).normalized;
        angle =360-Mathf.Rad2Deg * (Mathf.Acos(Vector2.Dot(t0, D))*2);
    }

    [ContextMenu("restrictedAngle")]
    private void AttemptMath2()
    {
        P0 = obj1.transform.position;
        P1 = obj4.transform.position;
        T0 = (obj2.transform.position - obj1.transform.position).normalized;
        Vector2 D = (P0 - P1).normalized;
        angle = 360 - Mathf.Rad2Deg * (Mathf.Acos(Vector2.Dot(T0, D)) * 2);
        Vector2 vec = obj1.transform.position - obj2.transform.position;
        Vector2 dir = obj4.transform.position - obj2.transform.position;

        Vector3 cross = Vector3.Cross(vec, dir);
        if(cross.z < 0)
        {
            angle *= -1;
        }
        float a = Mathf.Deg2Rad * angle;
        float d = Vector2.Distance(P0, P1);
        float R = d / (2 * (Mathf.Sin(a * 0.5f)));
        Vector2 V = P1 - P0;
        Vector2 t1 = Vector2.zero;
        V = V.normalized;
        float theta = a * 0.5f;
        t1.x = V.x * Mathf.Cos(theta) + V.y * Mathf.Sin(theta);
        t1.y = -V.x * Mathf.Sin(theta) + V.y * Mathf.Cos(theta);
        t1.Normalize();
        float L = ((4.0f * R) / 3.0f) * Mathf.Tan(a / 4.0f);
        knot1.Position = (Vector3)P0;
        knot2.Position = (Vector3)P1;
        knot1.TangentOut = (Vector3)(L * T0);
        knot2.TangentIn = -(Vector3)(L * t1);
        splines[0].Clear();
        splines[0].Add(knot1);
        splines[0].Add(knot2);
    }


}
