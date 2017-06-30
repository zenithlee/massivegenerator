using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  public class MassiveWater : MonoBehaviour
  {

    [Multiline(10)]
    public string sData;
    public Dictionary<string, object> Data;
    public Material OceanMaterial;
    public DRect TileBoundsMeters;
    MeshCollider mc;

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

    public void GenerateWaterJSON(string data, GameObject Container, DVector3 PositionMeters)
    {
      mc = transform.parent.Find("terrain").GetComponent<MeshCollider>();
      TileBoundsMeters = this.transform.parent.GetComponent<MassiveTile>().TileBoundsMeters;
      Container.layer = LayerMask.NameToLayer("Water");
      sData = data;
      Data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(sData);

      IList features = (IList)Data["features"];

      foreach (IDictionary feature in features)
      {
        IDictionary geometry = (IDictionary)feature["geometry"];
        string type = (string)geometry["type"];
        IDictionary properties = (IDictionary)feature["properties"];
        string kind = OSMTools.GetProperty(properties, "kind");

        List<List<Vector3>> segments = new List<List<Vector3>>();

        /*
        if (type == "LineString")
        {
          List<DVector3> list = new List<DVector3>();
          foreach (IList piece in (IList)geometry["coordinates"])
          {
            double dx = (double)piece[0];
            double dz = (double)piece[1];
            DVector3 v = MapFuncs.LatLonToMeters((float)dz, (float)dx);
            v -= PositionMeters;
            list.Add(v);
          }
          segments.Add(list);
        }
        */

        if (type == "MultiPolygon")
        {
          IList segs = (IList)geometry["coordinates"];          
          foreach (IList part in segs)
          {
            segments = OSMTools.CreateMultiPolygon(part, PositionMeters);
            GenerateGeometry(segments, Container, properties, type);
          }
        }
        if (type == "Polygon")
        {
          segments = OSMTools.CreatePolygon(geometry, PositionMeters.ToVector3());
          GenerateGeometry(segments, Container, properties, type);
        }

        //Debug.Log(segments);
       
      }
    }

    void GenerateGeometry(List<List<Vector3>> segments, GameObject container, IDictionary properties, string type)
    {
      List<Vector2> points = new List<Vector2>();

      foreach (List<Vector3> list in segments)
      {
        GameObject go = new GameObject("piece");
        go.transform.parent = container.transform;
        go.layer = LayerMask.NameToLayer("Water");
        go.name = properties["kind"].ToString();
        
        MassiveFeature mf = go.AddComponent<MassiveFeature>();
        mf.SetProperties(properties);
        mf.SetSegments(segments);
        mf.SetType(type);


        for (int i = 0; i < list.Count - 1; i++)
        {
          Vector3 v1 = list[i];
          points.Add(new Vector2((float)v1.x, (float)v1.z));
          //float heightv1 = GetMeshHeight(v1);
          //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
          //go.transform.parent = container.transform;
          //go.transform.position = v1.ToVector3();
          //go.transform.localScale = new Vector3(20, 20, 20);

        }

        Triangulator tr = new Triangulator(points.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[points.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
          vertices[i] = new Vector3(points[i].x, 0, points[i].y);
          float h = GetMeshHeight(vertices[i]);
          vertices[i].y = h;
        }

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        MeshRenderer r = go.AddComponent<MeshRenderer>();
        r.material = OceanMaterial;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        MeshFilter filter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        go.AddComponent<MeshCollider>();
      }
    }

    void Test(GameObject go)
    {
      // Create Vector2 vertices
      Vector2[] vertices2D = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,50),
            new Vector2(50,50),
            new Vector2(50,100),
            new Vector2(0,100),
            new Vector2(0,150),
            new Vector2(150,150),
            new Vector2(150,100),
            new Vector2(100,100),
            new Vector2(100,50),
            new Vector2(150,50),
            new Vector2(150,0),
        };

      // Use the triangulator to get indices for creating triangles
      Triangulator tr = new Triangulator(vertices2D);
      int[] indices = tr.Triangulate();

      // Create the Vector3 vertices
      Vector3[] vertices = new Vector3[vertices2D.Length];
      for (int i = 0; i < vertices.Length; i++)
      {
        vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
      }

      // Create the mesh
      Mesh msh = new Mesh();
      msh.vertices = vertices;
      msh.triangles = indices;
      msh.RecalculateNormals();
      msh.RecalculateBounds();

      // Set up game object with mesh;
      go.AddComponent(typeof(MeshRenderer));
      MeshFilter filter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
      filter.mesh = msh;
    }


    // Use this for initialization
    void Start()
    {
      Light l = GameObject.FindObjectOfType<Light>();
      Ocean o = GetComponent<Ocean>();
      o.m_sun = l.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }
  }

}