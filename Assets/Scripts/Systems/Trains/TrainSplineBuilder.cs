using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using UnityEngine.UIElements;
public class TrainSplineBuilder : MonoBehaviour
{    
    [SerializeField]
    private GameObject TrainNode;
    [SerializeField]
    private Transform NodesParent;
    [SerializeField]
    private SplineContainer splines;
    [SerializeField]
    private TrackMeshGenerator trackMeshGenerator;
    // Start is called before the first frame update
    [SerializeField]
    private TrainNode firstNode;
    [SerializeField]
    private SplineDir firstDir;
    [SerializeField]
    private TrainNode secondNode;
    [SerializeField]
    private SplineDir secondDir;
    [SerializeField]
    private TrainNode currentNode;
    [SerializeField]
    private TrainNode HoveredNode;
    [SerializeField]
    private TrainNode SnapNode;
    [SerializeField]
    private bool isSelected = false;
    [SerializeField]
    private bool isPlaceable = true;

    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private float minRadius;
    [SerializeField]
    private bool isBuildMode = false;

    private int currentSplineIndex;
    private int currentKnotIndex;
    private Stack<int> availableSplineIndex = new Stack<int>();
    private float SplineLength;
    void Start()
    {
        GameManager.Instance.InputManager.gameAction.Click.performed += OnClick;
        GameManager.Instance.InputManager.gameAction.RightClick.performed += OnRightClick;
        GameManager.Instance.InputManager.gameAction.ChangeBuildMode.performed += DebugBreak;
        GameManager.Instance.InputManager.gameAction.DebugButton.performed += DebugSpawnFirstPoint;
        GameManager.Instance.InputManager.gameAction.ScrollWheel.performed += OnScroll;
        generateHeadingVectors();
    }

    // Update is called once per frame
    void Update()
    {        
        if (isSelected)
        {
            SplineGenerate();
        }
        else if(currentNode != null && !SnapNode)
        {
            currentNode.transform.position = InputManager.mousePosInWorld;
        }
    }
    public void EnterBuildMode()
    {
        isBuildMode = true;
    }
    private void DebugSpawnFirstPoint(InputAction.CallbackContext context)
    {
        firstNode = Instantiate(TrainNode,InputManager.mousePosInWorld,quaternion.identity,NodesParent).GetComponent<TrainNode>();
        firstNode.Initialise(this);
        currentNode = firstNode;
        SetRotationValues();
        BuildState = 1;
        isBuildMode = true;

    }

