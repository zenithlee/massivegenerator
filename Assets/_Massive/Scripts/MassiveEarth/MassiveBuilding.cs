using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet;
using System;

namespace _Massive
{

  public class MassiveBuilding : MonoBehaviour
  {

    Material BuildingMaterial;
    public Dictionary<string, object> Data;
    MeshCollider mc;
    public DRect TileBoundsMeters;
    public DVector3 PositionMeters;

    public void Generate(string sData, GameObject Container, DVector3 iPositionMeters, string AssetPath)
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
          IList segs =(IList) geometry["coordinates"];
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
          List<List<Vector3>> segments = new List<List<Vector3>>();
          segments = OSMTools.CreatePolygon(geometry, PositionMeters.ToVector3());
          GenerateGeometry(segments, Container, properties, type);
        }

        //Debug.Log(segments);

      }
    }

    void GenerateGeometry(List<List<Vector3>> segments, GameObject container, IDictionary properties, string type)
    {

      string pname = OSMTools.GetProperty(properties, "name");
      // Debug.Log( "Processing:" + pname);
      //List<Vector2> points = new List<Vector2>();

      UnityEngine.Mesh msh = new UnityEngine.Mesh();
      msh = MeshTools.Triangulate(segments);

      MeshTools.FitMeshToTerrain(TileBoundsMeters, this.transform.position, mc, msh);
      MeshTools.CreateUVS(TileBoundsMeters, msh);
      msh.RecalculateNormals();
      msh.RecalculateTangents();

      GameObject go = new GameObject("building");
      go.transform.parent = container.transform;
      go.layer = LayerMask.NameToLayer("Buildings");
      go.name = pname;
      go.transform.localPosition = Vector3.zero; 
      //go.name = properties["kind"].ToString();
      MassiveFeature f = go.AddComponent<MassiveFeature>();
      f.SetProperties(properties);
      f.SetSegments(segments);
      f.SetType(type);

      // Set up game object with mesh;
      MeshRenderer r = go.AddComponent<MeshRenderer>();
      r.material = BuildingMaterial;
      r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
      MeshFilter filter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
      filter.mesh = msh;
      go.AddComponent<MeshCollider>();

    }

    public void SetMaterial(Material m)
    {
      BuildingMaterial = m;
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