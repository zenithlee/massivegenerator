﻿using System.Collections;
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
    public static float GetMeshHeight(DRect TileBoundsMeters, Vector3 TilePos, MeshCollider mc, Vector3 v)
    {
      RaycastHit HitInfo;

      Vector3 pos = new Vector3((float)v.x, 9000.0f, (float)v.z);
      Vector3 cpos = new Vector3(
        Mathf.Clamp(pos.x, 0, (float)TileBoundsMeters.Size.x),
        pos.y,
        Mathf.Clamp(pos.z, 0, (float)TileBoundsMeters.Size.z));
      Ray ray = new Ray(cpos + TilePos, -Vector3.up);
      mc.Raycast(ray, out HitInfo, 10000);

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

    public static void FitMeshToTerrain(DRect TileBoundsMeters, Vector3 TilePos, MeshCollider TerrainMesh, UnityEngine.Mesh SourceMesh)
    {
      List<Vector3> vertices = new List<Vector3>();
      SourceMesh.GetVertices(vertices);
      for( int i=0; i< vertices.Count; i++)
      {
        vertices[i] = new Vector3(vertices[i].x,
          GetMeshHeight(TileBoundsMeters, TilePos, TerrainMesh, vertices[i]),
          vertices[i].z);
      }
      SourceMesh.SetVertices(vertices);
    }

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