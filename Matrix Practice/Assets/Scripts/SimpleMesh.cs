using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class SimpleMesh : MonoBehaviour
{
    [SerializeField] float scaleDelta = 0.001f;
    float scaleFactor = 1f;

    private Mesh mesh;
    private int[] faces;
    private Vector3[] vertices;
    
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        vertices = new[]
        {
            new Vector3(-1f, 0f, 1f),
            new Vector3(1f, 0f, -1f),
            new Vector3(0f, 0f, 2f),
            new Vector3(0f, 3f, 0f)
        };
        
        faces = new int[]
        {
            0, 1, 2,
            0, 3, 1,
            1, 3, 2,
            2, 3, 0
        };
        
        mesh.vertices = vertices;
        mesh.triangles = faces;
        
        mesh.RecalculateNormals();
    }

    void Update()
    {
        Matrix4x4 scaler = HW_Transform.ScaleMat(scaleFactor, scaleFactor, scaleFactor);
        Matrix4x4 move = HW_Transform.TranslationMat(-3,0,5);
        
        Matrix4x4 composite = move * scaler;
        
        Vector3[] modified = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            modified[i] = new Vector4(vertices[i].x, vertices[i].y, vertices[i].z, 1f);
            modified[i] = composite * modified[i];
        }

        mesh.vertices = modified;
        
        scaleFactor += scaleDelta;

    }
}
