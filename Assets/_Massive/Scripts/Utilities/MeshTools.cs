using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;
using TriangleNet.Meshing;
using TriangleNet.Geometry;
using System;

namespace _Massive
{
  public class MeshTools : MonoBehaviour
  {

    /** 
     * <summary>
     * Finds the floorplan angle between two vectors, allows negative values 
     * </summary>
     * **/
    public static float AngleBetween2D(Vector3 A, Vector3 B)
    {
      Vector2 AV2 = new Vector2(A.x, A.z);
      Vector2 BV2 = new Vector2(B.x, B.z);
      Vector2 diference = AV2 - BV2;
      float sign = (BV2.y < AV2.y) ? -1.0f : 1.0f;
      return Vector2.Angle(Vector2.right, diference) * sign;
    }

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
      Vector3 P = x * Vector3.Normalize(B - A) + A;
      return P;
    }

    public static Vector3 GetNormal(Vector3 p1, Vector3 newPos, Vector3 p2)
    {
      if (newPos == p1 || newPos == p2)
      {
        var n = (p2 - p1).normalized;
        return new Vector3(-n.z, 0, n.x);
      }

      var b = (p2 - newPos).normalized + newPos;
      var a = (p1 - newPos).normalized + newPos;
      var t = (b - a).normalized;

      if (t == Vector3.zero)
      {
        var n = (p2 - p1).normalized;
        return new Vector3(-n.z, 0, n.x);
      }

      return new Vector3(-t.z, 0, t.x);
    }

    /**
     * Sample a mesh and return the highest point at position v
     * */
    public static float GetMeshHeight(DRect TileBoundsMeters, Vector3 TilePos, MeshCollider mc, Vector3 v)
    {
      RaycastHit HitInfo;

      Vector3 pos = new Vector3((float)v.x, 9000.0f, (float)v.z);
      Vector3 cpos = new Vector3(
        Mathf.Clamp(pos.x, 0, (float)TileBoundsMeters.Size.x),
        pos.y,
        Mathf.Clamp(pos.z, 0, (float)TileBoundsMeters.Size.z));
      Ray ray = new Ray(cpos + TilePos, -Vector3.up);
      mc.Raycast(ray, out HitInfo, 12000);

      //5 * (int)Math.Round(p / 5.0)
      //float snapped = YSnap * (int)Mathd.Round(HitInfo.point.y / YSnap);
      return HitInfo.point.y;
    }

    /**
     * Sample a mesh and return the highest point at position v
     * */
    public static float GetMeshHeightSimple(MeshCollider mc, Vector3 v)
    {
      RaycastHit HitInfo;

      Vector3 pos = new Vector3((float)v.x, 9000.0f, (float)v.z);      
      Ray ray = new Ray(pos, -Vector3.up);
      mc.Raycast(ray, out HitInfo, 12000);

      //5 * (int)Math.Round(p / 5.0)
      //float snapped = YSnap * (int)Mathd.Round(HitInfo.point.y / YSnap);
      return HitInfo.point.y;
    }

    public static bool IsClockwise(IList<Vector3> vertices)
    {
      double sum = 0.0;
      for (int i = 0; i < vertices.Count; i++)
      {
        Vector3 v1 = vertices[i];
        Vector3 v2 = vertices[(i + 1) % vertices.Count]; // % is the modulo operator
        sum += (v2.x - v1.x) * (v2.z + v1.z);
      }
      return sum > 0.0;
    }

