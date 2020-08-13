using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMesh : MonoBehaviour
{
    [SerializeField]
    private int GridSize;

    public float a = 1.0f;

    void Awake()
    {
        if(GridSize % 2 != 0)
        {
            Debug.Log("ERROR: Please enter a even number for GridSize!");
        }
        transform.position = new Vector3(-GridSize / 2, -GridSize / 2, 0);

        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        var vertices = new List<Vector3>();

        var indicies = new List<int>();
        for (int i = 0; i <= GridSize; i++)
        {
            vertices.Add(new Vector3(i, 0, 0));
            vertices.Add(new Vector3(i, GridSize, 0));

            indicies.Add(4 * i + 0);
            indicies.Add(4 * i + 1);

            vertices.Add(new Vector3(0, i, 0));
            vertices.Add(new Vector3(GridSize, i, 0));

            indicies.Add(4 * i + 2);
            indicies.Add(4 * i + 3);
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        filter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        meshRenderer.material.color = new Color(255, 255, 255, a);

    }
}
