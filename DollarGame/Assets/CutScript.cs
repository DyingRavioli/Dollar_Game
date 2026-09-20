using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UnityEngine;

public class CutScript : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Mesh dollarMesh;
    Plane plane;

    int[] oldTriangles;
    Vector3[] oldVertices;
    Vector3[] oldNormals;
    Vector2[] oldUvs;

    List<int> leftTriangles = new List<int>();
    List<Vector3> leftVertices = new List<Vector3>();
    List<Vector3> leftNormals = new List<Vector3>();
    List<Vector2> leftUvs = new List<Vector2>();

    List<int> rightTriangles = new List<int>();
    List<Vector3> rightVertices = new List<Vector3>();
    List<Vector3> rightNormals = new List<Vector3>();
    List<Vector2> rightUvs = new List<Vector2>();

    //
    Mesh leftMesh;
    Mesh rightMesh;

    public GameObject left;
    public GameObject right;

    List<Vector3> tests = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        dollarMesh = meshFilter.mesh;

        oldTriangles = dollarMesh.triangles;
        oldVertices = dollarMesh.vertices;
        oldNormals = dollarMesh.normals;
        oldUvs = dollarMesh.uv;

        Cut();

        left.GetComponent<MeshFilter>().mesh = leftMesh;
        right.GetComponent<MeshFilter>().mesh = rightMesh;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Vector3 testDebug in tests)
        {
            Debug.DrawRay(testDebug, Vector3.up, Color.green);
        }
        
        //foreach (Vector3 vert in leftVertices)
        //{
        //    Vector3 worldVert = transform.TransformPoint(vert);
        //    print(worldVert);
        //    Debug.DrawRay(worldVert, Vector3.up, Color.green);
        //}
        //foreach (Vector3 vert in rightVertices)
        //{
        //    Vector3 worldVert = transform.TransformPoint(vert);
        //    Debug.DrawRay(worldVert, Vector3.up, Color.red);
        //}
    }

    int AddNewVertex(Vector3 vertex, Vector3 normal, Vector2 uv, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs)
    {
        int index = vertices.IndexOf(vertex);

        if (index < 0)
        {
            index = vertices.Count;

            vertices.Add(vertex);
            normals.Add(normal);
            uvs.Add(uv);
        }

        return index;
    }

    void TriangleCutReimage(Vector3 vert1, Vector3 vert1norm, Vector3 vert2, Vector3 vert2norm, Vector3 opp, Vector2[] triUvs)
    {
        List<int> trianglesToFill = leftTriangles;
        List<int> otherTrianglesToFill = rightTriangles;
        if (!plane.GetSide(vert1))
        {
            trianglesToFill = rightTriangles;
            otherTrianglesToFill = leftTriangles;
        }

        List<int> intersectionInts = new List<int>();
        List<int> oldVertInts = new List<int>();

        Ray[] rays = {new Ray(vert1, opp - vert1), new Ray(vert2, opp - vert2)};

        foreach (Ray ray in rays)
        {
            int newIntIndex;
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 intersection = ray.GetPoint(distance);
                if (plane.GetSide(vert1))
                {
                    newIntIndex = AddNewVertex(transform.InverseTransformPoint(intersection), new Vector3(0.5f,0.5f,0.5f), new Vector2(0,0), leftVertices, leftNormals, leftUvs);
                }
                else
                {
                    newIntIndex = AddNewVertex(transform.InverseTransformPoint(intersection), new Vector3(0.5f,0.5f,0.5f), new Vector2(0,0), rightVertices, rightNormals, rightUvs);
                }
                
                intersectionInts.Add(newIntIndex);
            }
        }

        int newIndex;
        if (plane.GetSide(vert1))
        {
            newIndex = AddNewVertex(transform.InverseTransformPoint(vert1), new Vector3(0,0,0), new Vector3(0,0), leftVertices, leftNormals, leftUvs);
            oldVertInts.Add(newIndex);
            newIndex = AddNewVertex(transform.InverseTransformPoint(vert2), new Vector3(0,0,0), new Vector3(0,0), leftVertices, leftNormals, leftUvs);
            oldVertInts.Add(newIndex);
            newIndex = AddNewVertex(transform.InverseTransformPoint(opp), new Vector3(0,0,0), new Vector3(0,0), rightVertices, rightNormals, rightUvs);
            oldVertInts.Add(newIndex);
        }
        else
        {
            newIndex = AddNewVertex(transform.InverseTransformPoint(vert1), new Vector3(0,0,0), new Vector3(0,0), rightVertices, rightNormals, rightUvs);
            oldVertInts.Add(newIndex);
            newIndex = AddNewVertex(transform.InverseTransformPoint(vert2), new Vector3(0,0,0), new Vector3(0,0), rightVertices, rightNormals, rightUvs);
            oldVertInts.Add(newIndex);
            newIndex = AddNewVertex(transform.InverseTransformPoint(opp), new Vector3(0,0,0), new Vector3(0,0), leftVertices, leftNormals, leftUvs);
            oldVertInts.Add(newIndex);
            }

        trianglesToFill.Add(oldVertInts[0]);
        trianglesToFill.Add(intersectionInts[1]);
        trianglesToFill.Add(oldVertInts[1]);

        trianglesToFill.Add(oldVertInts[0]);
        trianglesToFill.Add(intersectionInts[0]);
        trianglesToFill.Add(intersectionInts[1]);

        otherTrianglesToFill.Add(oldVertInts[2]);
        otherTrianglesToFill.Add(intersectionInts[1]);
        otherTrianglesToFill.Add(intersectionInts[0]);
    }

    void Cut()
    {
        plane = new Plane(transform.up, transform.position);

        for (int triIndex = 0; triIndex < oldTriangles.Length; triIndex += 3)
        {
            int[] triVertInts = {oldTriangles[triIndex], oldTriangles[triIndex+1], oldTriangles[triIndex+2]};
            Vector2[] triUvs = {oldUvs[triVertInts[0]], oldUvs[triVertInts[1]], oldUvs[triVertInts[2]]};
            Vector3[] triNorms = {oldNormals[triVertInts[0]], oldNormals[triVertInts[1]], oldNormals[triVertInts[2]]};

            Vector3 vert0World = transform.TransformPoint(oldVertices[triVertInts[0]]);
            Vector3 vert1World = transform.TransformPoint(oldVertices[triVertInts[1]]);
            Vector3 vert2World = transform.TransformPoint(oldVertices[triVertInts[2]]);

            //all on one side of the cut
            if (plane.GetSide(vert0World) == plane.GetSide(vert1World) && plane.GetSide(vert1World) == plane.GetSide(vert2World))
            {
                if (plane.GetSide(transform.TransformPoint(oldVertices[triVertInts[0]]))) 
                {
                    for (int count = 0; count < 3; count++)
                    {
                        int newIndex = AddNewVertex(oldVertices[triVertInts[count]], triNorms[count], triUvs[count], leftVertices, leftNormals, leftUvs);
                        leftTriangles.Add(newIndex);
                    }
                }
                else
                {
                    for (int count = 0; count < 3; count++)
                    {
                        int newIndex = AddNewVertex(oldVertices[triVertInts[count]], triNorms[count], triUvs[count], rightVertices, rightNormals, rightUvs);
                        rightTriangles.Add(newIndex);
                    }
                }
            }

            else if (plane.GetSide(vert0World) == plane.GetSide(vert1World))
            {
                TriangleCutReimage(vert0World, triNorms[0], vert1World, triNorms[1], vert2World, triUvs);
            }

            else if (plane.GetSide(vert1World) == plane.GetSide(vert2World))
            {
                TriangleCutReimage(vert1World, triNorms[1], vert2World, triNorms[2], vert0World, triUvs);
            }

            else if (plane.GetSide(vert2World) == plane.GetSide(vert0World))
            {
                TriangleCutReimage(vert2World, triNorms[2], vert0World, triNorms[0], vert1World, triUvs);
            }
            
        }

        leftMesh = new Mesh();
        leftMesh.vertices = leftVertices.ToArray();
        leftMesh.uv = leftUvs.ToArray();
        leftMesh.normals = leftNormals.ToArray();
        leftMesh.triangles = leftTriangles.ToArray();
        leftMesh.RecalculateNormals();
        leftMesh.RecalculateTangents();

        rightMesh = new Mesh();
        rightMesh.vertices = rightVertices.ToArray();
        rightMesh.uv = rightUvs.ToArray();
        rightMesh.normals = rightNormals.ToArray();
        rightMesh.triangles = rightTriangles.ToArray();
        rightMesh.RecalculateNormals();
        rightMesh.RecalculateTangents();
    }

    
}
