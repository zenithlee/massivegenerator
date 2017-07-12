using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;
using System;

namespace _Massive
{

  public class MassiveBuilding : MonoBehaviour
  {
    public Material BuildingMaterial;
    public Material RoofMaterial;
    public Dictionary<string, object> Data;
    MeshCollider mc;
    public DRect TileBoundsMeters;
    public DVector3 PositionMeters;
    GameObject Walls;

    float height = 1;

    public void Generate(string sData, GameObject Container, DVector3 iPositionMeters)
    {
      mc = transform.parent.Find("terrain").GetComponent<MeshCollider>();
      Container.layer = LayerMask.NameToLayer("Buildings");
      TileBoundsMeters = this.transform.parent.GetComponent<MassiveTile>().TileBoundsMeters;
      PositionMeters = iPositionMeters;

      Data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(sData);
      IList features = (IList)Data["features"];
      foreach (IDictionary feature in features)
      {
        IDictionary geometry = (IDictionary)feature["geometry"];
        string type = (string)geometry["type"];
        IDictionary properties = (IDictionary)feature["properties"];
        //string kind = OSMTools.GetProperty(properties, "kind");

        if (type == "MultiPolygon")
        {
          IList segs = (IList)geometry["coordinates"];
          foreach (IList segment in segs)
          {
            //segments = OSMTools.CreateMultiPolygon(geometry, PositionMeters);
            List<List<Vector3>> segments = new List<List<Vector3>>();
            segments = OSMTools.CreateMultiPolygon(segment, PositionMeters);
            GenerateGeometry(segments, Container, properties, type);
          }
        }
        if (type == "Polygon")
        {
          IList segs = (IList)geometry["coordinates"];
          foreach (IList segment in segs)
          {
            List<List<Vector3>> segments = new List<List<Vector3>>();
            segments = OSMTools.CreatePolygon(segment, PositionMeters.ToVector3());
            GenerateGeometry(segments, Container, properties, type);
          }
        }

        //Debug.Log(segments);

      }
    }


    void CreateWall(UnityEngine.Mesh m, List<Vector3> wall, float height, GameObject parent)
    {
      if (MeshTools.IsClockwise(wall))
      {
        wall.Reverse();
      }

      List<Vector3> v = new List<Vector3>();
      m.GetVertices(v);
      List<int> t = new List<int>();
      m.GetTriangles(t, 0);

      List<Vector2> uv = new List<Vector2>();
      m.GetUVs(0, uv);

      Vector3 h = new Vector3(0, height, 0);

      for (int i = 0; i < wall.Count - 1; i++)
      {
        Vector3 v1b = wall[i];
        Vector3 v2b = wall[i + 1];

        Vector3 v1t = v1b + h;
        Vector3 v2t = v2b + h;

        v.Add(v1b);
        t.Add(v.Count - 1);
        uv.Add(new Vector2(0, 1 * height));

        v.Add(v1t);
        t.Add(v.Count - 1);
        uv.Add(new Vector2(0, 0));

        v.Add(v2b);
        t.Add(v.Count - 1);
        uv.Add(new Vector2(1, 1 * height));

        v.Add(v1t);
        t.Add(v.Count - 1);
        uv.Add(new Vector2(0, 0));


        v.Add(v2t);
        t.Add(v.Count - 1);
        uv.Add(new Vector2(1, 0));

        v.Add(v2b);
        t.Add(v.Count - 1);
        uv.Add(new Vector2(1, 1 * height));

      }
      m.SetVertices(v);
      m.SetTriangles(t, 0);
      m.SetUVs(0, uv);
    }

    void CreateWalls(List<List<Vector3>> segments, GameObject parent)
    {
      Walls = new GameObject("walls");
      Walls.transform.parent = parent.transform;
      //Walls.transform.position = segments[0][0];
      Walls.transform.localPosition = Vector3.zero;
      Walls.layer = LayerMask.NameToLayer("Buildings");

      UnityEngine.Mesh m = new UnityEngine.Mesh();
      foreach (List<Vector3> wall in segments)
      {
        CreateWall(m, wall, height, parent);
      }
      m.RecalculateBounds();
      m.RecalculateNormals();
      m.RecalculateTangents();
      MeshFilter mf = Walls.AddComponent<MeshFilter>();
      mf.mesh = m;
      MeshRenderer r = Walls.AddComponent<MeshRenderer>();
      r.material = BuildingMaterial;
    }

    void CreateRoof(List<List<Vector3>> segments, GameObject parent)
    {
      GameObject Roof = new GameObject("roof");
      Roof.transform.parent = parent.transform;
      Roof.transform.localPosition = Vector3.zero;
      Roof.layer = LayerMask.NameToLayer("Buildings");

      UnityEngine.Mesh msh = new UnityEngine.Mesh();
      msh = MeshTools.Triangulate(segments);

      List<Vector3> v = new List<Vector3>();
      msh.GetVertices(v);
      for (int i = 0; i < v.Count; i++)
      {
        if (i == 0)
        {
          Roof.transform.position = v[i];
        }
        v[i] = new Vector3(v[i].x, height, v[i].z) - Roof.transform.position + this.transform.position;
      }
      msh.SetVertices(v);

      MeshTools.CreateUVS(TileBoundsMeters, msh);
      msh.RecalculateNormals();
      msh.RecalculateTangents();
      msh.RecalculateBounds();

      // Set up game object with mesh;
      MeshRenderer r = Roof.AddComponent<MeshRenderer>();
      r.material = RoofMaterial;
      r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
      MeshFilter filter = Roof.AddComponent(typeof(MeshFilter)) as MeshFilter;
      filter.mesh = msh;

      //Roof.AddComponent<MeshCollider>(); //required for mesh fix
    }

    void GenerateGeometry(List<List<Vector3>> segments, GameObject container, IDictionary properties, string type)
    {

      string pname = OSMTools.GetProperty(properties, "name");
      // Debug.Log( "Processing:" + pname);
      //List<Vector2> points = new List<Vector2>();

      GameObject go = new GameObject("building");
      go.transform.parent = container.transform;
      go.layer = LayerMask.NameToLayer("Buildings");
      go.name = pname;
      //go.transform.position = segments[0][0];
      go.transform.localPosition = Vector3.zero;
      //go.name = properties["kind"].ToString();
      MassiveFeature f = go.AddComponent<MassiveFeature>();
      f.SetProperties(properties);
      f.SetSegments(segments);
      f.SetType(type);
      //if height it not present assume 1 floor
      height = f.height == 0 ? 5 : f.height;

      CreateRoof(segments, go);
      CreateWalls(segments, go);
      MeshTools.MoveGameObjectToTerrain(TileBoundsMeters, this.transform.position, mc, go, Walls.GetComponent<MeshFilter>().sharedMesh);

      //create roof

      //MeshTools.FitMeshToTerrain(TileBoundsMeters, this.transform.position, mc, msh);
      //MeshTools.MoveMeshToTerrain(TileBoundsMeters, this.transform.position, mc, msh, f.height, go);


    }

    public void SetMaterial(Material matBuild, Material matRoof)
    {
      BuildingMaterial = matBuild;
      RoofMaterial = matRoof;
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