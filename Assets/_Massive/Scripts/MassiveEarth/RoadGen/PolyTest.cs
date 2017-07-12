using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;

public class PolyTest : MonoBehaviour {

  ConstraintOptions options = new ConstraintOptions() { ConformingDelaunay = false };
  QualityOptions quality = new QualityOptions() { MinimumAngle = 5 };

  public void Test()
  {
    quality = new QualityOptions() { MinimumAngle = 5 };

    Debug.Log("test!");
  
    List<Vector3> v = new List<Vector3>();
    v.Add(new Vector3(0, 0, 0));
    v.Add(new Vector3(10, 0, 95));
    v.Add(new Vector3(250, 0, 140));
    v.Add(new Vector3(200, 0, 20));
    v.Add(new Vector3(150, 0, 60));

    Generate(v);

  }

  void Generate(List<Vector3> vertices)
  {


    Polygon poly = new Polygon();

    foreach( Vector3 v in vertices)
    {
      poly.Add(new Vertex(v.x, v.y, v.z));
    }
    
    var pmesh = poly.Triangulate(options, quality);
    //var pmesh = poly.Triangulate();


    List<Vector3> outvertices = new List<Vector3>();
    foreach (Vertex v in pmesh.Vertices)
    {
      outvertices.Add(new Vector3((float)v.x, (float)v.z, (float)v.y));
    }

    List<int> triangles = new List<int>();
    foreach (Triangle t in pmesh.Triangles)
    {
      triangles.Add(t.GetVertexID(0));
      triangles.Add(t.GetVertexID(2));
      triangles.Add(t.GetVertexID(1));
    }


    UnityEngine.Mesh mesh = new UnityEngine.Mesh();
    mesh.SetVertices(outvertices);
    mesh.SetTriangles(triangles, 0);
    mesh.RecalculateNormals();
    mesh.RecalculateTangents();
    mesh.RecalculateBounds();


    MeshFilter mf = gameObject.AddComponent<MeshFilter>();
    mf.mesh = mesh;
    MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
  }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
