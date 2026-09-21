using System.Collections.Generic;
using UnityEngine;

public class CutScript : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Mesh dollarMesh;
    public Mesh handMesh;
    public Material handMat;
    public Material dollarMat;
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

    //
    Vector3 tearDirection;

    GameObject oneHand;
    GameObject twoHand;

    float kinematicTimer = 1;

    public bool oneHandTriggered = false;
    public bool twoHandTriggered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        dollarMesh = meshFilter.mesh;

        oldTriangles = dollarMesh.triangles;
        oldVertices = dollarMesh.vertices;
        oldNormals = dollarMesh.normals;
        oldUvs = dollarMesh.uv;

        ChooseAngle();

    }

    // Update is called once per frame
    void Update()
    {
        if (oneHandTriggered && twoHandTriggered)
        {
            Cut();
            Destroy(oneHand);
            Destroy(twoHand);
            Destroy(gameObject);
        }
        if (kinematicTimer > 0)
        {
            kinematicTimer -= Time.deltaTime;
        }
        else
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
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

    void TriangleCutReimage(Vector3 vert1, Vector3 vert2, Vector3 opp)
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
        
        int oppInOtherTriangle;
        List<int> intsInOtherTriangle = new List<int>();

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
                    intersectionInts.Add(newIntIndex);
                    newIntIndex = AddNewVertex(transform.InverseTransformPoint(intersection), new Vector3(0.5f,0.5f,0.5f), new Vector2(0,0), rightVertices, rightNormals, rightUvs);
                    intsInOtherTriangle.Add(newIntIndex);
                }
                else
                {
                    newIntIndex = AddNewVertex(transform.InverseTransformPoint(intersection), new Vector3(0.5f,0.5f,0.5f), new Vector2(0,0), rightVertices, rightNormals, rightUvs);
                    intersectionInts.Add(newIntIndex);
                    newIntIndex = AddNewVertex(transform.InverseTransformPoint(intersection), new Vector3(0.5f,0.5f,0.5f), new Vector2(0,0), leftVertices, leftNormals, leftUvs);
                    intsInOtherTriangle.Add(newIntIndex);
                }
                
                
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
            oppInOtherTriangle = newIndex;
        }
        else
        {
            newIndex = AddNewVertex(transform.InverseTransformPoint(vert1), new Vector3(0,0,0), new Vector3(0,0), rightVertices, rightNormals, rightUvs);
            oldVertInts.Add(newIndex);
            newIndex = AddNewVertex(transform.InverseTransformPoint(vert2), new Vector3(0,0,0), new Vector3(0,0), rightVertices, rightNormals, rightUvs);
            oldVertInts.Add(newIndex);
            newIndex = AddNewVertex(transform.InverseTransformPoint(opp), new Vector3(0,0,0), new Vector3(0,0), leftVertices, leftNormals, leftUvs);
            oppInOtherTriangle = newIndex;
        }

        trianglesToFill.Add(oldVertInts[0]);
        trianglesToFill.Add(intersectionInts[1]);
        trianglesToFill.Add(oldVertInts[1]);

        trianglesToFill.Add(oldVertInts[0]);
        trianglesToFill.Add(intersectionInts[0]);
        trianglesToFill.Add(intersectionInts[1]);

        otherTrianglesToFill.Add(oppInOtherTriangle);
        otherTrianglesToFill.Add(intsInOtherTriangle[1]);
        otherTrianglesToFill.Add(intsInOtherTriangle[0]);
    }

    void ChooseAngle()
    {
        float angle = Random.Range(0f,360f);
        tearDirection = Quaternion.Euler(0, angle, 0) * transform.up;
        plane = new Plane(tearDirection, transform.position);

        oneHand = new GameObject();
        oneHand.transform.position = transform.position + tearDirection * 20;
        MoveTowardsScript oneHandMoveScript = oneHand.AddComponent<MoveTowardsScript>();
        MeshRenderer oneHandmRender = oneHand.AddComponent<MeshRenderer>();
        oneHandmRender.material = handMat;
        MeshFilter oneHandmFilter = oneHand.AddComponent<MeshFilter>();
        oneHandmFilter.mesh = handMesh;
        oneHandMoveScript.target = gameObject;
        SphereCollider oneHandTrigger = oneHand.AddComponent<SphereCollider>();
        oneHandTrigger.isTrigger = true;

        twoHand = new GameObject();
        twoHand.transform.position = transform.position + tearDirection * -20;
        MoveTowardsScript twoHandMoveScript = twoHand.AddComponent<MoveTowardsScript>();
        MeshRenderer twoHandmRender = twoHand.AddComponent<MeshRenderer>();
        twoHandmRender.material = handMat;
        MeshFilter twoHandmFilter = twoHand.AddComponent<MeshFilter>();
        twoHandmFilter.mesh = handMesh;
        twoHandMoveScript.target = gameObject;
        SphereCollider twoHandTrigger = twoHand.AddComponent<SphereCollider>();
        twoHandTrigger.isTrigger = true;

    }

    void Cut()
    {

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
                TriangleCutReimage(vert0World, vert1World, vert2World);
            }

            else if (plane.GetSide(vert1World) == plane.GetSide(vert2World))
            {
                TriangleCutReimage(vert1World, vert2World, vert0World);
            }

            else if (plane.GetSide(vert2World) == plane.GetSide(vert0World))
            {
                TriangleCutReimage(vert2World, vert0World, vert1World);
            }
            
        }

        leftMesh = new Mesh();
        leftMesh.vertices = leftVertices.ToArray();
        leftMesh.uv = leftUvs.ToArray();
        leftMesh.normals = leftNormals.ToArray();
        leftMesh.triangles = leftTriangles.ToArray();
        leftMesh.RecalculateNormals();
        leftMesh.RecalculateTangents();
        leftMesh.RecalculateBounds();

        GameObject leftGameObject = new GameObject();
        leftGameObject.transform.position = leftMesh.bounds.center;
        leftGameObject.transform.localScale = Vector3.one * 100f;
        leftGameObject.transform.rotation = Quaternion.Euler(-90,0,0);
        MeshFilter leftMeshFil = leftGameObject.AddComponent<MeshFilter>();
        MeshCollider leftMeshColl = leftGameObject.AddComponent<MeshCollider>();
        leftMeshColl.sharedMesh = leftMesh;
        MeshRenderer leftMeR = leftGameObject.AddComponent<MeshRenderer>();
        leftMeR.material = dollarMat;
        CutScript leftCutScript = leftGameObject.AddComponent<CutScript>();
        leftCutScript.handMesh = handMesh;
        leftCutScript.handMat = handMat;
        leftCutScript.dollarMat = dollarMat;
        Rigidbody leftRB = leftGameObject.AddComponent<Rigidbody>();
        leftRB.useGravity = false;
        leftMeshFil.mesh = leftMesh;
        leftRB.linearDamping = 3;
        leftRB.AddForce(tearDirection * 10, ForceMode.Impulse);

        rightMesh = new Mesh();
        rightMesh.vertices = rightVertices.ToArray();
        rightMesh.uv = rightUvs.ToArray();
        rightMesh.normals = rightNormals.ToArray();
        rightMesh.triangles = rightTriangles.ToArray();
        rightMesh.RecalculateNormals();
        rightMesh.RecalculateTangents();
        rightMesh.RecalculateBounds();

        GameObject rightGameObject = new GameObject();
        rightGameObject.transform.position = rightMesh.bounds.center;
        rightGameObject.transform.localScale = Vector3.one * 100f;
        rightGameObject.transform.rotation = Quaternion.Euler(-90,0,0);
        MeshFilter rightMeshFil = rightGameObject.AddComponent<MeshFilter>();
        MeshRenderer rightMeR = rightGameObject.AddComponent<MeshRenderer>();
        rightMeR.material = dollarMat;
        MeshCollider rightMeshColl = rightGameObject.AddComponent<MeshCollider>();
        rightMeshColl.sharedMesh = rightMesh;
        CutScript rightCutScript = rightGameObject.AddComponent<CutScript>();
        rightCutScript.handMesh = handMesh;
        rightCutScript.handMat = handMat;
        rightCutScript.dollarMat = dollarMat;
        Rigidbody rightRB = rightGameObject.AddComponent<Rigidbody>();
        rightRB.useGravity = false;
        rightMeshFil.mesh = rightMesh;
        rightRB.linearDamping = 3;
        rightRB.AddForce(tearDirection * -10, ForceMode.Impulse);
    }

    
}