    private void DebugBreak(InputAction.CallbackContext context)
    {
        Debug.Break();
        /*int current = (int)currentBuildMode;
        current += 1;
        current = current % Enum.GetValues(typeof(BuildMode)).Length;
        currentBuildMode = (BuildMode)current;
        GenerateKnots();*/
    }
    [SerializeField]
    private int BuildState = 0;
    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (firstNode && !secondNode)
        {
            isBuildMode = false;
            Destroy(firstNode.gameObject);
            ResetBuildMode();
        }
        else if(currentNode && secondNode)
        {
            if(firstNode.IsEmptyNode())
                Destroy(firstNode.gameObject);
            firstNode = currentNode;
            secondNode = null;
            BuildState = 1;
            isSelected = false;

            splines[currentSplineIndex].Clear();
            availableSplineIndex.Push(currentSplineIndex);
        }
    }
    private void OnClick(InputAction.CallbackContext context)
    {
        switch (BuildState)
        {
            case 1:
                SelectNode(firstNode);
                firstNode.EnableCol();
                break;
            case 2:
                SelectNode(HoveredNode);
                HoveredNode = null;
                break;
            case 3:
                if (!isPlaceable)
                    return;
                
                firstNode.AddNode(new Node(secondNode, currentSplineIndex, 0, secondDir, SplineLength), firstDir);
                if(!SnapNode)
                    secondNode.AddNode(new Node(firstNode, currentSplineIndex, currentKnotIndex, firstDir, SplineLength), secondDir);
                else
                {
                    SnapNode.AddNode(new Node(firstNode, currentSplineIndex, currentKnotIndex, firstDir, SplineLength), secondDir);
                    Destroy(secondNode.gameObject);
                    secondNode = SnapNode;
                    secondNode.SetSelectedTangent(secondDir == SplineDir.forward? SplineDir.backward: SplineDir.forward);
                }
                firstNode.ResetColour();
                currentNode.EnableCol();
                firstNode = secondNode;
                SelectNode(firstNode);
                break;
        }
    }
    private void ResetBuildMode()
    {
        isSelected = false;
        firstNode.ResetColour();
        firstNode = null;
        secondNode = null;
        currentNode.EnableCol();
        currentNode = null;
        BuildState = 0;
    }


    #region Spline Generation
    [SerializeField]
    private Transform debugCircle;
    /// <summary>
    /// normalised forward tangent direction of the first point
    /// for bezier curve control point
    /// </summary>
    private Vector2 Tfirst;
    /// <summary>
    /// position of the first control point a bezier curve
    /// </summary>
    private Vector2 Pfirst;
    private void SplineGenerate()
    {
        if(!SnapNode)
            currentNode.transform.position = InputManager.mousePosInWorld;
        
        Vector2 Pfinal = currentNode.transform.position;
        Vector2 Tfinal = secondNode.GetT0();
        bool isBehind = false;
        if (Vector2.Dot(Tfirst,Pfinal - Pfirst) < 0)
        {
            isBehind = true;
        }
        float angle = Vector2.SignedAngle(Tfinal, Tfirst);
        Vector2 dir = Pfinal - Pfirst;
        Vector3 cross = Vector3.Cross(Tfirst, dir);
        bool isRight = cross.z <= 0? true : false;
        if (cross.z <= 0 && angle < 0)
        {
            angle += 360;
        }
        else if (cross.z > 0 && angle > 0)
        {
            angle -= 360;
        }
        float a = Mathf.Abs(angle);
        angle *= Mathf.Deg2Rad;
        bool cantPlace;
        KnotsPerSpline(3);
        if(a == 0)
        {
            if (isBehind)
                cantPlace = SplineReflex(Pfirst, Pfinal, Tfirst, Tfinal, angle, isBehind, isRight);
            else
                cantPlace = SplineStraight(Pfirst, Pfinal, Tfirst);
        }
        else if (a == 180)
        {
            
            cantPlace = SplineOneEighty(Pfirst, Pfinal, Tfirst, isBehind);
        }
        else if (a > 0 && a < 180)
        {
            cantPlace = SplineAcute(Pfirst, Pfinal, Tfirst, Tfinal, angle);
        }
        else
        {
            cantPlace = SplineReflex(Pfirst, Pfinal, Tfirst, Tfinal, angle, isBehind, isRight);
        }
        if (SnapNode)
        {
            cantPlace = firstNode.ContainSameSpline(firstDir, secondDir, SnapNode);
        }
        IsPlaceable(cantPlace);
        SplineLength = splines.CalculateLength(currentSplineIndex);
        trackMeshGenerator.GeneratePositions(splines[currentSplineIndex], SplineLength);
    }

    private bool SplineReflex(Vector2 P0, Vector2 P1, Vector2 T0, Vector2 T1, float angle,bool IsP1Behind,bool IsRight, int index = 0)
    {
        float a = angle * Mathf.Rad2Deg;
        Vector2 Pmid;
        bool CantPlace = false;
        if(Mathf.Abs(a) <= 270 || IsP1Behind)
        {
            //big cirlce
            Vector2 perpendicular = Vector2.Perpendicular(T1).normalized;
            LinesUtils.Intersect(P1,P1 + T1, P0,P0 + T0, out Vector2 intersection , LinesUtils.Mode.Lines);
            float D = Vector2.Distance(intersection, P1);
            float R = a == 0? minRadius : D * Mathf.Sin(-angle);
            R = Mathf.Abs(R);
            if(Vector2.Dot(T0, perpendicular) < 0 || (IsRight && angle == 0))
            {
                perpendicular *= -1;
            }
            Vector2 O = P1 + perpendicular.normalized * R;
            Pmid = Vector2.Perpendicular(T0).normalized;

            
            if (angle > 0 || (IsRight && angle == 0))
            {
                Pmid *= -1;
                angle -= Mathf.PI;
            }
            else
            {
                angle += Mathf.PI;
            }
            debugCircle.position = Pmid;
            Pmid = O + Pmid * R;
            bool isBehind = false;
            if (Vector2.Dot(T0, Pmid - P0) < 0)
            {
                isBehind = true;
            }
            CantPlace = SplineOneEighty(P0, Pmid, T0, isBehind);
            AddKnots(1);
            (Vector2 Pfrist, Vector2 Tfrist, Vector2 PLast, Vector2 Tlast, bool boolone) = CurveCalculation(Pmid, P1, -T0, T1, angle);
            SetKnots(Pfrist, Tfrist, PLast, Tlast, 2);
        }
        else
        {
            //straight into curve
            Vector2 perpendicular = Vector2.Perpendicular(T1).normalized;
            if (Vector2.Dot(T0, perpendicular) > 0)
            {
                perpendicular *= -1;
            }
            Vector2 O = P1 + perpendicular * minRadius;
            debugCircle.position = O;
            Pmid = Vector2.Perpendicular(T0).normalized;
            if(angle > 0)
            {
                Pmid *= -1;
            }
            Pmid = O + Pmid * minRadius;

            CantPlace = SplineStraight(P0, Pmid, T0, index);
            
            AddKnots(1);
            angle = angle < 0 ? angle + 180 : angle - 180;
            (Vector2 Pfrist, Vector2 Tfrist, Vector2 PLast, Vector2 Tlast, bool boolOne) = CurveCalculation(Pmid,P1,T0,T1,angle);
            SetKnots(Pfrist, Tfrist, PLast, Tlast, 2);
        }
        return CantPlace;
    }
    private bool SplineAcute(Vector2 P0, Vector2 P1, Vector2 T0, Vector2 T1, float angle, int index = 0)
    {
        Vector2 Pmid;
        bool cantPlace = false;
        Vector2 Pfirst, Tfirst, Plast, Tlast;
        LinesUtils.Intersect(P0, P0 + T0, P1,P1 + T1, out Pmid, LinesUtils.Mode.Lines);
        float D0 = Vector2.Distance(P0, Pmid);
        float D1 = Vector2.Distance(P1, Pmid);
        float Dmid;
        if ( D1 > D0)
        {
            //straight line goes second
            Dmid = D1 - D0;
            Pmid = (P1 - (T1 * Dmid));
            float a = Vector2.Angle(T0, P1 - P0);
            if (Mathf.Abs(angle * Mathf.Rad2Deg) < a)
            {
                //if P1 is outside of the angular space defined by t0 and rotation curve then recursive call
                Pmid = P0 + ((P1 - P0) * 0.5f);
                (Pfirst, Tfirst, Plast, Tlast, cantPlace) = CurveCalculation(P0, Pmid, T0);
                index = SetKnots(Pfirst, Tfirst, Plast, Tlast, index);
                float angle2 = -Vector2.SignedAngle(-Tlast, T1) * Mathf.Deg2Rad;
                AddKnots(1);
                bool place2 = SplineAcute(Pmid, P1, -Tlast.normalized, T1, angle2, index);
                cantPlace = place2 || cantPlace ;
            }
            else
            {
                (Pfirst, Tfirst, Plast, Tlast, cantPlace) = CurveCalculation(P0, Pmid, T0, T1, angle);
                index = SetKnots(Pfirst, Tfirst, Plast, Tlast, index);
                SetKnots(Plast, -Tlast.normalized, P1, Tlast.normalized, index);
            }
        }
        else
        {
            //straight line goes first
            Dmid = D0 - D1;
            Pmid = (P0 + T0 * Dmid);
            index = SetKnots(P0, T0, Pmid, -T0, index);
            (Pfirst, Tfirst, Plast, Tlast, cantPlace) = CurveCalculation(Pmid, P1, T0, T1, angle);
            SetKnots(Pfirst, Tfirst, Plast, Tlast, index);
            if(Vector2.Dot(T0,(P1-Pmid).normalized) < 0)
            {
                cantPlace = true;
            }
        }

        //debugCircle.position = Pmid;
        return cantPlace;
        //IsPlaceable(isBehind);

    }
    private bool SplineOneEighty(Vector2 P0, Vector2 P1, Vector2 T0, bool isBehind, int index = 0)
    {
        bool cantPlace;
        Vector2 Pmid;
        Vector2 Pfirst, Tfirst, Plast, Tlast;
        if (!isBehind)
        {
            LinesUtils.Intersect(P0, P0 + T0, P1, P1 + Vector2.Perpendicular(T0), out Pmid, LinesUtils.Mode.Lines);
            index = SetKnots(P0, T0, Pmid, -T0, index);
            (Pfirst, Tfirst, Plast, Tlast, cantPlace) = CurveCalculation(Pmid, P1, T0);
            SetKnots(Pfirst, Tfirst, Plast, Tlast, index);
        }
        else
        {
            LinesUtils.Intersect(P1, P1 + T0, P0, P0 + Vector2.Perpendicular(T0), out Pmid, LinesUtils.Mode.Lines);
            (Pfirst, Tfirst, Plast, Tlast, cantPlace) = CurveCalculation(P0, Pmid, T0);
            index =SetKnots(Pfirst, Tfirst, Plast, Tlast, index);
            SetKnots(Pmid, -T0, P1, T0, index);
        }
        //debugCircle.position = Pmid;
        return cantPlace;
        
    }
    private bool SplineStraight(Vector2 P0,Vector2 P1, Vector2 T0, int index = 0)
    {
        Vector2 midpoint = P0 + (P1 - P0)* 0.5f;
        (Vector2 Pfrist, Vector2 Tfrist, Vector2 PLast, Vector2 Tlast, bool boolOne) = CurveCalculation(P0, midpoint, T0);
        index = SetKnots(Pfrist, Tfrist, PLast, Tlast, index);
        bool boolTwo;
        (Pfrist, Tfrist, PLast, Tlast, boolTwo) = CurveCalculation(P1, midpoint, -T0);
        SetKnots(PLast, Tlast, Pfrist, Tfrist, index);
        return boolOne || boolTwo;
    }
    /// <summary>
    /// When placeable is true, disallow the placement of the node
    /// as a fail condition is true
    /// </summary>
    /// <param name="placeable"></param>
    private void IsPlaceable(bool placeable)
    {
        if(placeable)
        {
            isPlaceable = false;
            secondNode.ChangeColour(Color.red);
        }
        else
        {
            isPlaceable = true;
            secondNode.ChangeColour(Color.cyan);
        }
    }
    #endregion
    #region Geometric curve approximation
    /// <summary>
    /// generates four vectors defining a bezier curve that approximates a geometric curve.
    /// Returns in order of P0, T0, P1, T1.
    /// T0 and T1 are not positions but a direction that defines the 2nd and 3rd control points
    /// used by the unity spline system
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P1"></param>
    /// <param name="T0"></param>
    /// <returns></returns>
    private (Vector2, Vector2, Vector2, Vector2, bool) CurveCalculation(Vector2 P0, Vector2 P1, Vector2 T0)
    {

        Vector2 D = (P0 - P1).normalized;
        float angle = 360 - Mathf.Rad2Deg * (Mathf.Acos(Vector2.Dot(T0, D)) * 2);
        Vector2 vec = T0;
        Vector2 dir = P1 - P0;
        Vector3 cross = Vector3.Cross(vec, dir);
        if (cross.z > 0)
        {
            angle *= -1;
        }
        return CurveCalculation(P0, P1, T0, Mathf.Deg2Rad * angle);
    }
    /// <summary>
    /// generates four vectors defining a bezier curve that approximates a geometric curve.
    /// Returns in order of P0, T0, P1, T1.
    /// T0 and T1 are not positions but a direction that defines the 2nd and 3rd control points
    /// used by the unity spline system
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P1"></param>
    /// <param name="T0"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    private (Vector2, Vector2, Vector2, Vector2, bool) CurveCalculation(Vector2 P0, Vector2 P1, Vector2 T0, float angle)
    {
        Vector2 V = P1 - P0;
        Vector2 t1 = Vector2.zero;
        V = V.normalized;
        float theta = angle * 0.5f;
        t1.x = V.x * Mathf.Cos(theta) + V.y * Mathf.Sin(theta);
        t1.y = -V.x * Mathf.Sin(theta) + V.y * Mathf.Cos(theta);
        t1.Normalize();

        return CurveCalculation(P0, P1, T0, t1, angle);
    }
    /// <summary>
    /// generates four vectors defining a bezier curve that approximates a geometric curve.
    /// Returns in order of P0, T0, P1, T1.
    /// T0 and T1 are not positions but a direction that defines the 2nd and 3rd control points
    /// used by the unity spline system
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P1"></param>
    /// <param name="T0"></param>
    /// <param name="T1"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    private (Vector2, Vector2, Vector2, Vector2, bool) CurveCalculation(Vector2 P0, Vector2 P1, Vector2 T0, Vector2 T1, float angle)
    {
        float d = Vector2.Distance(P0, P1);
        float R = d / (2 * (Mathf.Sin(angle * 0.5f)));
        float L = ((4.0f * R) / 3.0f) * Mathf.Tan(angle / 4.0f);
        bool cantPlace = (math.abs(R) < minRadius || math.abs(angle * Mathf.Rad2Deg) > maxAngle);
        return (P0, L * T0, P1, -(L * T1), cantPlace);
    }
    #endregion
    #region Knot Manipulations
    private int SetKnots(Vector3 P0, Vector3 T0, Vector3 P1, Vector3 T1, int KnotIndex = 0)
    {
        BezierKnot knot = splines[currentSplineIndex][KnotIndex];
        knot.Position = P0;
        knot.TangentOut = T0;
        knot.Rotation = quaternion.identity;
        splines[currentSplineIndex].SetKnot(KnotIndex, knot);
        knot.Position = P1;
        knot.TangentIn = T1;
        splines[currentSplineIndex].SetKnot(KnotIndex + 1, knot);
       /* if (secondNode.GetAssociatedKnot(firstNode).Knot == knotIndex + 1)
        secondNode.SetT0(-T1);*/
        return KnotIndex + 1;
    }
    /// <summary>
    /// Generate the initial require amount of knots for the basic spline shape
    /// </summary>
    /// <param name="amount"></param>
    private void KnotsPerSpline(int amount)
    {
        //create new bezier knots
        BezierKnot knot = new BezierKnot();
        knot.Position = Vector3.zero;
        knot.Rotation = Quaternion.identity;
        //modify global variables
        currentKnotIndex = amount - 1;
        //clear and recreate the knots for the spline
        splines[currentSplineIndex].Clear();
        for (int i = 0; i < amount; i++)
        {
            splines[currentSplineIndex].Add(knot);
        }
    }
    /// <summary>
    /// Adds knots to the current selected spline to generate
    /// more complex spline shapes
    /// </summary>
    /// <param name="amount">The amount of extra knots to add</param>
    private void AddKnots(int amount)
    {

        for (int i = 0; i < amount; i++)
        {
            splines[currentSplineIndex].Add(new BezierKnot());
        }
        currentKnotIndex += amount;

    }
    #endregion
    #region Selecting nodes for spline
    /// <summary>
    /// Sets the first node in the spline generation
    /// </summary>
    /// <param name="node"></param>
    public void SelectNode(TrainNode node)
    {
        if(SnapNode && !secondNode)
        {
            firstNode = SnapNode;
            Destroy(node.gameObject);
        }
        else
            firstNode = node;
        firstNode.SetSelectedTangent(firstDir);
        isSelected = true;
        secondNode = Instantiate(TrainNode, InputManager.mousePosInWorld, quaternion.identity,NodesParent).GetComponent<TrainNode>();
        secondNode.Initialise(this);
        secondDir = SplineDir.backward;
        if(availableSplineIndex.Count != 0)
        {
            currentSplineIndex = availableSplineIndex.Pop();
        }
        else
        {
            splines.AddSpline();
            currentSplineIndex = splines.Splines.Count - 1;
        }

        currentNode = secondNode;
        SetKnotOne();
        
        BuildState = 3;
    }
    /// <summary>
    /// Sets global values for the first node to be used for calculations.
    /// Also sets the rotation of the second node to be
    /// the same direction of the first node.
    /// </summary>
    private void SetKnotOne()
    {
        Tfirst = firstNode.GetSelectedTangent();
        //determins which direction the first knot faces
        //firstDir = (Tfirst == firstNode.GetT0()) ? SplineDir.forward : SplineDir.backward;
        Pfirst = firstNode.transform.position;
        rotation = firstNode.GetRotation();
        if (Tfirst != firstNode.GetT0())
        {
            rotation = (rotation + Mathf.CeilToInt(maxRotations * 0.5f)) % maxRotations;
        }
        SetRotationValues();

    }
    #endregion
    #region Scrolling
    /// <summary>
    /// number representing which direction to face.
    /// 0 represents facing directly right
    /// </summary>
    [SerializeField]
    private int rotation = 0;
    /// <summary>
    /// the maximum amount of numbers used to represent the rotation.
    /// </summary>
    [SerializeField,Tooltip("Please select numbers divisible by 2 with no remainders")]
    private int maxRotations;
    /// <summary>
    /// Arrays holding unit vectors pointing in all possible direction
    /// determined by maxRotations.
    /// position 0 holds the vector pointing right.
    /// </summary>
    [SerializeField]
    private Vector2[] headingVectors;
    /// <summary>
    /// Modifying the rotation of the current node based on scroll direction
    /// </summary>
    /// <param name="context"></param>
    private void OnScroll(InputAction.CallbackContext context)
    {
        //escape the function when not applicable
        if (currentNode == null && firstNode == null)
            return;
        //if there is a snapNode, restrict the rotation to flipping 180 degrees from scrolling
        if (SnapNode)
        {
            if(secondNode)
                secondDir = secondDir == SplineDir.forward ? SplineDir.backward : SplineDir.forward;
            else
            {
                firstDir = firstDir == SplineDir.forward ? SplineDir.backward : SplineDir.forward;
                currentNode.SetSelectedTangent(firstDir);
            }
            rotation = (rotation + (Mathf.RoundToInt(maxRotations * 0.5f))) % maxRotations;   
            
        }

        //otherwise just rotate by 360/maxrotation degrees
        else
        {
            rotation += (int)context.ReadValue<float>();
            rotation += maxRotations;
            rotation %= maxRotations;
        }
        SetRotationValues();
    }
    /// <summary>
    /// modify the stored rotation values of the second node
    /// </summary>
    private void SetRotationValues()
    {

        currentNode.SetT0(headingVectors[rotation]);
        currentNode.SetRotation(rotation);
    }
    /// <summary>
    /// Generates all the units vectors representing each possible
    /// direction at start.
    /// </summary>
    private void generateHeadingVectors()
    {
        headingVectors = new Vector2[maxRotations];
        float a = 360f / maxRotations;
        a *= Mathf.Deg2Rad;
        for (int i = 0; i < maxRotations; i++)
        {
            headingVectors[i] = new Vector2(Mathf.Cos(a * i),Mathf.Sin(a * i));
        }
    }
    #endregion
    #region Node setting and getting
    public TrainNode GetCurrentNode()
    {
        return currentNode;
    }
    public bool GetIsBuildMode()
    {
        return isBuildMode;
    }
    #region HoverNode
    /// <summary>
    /// Select the current hovered node.
    /// Used to determine what node to select to start
    /// spline generation
    /// </summary>
    /// <param name="HoverNode"></param>
    public void SetHoverNode(TrainNode HoverNode)
    {
        //if no hovernode, reset build state so that no spline is to be built
        if(HoverNode == null && BuildState == 2)
        {
            BuildState = 0;
            HoveredNode = null;
        }
        //if no node is selected and the hovered node is valid
        //set the build state to 2
        else if(!isSelected && HoverNode != firstNode && BuildState != 1)
        {
            BuildState = 2;
            HoveredNode = HoverNode;
        }
    }

    public TrainNode getHoverNode()
    {
        return HoveredNode;
    }
    #endregion
    #region SnapNode
    /// <summary>
    /// Sets the node that the spline end point should attempt to snap to
    /// </summary>
    /// <param name="trainNode">the node that is hovered over</param>
    public void SetSnapeNode(TrainNode trainNode)
    {
        //null node meaning that we have moved off the boundary for snapping
        //resets the secondDir back to backwards
        //as we are creating a new node, backward is default
        if (trainNode == null)
        {
            secondDir = SplineDir.backward;
            SnapNode = null;
        }
        //cant snap to starting node
        else if (trainNode == firstNode)
            return;
        //sets the snap node
        else
        {
            //sets variable and locks position
            SnapNode = trainNode;
            currentNode.transform.position = SnapNode.transform.position;
            //aquire the rotation value of the selected snap node
            int rotation = SnapNode.GetRotation();
            //decides which facing direction
            //required to determine end point direction for spline generation
            int r = (this.rotation + maxRotations - rotation) % maxRotations;
            if (r > maxRotations * 0.25f && r < maxRotations * 0.75f)
            {
                this.rotation = (rotation + Mathf.RoundToInt(maxRotations * 0.5f)) % maxRotations;
                if (secondNode)
                    secondDir = SplineDir.forward;
                else
                    firstDir = SplineDir.backward;
            }
            else
            {
                this.rotation = rotation;
                if(secondNode)
                    secondDir = SplineDir.backward;
                else 
                    firstDir = SplineDir.forward;
            }
            //modify the rotation value of the second node to represent the new dir
            SetRotationValues();
        }
    }
    public TrainNode getSnapNode()
    {
        return SnapNode;
    }
    #endregion
    #endregion
    #region Utility
    public Spline GetCurrentSpline()
    {
        return splines[currentSplineIndex];
    }

    public float GetSplineLength()
    {
        return SplineLength;
    }
    #endregion
}
