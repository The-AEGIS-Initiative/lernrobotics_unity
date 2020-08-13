using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GraphOverlay : MonoBehaviour
{
    Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
    Dictionary<string, string[]> graph = new Dictionary<string, string[]>();

    void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Floor")) // Search all floor tiles
        {
            string nodeName = go.GetComponent<GraphNode>().NodeName;
            
            if (nodeName != null && nodeName != "") // If nodeName is not null, tile is a node.
            {
                // Add node to nodes and graph
                Debug.Log(nodeName);
                nodes.Add(nodeName, go);
                graph.Add(nodeName, go.GetComponent<GraphNode>().GetAdjNodes());
            }
        }
        DrawGraph();
    }

    void DrawGraph()
    {
        foreach(KeyValuePair<string, string[]> nodeAdjList in graph)
        {
            string node = nodeAdjList.Key;

            TextMeshPro tmp = nodes[node].GetComponentInChildren<TextMeshPro>();
            tmp.SetText(node);

            string[] adjNodes = nodeAdjList.Value;
            if(adjNodes != null) 
            {
                foreach (string target in adjNodes) // For each adjacent node
                {
                    GameObject go = Instantiate((GameObject)Resources.Load("GraphEdge")); // Instantiate edge prefab
                    GameObject targetNode = nodes[target];

                    // Scale edge length to distance between node and target_node
                    float edgeLength = Vector3.Distance(nodes[node].transform.position, targetNode.transform.position);
                    go.transform.localScale = new Vector3(0.02f, edgeLength, 0);

                    // Position edge halfway between node and target_node
                    go.transform.position = (targetNode.transform.position + nodes[node].transform.position) / 2;
                    go.transform.position += new Vector3(0f, 0f, -5f);

                    // Rotate edge to point from node to target_node
                    go.transform.rotation = Quaternion.LookRotation(targetNode.transform.position - nodes[node].transform.position, new Vector3(0, 0, 1));
                    go.transform.eulerAngles = new Vector3(0f, 0f, go.transform.eulerAngles.z);

                }
            }
        }
    }



}