    public static UnityEngine.Mesh Triangulate(List<List<Vector3>> polygons)
    {

      var data = new List<int>();
      var polygon = new Polygon();
      Vertex firstVert = null;
      Vertex nextVert = null;
      Vertex currentVert = null;

      UnityEngine.Mesh m = new UnityEngine.Mesh();
      List<List<int>> Triangles = new List<List<int>>();
      List<int> Edges = new List<int>();
      List<Vector3> Vertices = new List<Vector3>();

      foreach (var sub in polygons)
      {
        if (IsClockwise(sub))
        {
          nextVert = null;
          var wist = new List<Vector3>();
          for (int i = 0; i < sub.Count; i++)
          {
            if (nextVert == null)
            {
              currentVert = new Vertex(sub[i].x, sub[i].y, sub[i].z);
              nextVert = new Vertex(sub[i + 1].x, sub[i].y, sub[i + 1].z);
            }
            else
            {
              currentVert = nextVert;
              if (i == sub.Count - 1)
              {
                nextVert = firstVert;
              }
              else
              {
                nextVert = new Vertex(sub[i + 1].x, sub[i + 1].y, sub[i + 1].z);
              }
            }

            if (i == 0)
              firstVert = currentVert;

            wist.Add(sub[i]);
            polygon.Add(currentVert);
            polygon.Add(new Segment(currentVert, nextVert));
          }
        }
        else
        {
          var cont = new List<Vertex>();
          var wist = new List<Vector3>();
          for (int i = 0; i < sub.Count; i++)
          {
            wist.Add(sub[i]);
            cont.Add(new Vertex(sub[i].x, sub[i].y, sub[i].z));
          }
          polygon.Add(new Contour(cont), true);
        }
      }

      var mesh = polygon.Triangulate();
      //smoother mesh with smaller triangles and extra vertices in the middle
      //var mesh = (TriangleNet.Mesh)polygon.Triangulate(options, quality);

      foreach (var tri in mesh.Triangles)
      {
        data.Add(tri.GetVertexID(0));
        data.Add(tri.GetVertexID(2));
        data.Add(tri.GetVertexID(1));
      }

      foreach (var edge in mesh.Edges)
      {
        if (edge.Label == 0)
          continue;

        Edges.Add(edge.P0);
        Edges.Add(edge.P1);
      }

      Vertices.Clear();
      using (var sequenceEnum = mesh.Vertices.GetEnumerator())
      {
        while (sequenceEnum.MoveNext())
        {
          Vertices.Add(new Vector3((float)sequenceEnum.Current.x, (float)sequenceEnum.Current.z, (float)sequenceEnum.Current.y));
        }
      }
      Triangles.Add(data);

      m.SetVertices(Vertices);
      for (int i = 0; i < Triangles.Count; i++)
      {
        var triangle = Triangles[i];
        m.SetTriangles(triangle, i);
      }

      m.RecalculateNormals();

      return m;
    }

    public static UnityEngine.Mesh Triangulate2(List<List<Vector3>> polygons)
    {
      UnityEngine.Mesh m = new UnityEngine.Mesh();
      List<List<int>> Triangles = new List<List<int>>();
      List<int> Edges = new List<int>();
      List<Vector3> Vertices = new List<Vector3>();

      ConstraintOptions options = new ConstraintOptions() { ConformingDelaunay = false };
      QualityOptions quality = new QualityOptions() { MinimumAngle = 25.0 };

      //if (polygons.Count < 3)
      //        return null;

      var data = new List<int>();
      var polygon = new Polygon();
      Vertex firstVert = null;
      Vertex nextVert = null;
      Vertex currentVert = null;

      /*
      foreach (var sub in polygons)
      {
        if (IsClockwise(sub))
        {
          nextVert = null;
          var wist = new List<Vector3>();
          for (int i = 0; i < sub.Count; i++)
          {
            if (nextVert == null)
            {
              currentVert = new Vertex(sub[i].x, sub[i].y, sub[i].z);
              nextVert = new Vertex(sub[i + 1].x, sub[i].y, sub[i + 1].z);
            }
            else
            {
              currentVert = nextVert;
              if (i == sub.Count - 1)
              {
                nextVert = firstVert;
              }
              else
              {
                nextVert = new Vertex(sub[i + 1].x, sub[i + 1].y, sub[i + 1].z);
              }
            }

            if (i == 0)
              firstVert = currentVert;

            wist.Add(sub[i]);
            polygon.Add(currentVert);
            polygon.Add(new Segment(currentVert, nextVert));
          }
        }
        else
        {
          var cont = new List<Vertex>();
          var wist = new List<Vector3>();
          for (int i = 0; i < sub.Count; i++)
          {
            wist.Add(sub[i]);
            cont.Add(new Vertex(sub[i].x, sub[i].y, sub[i].z));
          }
          polygon.Add(new Contour(cont), true);
        }
      }
      */


      foreach (var sub in polygons)
      {
        var poly = new Polygon();
        var w = new List<Vector3>();
        for (int i = 0; i < sub.Count; i++)
        {
          //w.Add(sub[i]);
          poly.Add(new Vertex(sub[i].x, sub[i].y, sub[i].z));
        }
        var mesh = poly.Triangulate();

        //crashes
        //var mesh = poly.Triangulate(options, quality);


        foreach (var tri in mesh.Triangles)
        {
          data.Add(tri.GetVertexID(0));
          data.Add(tri.GetVertexID(2));
          data.Add(tri.GetVertexID(1));
        }

        foreach (var edge in mesh.Edges)
        {
          if (edge.Label == 0)
            continue;

          Edges.Add(edge.P0);
          Edges.Add(edge.P1);
        }


        using (var sequenceEnum = mesh.Vertices.GetEnumerator())
        {
          while (sequenceEnum.MoveNext())
          {
            //Vertices.Add(new Vector3((float)sequenceEnum.Current.x, (float)sequenceEnum.Current.z, (float)sequenceEnum.Current.y));
            Vertices.Add(new Vector3((float)sequenceEnum.Current.x, (float)0, (float)sequenceEnum.Current.y));
          }
        }

        Triangles.Add(data);
      }


      //smoother mesh with smaller triangles and extra vertices in the middle      

      //var mesh = (TriangleNet.Mesh)polygon.Triangulate(options, quality);
      m.SetVertices(Vertices);
      m.subMeshCount = Triangles.Count;
      for (int i = 0; i < Triangles.Count; i++)
      {
        var triangle = Triangles[i];
        m.SetTriangles(triangle, i);
      }

      //m.RecalculateNormals();
      return m;
    }
    
