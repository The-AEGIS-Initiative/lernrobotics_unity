using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mesh2White : MonoBehaviour
{
    public float a = 1.0f;
    void Awake()
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        meshRenderer.material.color = new Color(255f, 255f, 255f, a);
    }
}
