using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace _Massive
{

  public class MassiveRoad : MonoBehaviour
  {
    public Dictionary<string, object> Data;
    List<Vector3> Nodes = new List<Vector3>();
    MeshCollider mc;

    DVector3[] Profile = { new DVector3(0, 0, 0), new DVector3(0, 1, 0) };
    public float RoadWidth = 10;
    public float YSnap = 3;
    public DRect TileBoundsMeters;
    public string[] AllowShapes = { "LineString", "MultiLineString" };

    Material MajorRoadMaterial;
    Material SurfaceMaterial;
    Material PathMaterial;
    Material RailwayMaterial;
    int InterpolateRoadMeters = 5; // lower values = more polygons

    //highway, major_road, minor_road, rail, path, ferry, piste, aerialway, aeroway, racetrack, portage_way
    public string[] AllowKinds = { "highway", "major_road", "minor_road", "path", "motorway", "rail" }; 

    bool CanWeInclude(string[] array, string s)
    {
      foreach (string c in array)
      {
        if (s.CompareTo(c) == 0) return true;
      }
      return false;
    }

    public void SetMaterial(Material Major, Material m, Material PathMat, Material RailMat)
    {
      MajorRoadMaterial = Major;
      SurfaceMaterial = m;
      PathMaterial = PathMat;
      RailwayMaterial = RailMat;
    }

    private float GetWidthForKind(string kind)
    {
      switch (kind)
      {
        case "highway":
          return 35;
        case "major_road":
          return 10;
        case "minor_road":
          return 6;
        case "service":
          return 3;
        case "path":
          return 2;
        case "rail":
          return 3;
      }
      return 1;
    }

    private Material GetMaterialForKind(string kind)
    {
      switch (kind)
      {
        case "highway":
          return SurfaceMaterial;
        case "major_road":
          return MajorRoadMaterial;
        case "minor_road":
          return SurfaceMaterial;
        case "service":
          return SurfaceMaterial;
        case "path":
          return PathMaterial;
        case "rail":
          return RailwayMaterial;
        default:
          return SurfaceMaterial;
      }
      return SurfaceMaterial;
    }

    public float GetMeshHeight(Vector3 v)
    {
      RaycastHit HitInfo;

      Vector3 pos = new Vector3((float)v.x, 9000.0f, (float)v.z);
      Vector3 cpos = new Vector3(
        Mathf.Clamp(pos.x, 0, (float)TileBoundsMeters.Size.x),
        pos.y,
        Mathf.Clamp(pos.z, 0, (float)TileBoundsMeters.Size.z));
      Ray ray = new Ray(cpos + this.transform.parent.position, -Vector3.up);
      mc.Raycast(ray, out HitInfo, 10000);

      //5 * (int)Math.Round(p / 5.0)
      //float snapped = YSnap * (int)Mathd.Round(HitInfo.point.y / YSnap);
      return HitInfo.point.y;
    }

    public void GenerateTest(string sData, DVector3 PositionMeters)
    {
      List<List<Vector3>> segments = new List<List<Vector3>>();

      List<Vector3> list = new List<Vector3>();

      list.Add(new Vector3(0, 0, 0));
      list.Add(new Vector3(100, 0, 0));
      list.Add(new Vector3(200, 0, 0));

      segments.Add(list);

      List<Vector3> list2 = new List<Vector3>();

      list2.Add(new Vector3(65, 0, 0));
      list2.Add(new Vector3(55, 0, 100));
      list2.Add(new Vector3(45, 0, 200));

      segments.Add(list2);

      GameObject go = new GameObject("test");
      MassiveFeature f = go.AddComponent<MassiveFeature>();
      f.SetSegments(segments);
      go.transform.parent = this.transform;
      go.transform.localPosition = Vector3.zero;
      MeshData md = new MeshData();
      Run(segments, md, 20);

      // Create the mesh
      CreateGameObject(md, go, "minor_road");

    }
    public void Generate(string sData, DVector3 PositionMeters, string AssetPath)
    {
      mc = transform.parent.Find("terrain").GetComponent<MeshCollider>();
      TileBoundsMeters = this.transform.parent.GetComponent<MassiveTile>().TileBoundsMeters;
      Data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(sData);

      IList features = (IList)Data["features"];
      foreach (IDictionary feature in features)
      {
        IDictionary geometry = (IDictionary)feature["geometry"];
        string geotype = (string)geometry["type"];

        IDictionary properties = (IDictionary)feature["properties"];
        string kind = "";
        string kinddetail = "";
        string name = "_";
        string id = "";
        name = OSMTools.GetProperty(properties, "name");
        kind = OSMTools.GetProperty(properties, "kind");
        kinddetail = OSMTools.GetProperty(properties, "kind_detail");
        id = OSMTools.GetProperty(properties, "id");        

        //if (name == "_") continue;

        if (!CanWeInclude(AllowKinds, kind))
        {
          continue;
        }


        List<List<Vector3>> segments = new List<List<Vector3>>();

        if (geotype == "LineString"
          && CanWeInclude(AllowShapes, "LineString"))
        {
          List<Vector3> list = new List<Vector3>();
          foreach (IList piece in (IList)geometry["coordinates"])
          {
            double dx = (double)piece[0];
            double dz = (double)piece[1];
            Vector3 v = MapTools.LatLonToMeters((float)dz, (float)dx).ToVector3();
            v -= PositionMeters.ToVector3();
            list.Add(v);
          }

          list = OSMTools.InsertPoints(list, InterpolateRoadMeters);

          segments.Add(list);
        }

        if (geotype == "MultiLineString"
          && CanWeInclude(AllowShapes, "MultiLineString"))
        {

          foreach (IList piecelist in (IList)geometry["coordinates"])
          {
            List<Vector3> list = new List<Vector3>();
            foreach (IList piece in (IList)piecelist)
            {
              double dx = (double)piece[0];
              double dz = (double)piece[1];
              Vector3 v = MapTools.LatLonToMeters((float)dz, (float)dx).ToVector3();
              v -= PositionMeters.ToVector3();
              list.Add(v);
            }
            list = OSMTools.InsertPoints(list, InterpolateRoadMeters);
            segments.Add(list);
          }

        } 

        if (segments.Count > 0)
        {
          MeshData md = new MeshData();
          Run(segments, md, GetWidthForKind(kind));

          GameObject go = new GameObject(name);
          go.layer = LayerMask.NameToLayer("Roads");
          MassiveFeature f = go.AddComponent<MassiveFeature>();
          f.SetSegments(segments);
          f.SetProperties(properties);
          f.SetType(geotype);
          f.SetID(id);
          go.transform.parent = this.transform;
          go.transform.localPosition = Vector3.zero;
          // Create the mesh
          CreateGameObject(md, go, kind);
          //WriteAsset(go, AssetPath);
        }
      }

#if UNITY_EDITOR
      AssetDatabase.SaveAssets();
#endif
      
    }

    void WriteAsset(GameObject go, string path)
    {

      if (!File.Exists(path))
      {
        //Directory.CreateDirectory(path);
      }
      MeshFilter f = go.transform.Find("segment").GetComponent<MeshFilter>();
      f.sharedMesh.name = go.name;
#if UNITY_EDITOR
      if (File.Exists(path))
      {
        AssetDatabase.AddObjectToAsset(f.sharedMesh, path);
      }
      else
      {
        AssetDatabase.CreateAsset(f.sharedMesh, path);
      }
#endif
    }

    void GenerateGeometry(List<List<Vector3>> segments, GameObject container)
    {
      List<Vector2> points = new List<Vector2>();

      Vector3 cross = new Vector3(0, 0, 0);
      foreach (List<Vector3> list in segments)
      {
        GameObject go = new GameObject("segment");
        go.transform.parent = container.transform;
        for (int i = 0; i < list.Count; i++)
        {
          Vector3 v1 = list[i];
          if (i < list.Count - 1)
          {
            Vector3 v2 = list[i + 1];
            cross = Vector3.Cross((v2 - v1).normalized, Vector3.up);
          }
          points.Add(new Vector2((float)v1.x, (float)v1.z) + new Vector2(cross.x, cross.z) * RoadWidth);
          //points.Add(new Vector2((float)v1.x, (float)v1.z));        
        }


        cross = new Vector3(0, 0, 0);
        for (int i = list.Count - 1; i >= 0; i--)
        {
          Vector3 v1 = list[i];
          if (i > 0)
          {
            Vector3 v2 = list[i - 1];
            cross = Vector3.Cross((v2 - v1).normalized, Vector3.up);
          }
          points.Add(new Vector2((float)v1.x, (float)v1.z) + new Vector2(cross.x, cross.z) * RoadWidth);
        }


        Triangulator tr = new Triangulator(points.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[points.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
          Vector3 v1 = new Vector3(points[i].x, 0, points[i].y);
          vertices[i] = (v1 + new Vector3(0, GetMeshHeight(v1), 0)) + container.transform.position;
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        MeshRenderer r = go.AddComponent<MeshRenderer>();
        MeshFilter filter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
      }
    }


    public void Run(List<List<Vector3>> segments, MeshData md, float Width)
    {
      if (segments.Count < 1)
        return;

      foreach (var roadSegment in segments)
      {
        var count = roadSegment.Count;
        for (int i = 1; i < count * 2; i++)
        {
          md.Edges.Add(md.Vertices.Count + i);
          md.Edges.Add(md.Vertices.Count + i - 1);
        }
        md.Edges.Add(md.Vertices.Count);
        md.Edges.Add(md.Vertices.Count + (count * 2) - 1);

        var newVerticeList = new Vector3[count * 2];
        var uvList = new Vector2[count * 2];
        Vector3 norm;
        var lastUv = 0f;
        var p1 = Vector3.zero;
        var p2 = Vector3.zero;
        var p3 = Vector3.zero;
        for (int i = 1; i < count; i++)
        {
          p1 = roadSegment[i - 1];
          p2 = roadSegment[i];
          p3 = p2;
          if (i + 1 < roadSegment.Count)
            p3 = roadSegment[i + 1];

          if (i == 1)
          {
            norm = GetNormal(p1, p1, p2) * Width; //road width
            newVerticeList[0] = (p1 + norm);
            newVerticeList[count * 2 - 1] = (p1 - norm);
            uvList[0] = new Vector2(0, 0);
            uvList[count * 2 - 1] = new Vector2(1, 0);
          }
          var dist = Vector3.Distance(p1, p2);
          lastUv += dist;
          norm = GetNormal(p1, p2, p3) * Width;
          newVerticeList[i] = (p2 + norm);
          newVerticeList[2 * count - 1 - i] = (p2 - norm);

          uvList[i] = new Vector2(0, lastUv);
          uvList[2 * count - 1 - i] = new Vector2(1, lastUv);
        }

        //GenerateTerrainHeight
        for (int i = 0; i < newVerticeList.Length; i++)
        {
          newVerticeList[i] += new Vector3(0, GetMeshHeight(newVerticeList[i])+0.1f, 0); 
        }

        var pcount = md.Vertices.Count;
        md.Vertices.AddRange(newVerticeList);
        md.UV[0].AddRange(uvList);
        var lineTri = new List<int>();
        var n = count;

        for (int i = 0; i < n - 1; i++)
        {
          lineTri.Add(pcount + i);
          lineTri.Add(pcount + i + 1);
          lineTri.Add(pcount + 2 * n - 1 - i);

          lineTri.Add(pcount + i + 1);
          lineTri.Add(pcount + 2 * n - i - 2);
          lineTri.Add(pcount + 2 * n - i - 1);
        }

        if (md.Triangles.Count < 1)
        {
          md.Triangles.Add(new List<int>());
        }
        md.Triangles[0].AddRange(lineTri);
      }
    }

    private Vector3 GetNormal(Vector3 p1, Vector3 newPos, Vector3 p2)
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

    private GameObject CreateGameObject(MeshData data, GameObject main, string kind)
    {
      var go = new GameObject("segment");
      go.layer = LayerMask.NameToLayer("Roads");
      MeshRenderer r = go.AddComponent<MeshRenderer>();
      r.material = GetMaterialForKind(kind);
      r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

      MeshFilter mf = go.AddComponent<MeshFilter>();
      mf.sharedMesh = new Mesh();
      mf.sharedMesh.name = main.name;
      var mesh = mf.sharedMesh;
      mesh.subMeshCount = data.Triangles.Count;

      mesh.SetVertices(data.Vertices);
      for (int i = 0; i < data.Triangles.Count; i++)
      {
        var triangle = data.Triangles[i];
        mesh.SetTriangles(triangle, i);
      }

      for (int i = 0; i < data.UV.Count; i++)
      {
        var uv = data.UV[i];
        mesh.SetUVs(i, uv);
      }

      mesh.RecalculateNormals();
      go.transform.SetParent(main.transform, false);

      go.AddComponent<MeshCollider>();

      return go;
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateRoadCubes(List<List<Vector3>> segments, GameObject container, float width, DVector3 PositionMeters)
    {

      DRect OffsetRect = TileBoundsMeters - PositionMeters;

      foreach (List<Vector3> list in segments)
      {

        for (int i = 0; i < list.Count - 1; i++)
        {
          Vector3 v1 = list[i];

          // Vector3 propv1 = MapFuncs.ProportionInBounds(v1, OffsetRect).ToVector3();
          //float heightv1 = QueryHeightData(propv1.x, propv1.z);
          float heightv1 = GetMeshHeight(v1);

          Vector3 v2 = list[i + 1];
          //Vector3 propv2 = MapFuncs.ProportionInBounds(v2, OffsetRect).ToVector3();
          float heightv2 = GetMeshHeight(v2);

          GameObject cube = new GameObject();
          cube.transform.parent = container.transform;
          cube.name = "roadsegment";
          GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
          if (width <= 20)
          {
            g.GetComponent<MeshRenderer>().material = SurfaceMaterial;
          }
          else
          {
            g.GetComponent<MeshRenderer>().material = SurfaceMaterial;
          }

          g.transform.parent = cube.transform;
          g.transform.position = new Vector3(0.5f, 0, 0.5f);

          Vector3 dif = v2 - v1;
          float distance = dif.magnitude;
          cube.transform.localScale = new Vector3(width, 7, distance);

          cube.transform.position = v1 + new Vector3(0, heightv1, 0) + this.transform.position;
          cube.transform.LookAt(v2 + new Vector3(0, heightv2, 0) + this.transform.position);
        }

      }

    }
  }



  // TODO: Do we need this class? Why not just use `Mesh`?
  public class MeshData
  {
    public List<int> Edges { get; set; }
    public Vector2 MercatorCenter { get; set; }
    public List<Vector3> Vertices { get; set; }
    public List<Vector3> Normals { get; set; }
    public List<List<int>> Triangles { get; set; }
    public List<List<Vector2>> UV { get; set; }

    public MeshData()
    {
      Edges = new List<int>();
      Vertices = new List<Vector3>();
      Normals = new List<Vector3>();
      Triangles = new List<List<int>>();
      UV = new List<List<Vector2>>();
      UV.Add(new List<Vector2>());
    }
  }



}