    /**
     * Assumes go is at the base of the mesh
     * */
    public static void MoveGameObjectToTerrain(DRect TileBoundsMeters, Vector3 TilePos, MeshCollider TerrainMesh, GameObject go, UnityEngine.Mesh SourceMesh)
    {
      Vector3 sv = go.transform.position;
      //float p = GetMeshHeight(TileBoundsMeters, TilePos, TerrainMesh, sv);

      List<Vector3> vertices = new List<Vector3>();
      SourceMesh.GetVertices(vertices);
      float lowest = 99999;
      for (int i = 0; i < vertices.Count; i++)
      {
        float p = GetMeshHeight(TileBoundsMeters, TilePos, TerrainMesh, vertices[i]);
        if (p < lowest) lowest = p;
      }

      go.transform.position = new Vector3(sv.x, lowest, sv.z);
    }

    /**
     * <summary>
     * Moves a planar mesh so that it touches a terrain
     * </summary>
     * */
    public static void MoveMeshToTerrain(DRect TileBoundsMeters, Vector3 TilePos, MeshCollider TerrainMesh, UnityEngine.Mesh SourceMesh, float offsetheight)
    {
      List<Vector3> vertices = new List<Vector3>();
      SourceMesh.GetVertices(vertices);

      float highest = -99999;
      for (int i = 0; i < vertices.Count; i++)
      {
        float p = GetMeshHeight(TileBoundsMeters, TilePos, TerrainMesh, vertices[i]);
        if (p > highest) highest = p;        
      }

      for (int i = 0; i < vertices.Count; i++)
      {
        vertices[i] = new Vector3(vertices[i].x,
          highest + offsetheight,
          vertices[i].z);
      }
      SourceMesh.SetVertices(vertices);
    }

    /**
     * <summary>
     * Tightly Fits all points in a planar mesh to a terrain
     * </summary>
     * */
    public static void FitMeshToTerrain(DRect TileBoundsMeters, Vector3 TilePos, MeshCollider TerrainMesh, UnityEngine.Mesh SourceMesh)
    {
      List<Vector3> vertices = new List<Vector3>();
      SourceMesh.GetVertices(vertices);
      for (int i = 0; i < vertices.Count; i++)
      {
        vertices[i] = new Vector3(vertices[i].x,
          GetMeshHeight(TileBoundsMeters, TilePos, TerrainMesh, vertices[i]),
          vertices[i].z);
      }      
      SourceMesh.SetVertices(vertices);      
    }

    public static void UpdatePivot(GameObject go, UnityEngine.Mesh mesh)
    {
      Vector3 p; //Pivot value -1..1, calculated from Mesh bounds
      Vector3 last_p; //Last used pivot      
      Bounds b = mesh.bounds;
      Vector3 offset = -1 * b.center;
      p = last_p = new Vector3(offset.x / b.extents.x, offset.y / (b.extents.y+0.001f), offset.z / b.extents.z);

      
      Vector3 diff = Vector3.Scale(mesh.bounds.extents, p); //Calculate difference in 3d position
      go.transform.position -= Vector3.Scale(diff, go.transform.localScale); //Move object position
                                                                               //Iterate over all vertices and move them in the opposite direction of the object position movement
      Vector3[] verts = mesh.vertices;
      for (int i = 0; i < verts.Length; i++)
      {
        verts[i] += diff;
      }
      mesh.vertices = verts; //Assign the vertex array back to the mesh
      mesh.RecalculateBounds(); //Recalculate bounds of the mesh, for the renderer's sake
                                //The 'center' parameter of certain colliders needs to be adjusted
                                //when the transform position is modified
    }

    /**
     * <summary>
     * Create UVS based on Tile Bitmap (shows roof)
     * </summary>
     * */
    public static void CreateUVS(DRect TileBoundsMeters, UnityEngine.Mesh fMesh)
    {
      var uv = new List<Vector2>();
      List<Vector3> Vertices = new List<Vector3>();
      fMesh.GetVertices(Vertices);

      foreach (var c in Vertices)
      {
        //creates roof uvs that matach satellite image
        var fromBottomLeft = new Vector2((float)((c.x) / TileBoundsMeters.Size.x),
            (float)((c.z) / TileBoundsMeters.Size.z));
        uv.Add(fromBottomLeft);
        //create simple uvs
        //  uv.Add(new Vector2(c.x, c.z));

      }
      //UV[0].AddRange(uv);
      fMesh.SetUVs(0, uv);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
  }

}