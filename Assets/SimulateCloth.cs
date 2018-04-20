using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateCloth : MonoBehaviour {

    const float EPSILON = 0.01f;
    float DAMPING = 0.99f;
    float TOLERANCE = 0.1f;// the lower, the stiffer the cloth
    
    Mesh mesh;

    Vector3 acceleration;
    int[] vertIndicesSorted;
    Vector3[] vertices;
    Vector3[] positions;
    Vector3[] oldPositions;
    float restLength, restLengthDiagonal;
    int sqrtVertCount;

    GameObject[] spheres;
    float[] radiusOther;
    Vector3[] positionOther;

    public Vector3[] lockedPositions;
    public bool[] isPositionLocked;

    // Use this for initialization
    void Start ()
    {
        lockedPositions = new Vector3[4];
        isPositionLocked = new bool[4];

        // Set rotation temporarily to zero to make initializing easier
        Quaternion tempOrientation = transform.rotation;
        transform.rotation = Quaternion.identity;

        // Get positions and radii of possible spheres to collide with
        spheres = GameObject.FindGameObjectsWithTag("Sphere");
        positionOther = new Vector3[spheres.Length];
        radiusOther = new float[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            positionOther[i] = spheres[i].transform.position;
            radiusOther[i] = spheres[i].GetComponent<SphereCollider>().radius * spheres[i].transform.localScale.x;
        }

        // Get cloth mesh vertex positions
        acceleration = new Vector3(0.0f, -9.81f, 0.0f);
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        oldPositions = new Vector3[vertices.Length];
        positions = new Vector3[vertices.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.TransformPoint(vertices[i]);
            oldPositions[i] = positions[i];
        }

        // Calculate rest lengths of edges
        sqrtVertCount = (int)Mathf.Sqrt(vertices.Length);
        restLength = positions[1].x - positions[0].x;
        restLengthDiagonal = Mathf.Sqrt(restLength * restLength + restLength * restLength);

        // Sort vertices
        float halfMeshWidth = (sqrtVertCount * restLength) / 2.0f;
        vertIndicesSorted = new int[vertices.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            float localx = positions[i].x - transform.position.x + halfMeshWidth;
            float localz = positions[i].z - transform.position.z + halfMeshWidth;

            vertIndicesSorted[(int)((localx / restLength)) + sqrtVertCount * (int)((localz / restLength))] = i;
        }
        
        // Set mesh back to actual orientation
        transform.rotation = tempOrientation;
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.TransformPoint(vertices[i]);
            oldPositions[i] = positions[i];
        }

        MakeMeshDoubleFaced();
    }
	
	// Update is called once per frame
	void Update () {
        // Update sphere positions
        for (int i = 0; i < spheres.Length; i++)
        {
            positionOther[i] = spheres[i].transform.position;
        }

        // Unlock cloth corner(s)
        if (Input.GetKeyDown(KeyCode.Alpha1) && isPositionLocked[0])
            isPositionLocked[0] = false;
        if (Input.GetKeyDown(KeyCode.Alpha2) && isPositionLocked[1])
            isPositionLocked[1] = false;
        if (Input.GetKeyDown(KeyCode.Alpha3) && isPositionLocked[2])
            isPositionLocked[2] = false;
        if (Input.GetKeyDown(KeyCode.Alpha4) && isPositionLocked[3])
            isPositionLocked[3] = false;

        vertices = mesh.vertices;

        Vector3 temp, velocity;
        for (int i = 0; i < positions.Length; i++)
        {
            // Transform new local vertex positions to global positions
            positions[i] = transform.TransformPoint(vertices[i]);

            // Perform verlet integration
            temp = positions[i];

            velocity = positions[i] - oldPositions[i];
            
            positions[i] += velocity * DAMPING + acceleration * Time.deltaTime * Time.deltaTime;                        

            oldPositions[i] = temp;
        }

        // Satisfy constraints
        for (int s = 0; s < 100; s++)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                SatisfyEnvironmentConstraints(i);
                SatisfyClothConstraints(i);
            }
            SolveLockedConstraints();
        }

        // Transform new global vertex positions to local positions
        for (int i = 0; i < positions.Length; i++)
        {
            vertices[i] = transform.InverseTransformPoint(positions[i]);
            vertices[i + positions.Length] = vertices[i];
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    void SatisfyClothConstraints(int index)
    {
        // Structural constraints
        if (index + 17 < positions.Length)
            SatisfyDistanceConstraint(index, index + 17, restLength);

        if (index - 17 >= 0)
            SatisfyDistanceConstraint(index, index - 17, restLength);

        if ((index + 1) % 17 != 0 && index + 1 < positions.Length)
            SatisfyDistanceConstraint(index, index + 1, restLength);

        if ((index + 1) % 17 != 1 && index - 1 >= 0)
            SatisfyDistanceConstraint(index, index - 1, restLength);


        // Shear constraints
        if ((index + 1) % 17 != 0 && index + 18 < positions.Length)
            SatisfyDistanceConstraint(index, index + 18, restLengthDiagonal);

        if ((index + 1) % 17 != 1 && index + 16 < positions.Length)
            SatisfyDistanceConstraint(index, index + 16, restLengthDiagonal);

        if ((index + 1) % 17 != 1 && index - 18 >= 0)
            SatisfyDistanceConstraint(index, index - 18, restLengthDiagonal);

        if ((index + 1) % 17 != 0 && index - 16 >= 0)
            SatisfyDistanceConstraint(index, index - 16, restLengthDiagonal);

        //if (Input.GetKeyDown(KeyCode.E) && !pressed)
        //{
        //    pressed = true;
        //    sid += 1;
        //    Debug.Log("!!!!" + sid);
        //}
        //else if (Input.GetKeyDown(KeyCode.Q) && !pressed)
        //{
        //    sid -= 1;
        //    pressed = true;
        //}
        //if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
        //    pressed = false;

        //Vector3 temp = new Vector3(positions[vertIndicesSorted[sid]].x, positions[vertIndicesSorted[sid]].y, positions[vertIndicesSorted[sid]].z);
        //spheres[0].transform.position = temp;
        //positions[3].x = -5.379427f - 19.2f;
        //positions[4].x = -5.379427f + 19.4f;
        //positions[5].x = -6.046001f + 19.4f;
        //Debug.Log(positions[8].x + ", " + positions[7].x + ", " + positions[9].x + ", ");

        //positions[8 + sqrtVertCount].y = 12.5f;       

    }

    void SatisfyDistanceConstraint(int indexA, int indexB, float distance)
    {
        indexA = vertIndicesSorted[indexA];
        indexB = vertIndicesSorted[indexB];

        Vector3 diffVec = positions[indexA] - positions[indexB];
        float dist = diffVec.magnitude;
        float difference = distance - dist;

        // normalize
        diffVec /= dist;

        // Change positions of A and B to satisfy distance constraint
        if (Mathf.Abs(difference) > TOLERANCE)// tolerance is set close to zero
        {
            Vector3 correction = diffVec * (difference * 0.5f);
            positions[indexA] += correction;
            positions[indexB] -= correction;
        }
    }

    void SatisfyEnvironmentConstraints(int index)
    {
        // Platform constraint
        if (positions[index].y < EPSILON)
            positions[index].y = EPSILON;

        // Sphere collision constraints
        for (int i = 0; i < positionOther.Length; i++)
        {
            Vector3 diff = positions[index] - positionOther[i];
            float dist = diff.magnitude;

            //normalize
            diff /= dist;

            if (dist < radiusOther[i])
                positions[index] += diff * (radiusOther[i] - dist + EPSILON);
        }
    }

    void SolveLockedConstraints()
    {
        // Locked to object constraints
        if (isPositionLocked[0])
            positions[vertIndicesSorted[16]] = lockedPositions[0];

        if (isPositionLocked[1])
            positions[vertIndicesSorted[0]] = lockedPositions[1];

        if (isPositionLocked[2])
            positions[vertIndicesSorted[positions.Length - 1]] = lockedPositions[2];

        if (isPositionLocked[3])
            positions[vertIndicesSorted[positions.Length - 17]] = lockedPositions[3];
    }

    public void ChangeTolerance(float newValue)
    {
        TOLERANCE = newValue;
    }

    public void ChangeDamping(float newValue)
    {
        DAMPING = newValue;
    }

    // source: https://answers.unity.com/questions/280741/how-make-visible-the-back-face-of-a-mesh.html
    void MakeMeshDoubleFaced()
    {
        vertices = mesh.vertices;
        var normals = mesh.normals;
        var szV = vertices.Length;
        var newVerts = new Vector3[szV * 2];
        var newNorms = new Vector3[szV * 2];
        for (var j = 0; j < szV; j++)
        {
            // duplicate vertices and uvs:
            newVerts[j] = newVerts[j + szV] = vertices[j];
            // copy the original normals...
            newNorms[j] = normals[j];
            // and revert the new ones
            newNorms[j + szV] = -normals[j];
        }
        var triangles = mesh.triangles;
        var szT = triangles.Length;
        var newTris = new int[szT * 2]; // double the triangles
        for (var i = 0; i < szT; i += 3)
        {
            // copy the original triangle
            newTris[i] = triangles[i];
            newTris[i + 1] = triangles[i + 1];
            newTris[i + 2] = triangles[i + 2];
            // save the new reversed triangle
            var j = i + szT;
            newTris[j] = triangles[i] + szV;
            newTris[j + 2] = triangles[i + 1] + szV;
            newTris[j + 1] = triangles[i + 2] + szV;
        }
        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.triangles = newTris; // assign triangles last!
    }
}
