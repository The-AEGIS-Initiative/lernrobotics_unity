using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of node (not a node if null)")]
    private string nodeName;
    public string NodeName
    {
        get { return nodeName; }
        set { nodeName = value; }
    }

    [SerializeField]
    [Tooltip("Adjacent nodes (deliminate with ,)")]
    private string adjNodes;
    public string AdjNodes
    {
        get { return adjNodes; }
        set { adjNodes = value; }
    }

    public string[] GetAdjNodes()
    {
        if (IsLeaf())
        {
            return null;
        }
        return adjNodes.Split(',');
    }

    public bool IsLeaf()
    {
        return adjNodes == null || adjNodes == "";
    }
}
