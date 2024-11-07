using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using Unity.VisualScripting;
using System;
using UnityEngine.Rendering.Universal.Internal;
[Serializable]
public struct Node
{
    public Node(TrainNode connectedNode, int SplineIndex, int KnotIndex, SplineDir connectedDir, float length)
    {
        this.connectedNode = connectedNode;
        this.SplineIndex = SplineIndex;
        this.KnotIndex = KnotIndex;
        this.ConnectedDir = connectedDir;
        this.Length = length;
    }
    public TrainNode connectedNode;
    public int SplineIndex;
    public int KnotIndex;
    public SplineDir ConnectedDir;
    public float Length;
}
public enum SplineDir
{
    forward,
    backward,
}
public class TrainNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Transform headingVector;

    [SerializeField]
    private Vector2 forwardTangent = Vector2.zero;
    [SerializeField]
    private Vector2 selectedTangent;
    [SerializeField]
    private TrainSplineBuilder builder;
    [SerializeField]
    private CircleCollider2D cirCol;

    [SerializedDictionary("node", "knot"),SerializeField]
    private SerializedDictionary<TrainNode, SplineKnotIndex> connectedNodes;

    [SerializeField]
    private List<Node> forwardConnectedNodes = new List<Node>();
    [SerializeField]
    private List<Node> backwardsConnectedNodes = new List<Node>();

    [SerializeField]
    private int rotation;
    public void Initialise(TrainSplineBuilder builder)
    {
        this.builder = builder;
        cirCol.enabled = false;
    }

    public void EnableCol()
    {
        cirCol.enabled = true;
    }

    public bool ContainSameSpline(SplineDir firstDir, SplineDir secondDir, TrainNode snapedNode)
    {
        bool doesContain = false;
        List<Node> listToCheck = firstDir == SplineDir.forward ? forwardConnectedNodes : backwardsConnectedNodes;
        foreach (Node nodeToCheck in listToCheck)
        {
            if(nodeToCheck.connectedNode == snapedNode && nodeToCheck.ConnectedDir == secondDir)
            {
                doesContain = true;
                break;
            }
        }
        return doesContain;
    }

    public void AddNode(Node nodeToAdd, SplineDir WhichListToAdd)
    {
        if(WhichListToAdd == SplineDir.forward)
        {
            forwardConnectedNodes.Add(nodeToAdd);
        }
        else
        {
            backwardsConnectedNodes.Add(nodeToAdd);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!builder.GetIsBuildMode())
            return;
        builder.SetHoverNode(this);
        builder.SetSnapeNode(this);
        if (!builder.GetCurrentNode())
        {
            ShowHoverIcon();
            spriteRenderer.color = Color.green;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!builder.GetIsBuildMode())
            return;
        if (builder.getHoverNode() == this)
            builder.SetHoverNode(null);
        if (builder.getSnapNode() == this)
            builder.SetSnapeNode(null);
        if (!builder.GetCurrentNode())
        {

            ResetColour();
        }
        HideHoverIcon();
    }

    public void ResetColour()
    {
        spriteRenderer.color = Color.white;
    }

    public void ChangeColour(Color colour)
    {
        spriteRenderer.color = colour;
    }
    void Start()
    {
        if (!builder)
            builder = transform.GetComponentInParent<TrainSplineBuilder>();
        
    }

    // Update is called once per frame
    void Update()
    {
        headingVector.right = forwardTangent;
    }
    public Vector2 GetT0()
    {
        return forwardTangent;
    }

    public void SetT0(Vector2 tangentVec)
    {
        forwardTangent = tangentVec.normalized;
        selectedTangent = forwardTangent;
    }

    public void SetRotation(int rotation)
    {
        this.rotation = rotation;
    }

    public int GetRotation()
    {
        return rotation;
    }

    public void SetSelectedTangent(SplineDir dir)
    {
        selectedTangent = dir == SplineDir.forward? forwardTangent: -forwardTangent;
    }
    public Vector2 GetSelectedTangent()
    {
        return selectedTangent;
    }

    public bool IsSameTangent(bool isForward)
    {
        if ((isForward? 1: -1) * selectedTangent == forwardTangent)
        {
            return true;
        }
        return false;
    }
    [SerializeField]
    private GameObject hoverIcon;
    private void ShowHoverIcon()
    {
        hoverIcon.SetActive(true);
    }
    public void HideHoverIcon()
    {
        hoverIcon.SetActive(false);
    }
    public bool IsEmptyNode()
    {
        bool isEmpty = false;
        if (forwardConnectedNodes.Count == 0 && backwardsConnectedNodes.Count == 0)
            isEmpty = true;

        return isEmpty;
    }



}
