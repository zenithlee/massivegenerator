using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  public class Road
  {
    public const string KIND_HIGHWAY = "highway";
    public const string KIND_MINOR_ROAD = "minor_road";
    public const string KIND_MAJOR_ROAD = "major_road";
    public const string KIND_RUNWAY = "runway";
    public const string KIND_RAIL = "rail";
    public const string KIND_PATH = "path";

    public const string KIND_SERVICE = "service";
    public const string KIND_CYCLEWAY = "cycleway";
    public const string KIND_PEDESTRIAN = "pedestrian";
    public GameObject go;
    string id;
    public string kind; //road, rail, path
    public bool isPath = false;
    public string kind_detail; //service
    public bool is_bridge = false;
    public int MetersPerTile = 10;
    public float width = 10;
    public float vscale = 1f;
    public float meshoffset = 0.05f;  //to put mesh on top of terrain mesh
    public float serviceroadoffset = -0.025f; //small offset to put service roads under regular roads
    public string name;
    public List<Node> nodes;

    List<Vector3> v = new List<Vector3>();
    List<int> t = new List<int>();
    List<Vector2> u = new List<Vector2>();
    public Mesh m = new Mesh();

    public Road(string iname, GameObject parent, string ikind, string ikinddetail = "")
    {
      name = iname;
      nodes = new List<Node>();
      go = new GameObject(name);
      go.layer = LayerMask.NameToLayer("Roads");
      go.transform.parent = parent.transform;
      go.transform.localPosition = Vector3.zero;
      kind = ikind;
      kind_detail = ikinddetail;
      width = GetWidthForKind(kind);
    }

    public void ds(Vector3 v, string s = "-")
    {
      GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.transform.position = v;
      go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
      go.name = s;
      go.transform.parent = this.go.transform;
      GameObject th = new GameObject();
      th.transform.parent = go.transform;
      th.transform.localPosition = new Vector3(0, 1.5f, 0);
      TextMesh tm = th.AddComponent<TextMesh>();
      tm.text = s;
    }
    /**
     * <summary>
     * v1s = start of segment 1
     * v1e = end of segment 1
     * v2s = start of segment 2
     * v2e = end of segment 2
     * mv1e = pulled back position on v1-v2
     * mvs2 = pulled back position on v2-v3
     * n1 = planar normal of v1
     * n2 = planar normal of v2
     * </summary>
     * */
    public void CreateSmoothConnection(Vector3 v1s, Vector3 v1e, Vector3 v2s, Vector3 v2e,
      Vector3 mv1e, Vector3 mv2s,
      Vector3 n1, Vector3 n2)
    {
      float numdivs = 5;
      float step = 1.0f / numdivs;

      //ds(v1s, "v1s");
      //ds(v1e, "v1e");

      //ds(v2s, "v2s");
      //ds(v2e, "v2e");

      // ds(mv1e, "mv1e");
      // ds(mv2s, "mv2s");

      //ds(mv1e + n1, "mv1e+n1");
      //ds(mv2s + n2, "mv2s+n2");

      Vector3 i1;
      Vector3 dir1 = (v1e - v1s);
      Vector3 dir2 = (v2e - v2s);

      //we use the 2d version of the vectors      
      bool b = MathTools.LineLineIntersection(out i1, v1s + n1, dir1, v2s + n2, dir2);
      if (b == false)
      {
        CreateConnection(mv1e, mv2s, n1, n2);
        return;
      }
      //Debug.Log(b);
      //Debug.Log(i1);

      //ds(i1, "i1");

      Vector3 i2;
      dir1 = (v1e - v1s);
      dir2 = (v2e - v2s);
      b = MathTools.LineLineIntersection(out i2, v1s - n1, dir1, v2s - n2, dir2);

      //inner curve
      Bezier bez1 = new Bezier(mv1e + n1, i1, i1, mv2s + n2);
      //outer curve
      Bezier bez2 = new Bezier(mv1e - n1, i2, i2, mv2s - n2);

      for (float f = 0; f < 1; f += step)
      {


        Vector3 pt1 = bez1.GetPointAtTime(f);
        //ds(pt1, "bezpt1");
        Vector3 pt2 = bez2.GetPointAtTime(f);
        //ds(pt2, "bezpt2");

        Vector3 pt2e = bez2.GetPointAtTime(f + step);
        //ds(pt2e, "bezpt2e");

        Vector3 pt1e = bez1.GetPointAtTime(f + step);
        // ds(pt1e, "bezpt1e");

        //tri1
        v.Add(pt1);
        t.Add(v.Count - 1);
        u.Add(new Vector2(1, f + step));

        v.Add(pt2e);
        t.Add(v.Count - 1);
        u.Add(new Vector2(0, f));

        v.Add(pt2);
        t.Add(v.Count - 1);
        u.Add(new Vector2(0, f + step));

        //tri 2
        v.Add(pt1e);
        t.Add(v.Count - 1);
        u.Add(new Vector2(0, f));

        v.Add(pt2e);
        t.Add(v.Count - 1);
        u.Add(new Vector2(1, f));

        v.Add(pt1);
        t.Add(v.Count - 1);
        u.Add(new Vector2(0, f + step));

      }

      /*

      b = MathTools.LineLineIntersection(out i1, v1s - n1, dir1, v2s - n2, dir2);
      Debug.Log(i1);

      go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.transform.position = i1;
      go.transform.parent = this.go.transform;
      */
    }

    // creates a straight or slightly angled connection between two end points
    // for joining two road segments
    //for more severe angles we use an elbow join
    // for sidewalks, only calc the one to the right (calc clockwise neighbour)
    public void CreateConnection(Vector3 v1, Vector3 v2, Vector3 n1, Vector3 n2)
    {
      //ds(v1, "v1");
      //ds(v2, "v2");
      //ds(v1+n1, "v1+n1");
      //ds(v1-n1, "v1-n1");


      v.Add(v2 + n2);
      t.Add(v.Count - 1);
      u.Add(new Vector2(0, 0));

      v.Add(v2 - n2);
      t.Add(v.Count - 1);
      u.Add(new Vector2(1, 0));

      v.Add(v1 - n1);
      t.Add(v.Count - 1);
      u.Add(new Vector2(1, Vector3.Distance(v2 - n2, v1 - n1) / MetersPerTile));


      v.Add(v2 + n2);
      u.Add(new Vector2(0, 0));
      t.Add(v.Count - 1);

      v.Add(v1 - n1);
      t.Add(v.Count - 1);
      u.Add(new Vector2(1, 0));

      v.Add(v1 + n1);
      t.Add(v.Count - 1);
      u.Add(new Vector2(0, Vector3.Distance(v2 + n2, v1 + n1) / MetersPerTile));

    }

    //n2 is used for the end angle
    // v1,v2 = point2 pulled away from intersection
    public void CreateHQStraight(Vector3 v1, Vector3 v2, Vector3 n1, MeshCollider Terrain)
    {
      float dist = Vector3.Distance(v1, v2);
      int i;


      float steps = dist / MetersPerTile;
      int isteps = Mathf.FloorToInt(steps);

      for (i = 0; i <= isteps; i++)
      {

        Vector3 vv1 = MeshTools.LerpByDistance(v1, v2, i * MetersPerTile);

        Vector3 vv2;

        //if we are on the last step (always equal or smaller than MetersPerTile) then use v2 rather than lerp
        if (i > isteps - 1)
        {
          vv2 = v2;
          vscale = steps - isteps;
        }
        else //otherwise create a segment MetersPerTile long
        {
          vv2 = MeshTools.LerpByDistance(v1, v2, i * MetersPerTile + MetersPerTile);
          vscale = 1;
        }

        vv1 += new Vector3(0, MeshTools.GetMeshHeightSimple(Terrain, vv1) + 1, 0);
        vv2 += new Vector3(0, MeshTools.GetMeshHeightSimple(Terrain, vv2) + 1, 0);

        v.Add(vv1 - n1);
        t.Add(v.Count - 1);
        v.Add(vv1 + n1);
        t.Add(v.Count - 1);
        v.Add(vv2 - n1);
        t.Add(v.Count - 1);

        u.Add(new Vector2(0, 0));
        u.Add(new Vector2(1, 0));
        u.Add(new Vector2(0, vscale));

        v.Add(vv1 + n1);
        t.Add(v.Count - 1);
        v.Add(vv2 + n1);
        t.Add(v.Count - 1);
        v.Add(vv2 - n1);
        t.Add(v.Count - 1);
        u.Add(new Vector2(1, 0));
        u.Add(new Vector2(1, vscale));
        u.Add(new Vector2(0, vscale));

        if (kind != KIND_PATH)
        {
          //add sidewalk
          v.Add(vv1 - n1 * 2 + new Vector3(0, -3, 0));
          t.Add(v.Count - 1);
          v.Add(vv1 - n1);
          t.Add(v.Count - 1);
          v.Add(vv2 - n1);
          t.Add(v.Count - 1);

          u.Add(new Vector2(0, 0));
          u.Add(new Vector2(1, 0));
          u.Add(new Vector2(0, vscale));

          v.Add(vv1 - n1 * 2 + new Vector3(0, -3, 0));
          t.Add(v.Count - 1);
          v.Add(vv2 - n1);
          t.Add(v.Count - 1);
          v.Add(vv2 - n1 * 2 + new Vector3(0, -3, 0));
          t.Add(v.Count - 1);

          u.Add(new Vector2(0, 0));
          u.Add(new Vector2(1, 0));
          u.Add(new Vector2(0, vscale));

          //add sidewalk 2
          v.Add(vv2 + n1);
          t.Add(v.Count - 1);
          v.Add(vv1 + n1);
          t.Add(v.Count - 1);
          v.Add(vv1 + n1 * 2 + new Vector3(0, -3, 0));
          t.Add(v.Count - 1);
          

          u.Add(new Vector2(0, 0));
          u.Add(new Vector2(1, 0));
          u.Add(new Vector2(0, vscale));

          v.Add(vv2 + n1 * 2 + new Vector3(0, -3, 0));
          t.Add(v.Count - 1);
          v.Add(vv2 + n1);
          t.Add(v.Count - 1);
          v.Add(vv1 + n1 * 2 + new Vector3(0, -3, 0));
          t.Add(v.Count - 1);
          

          u.Add(new Vector2(0, 0));
          u.Add(new Vector2(1, 0));
          u.Add(new Vector2(0, vscale));
        }

      }

      //Debug.Log("d:" + (dist - i));


    }

    //Creates a low quality straight section, suitable for flight sims with medium high altitude
    public void CreateStraight(Vector3 v1, Vector3 v2, Vector3 n1)
    {

      v.Add(v1 - n1);
      t.Add(v.Count - 1);
      v.Add(v1 + n1);
      t.Add(v.Count - 1);
      v.Add(v2 - n1);
      t.Add(v.Count - 1);

      u.Add(new Vector2(0, 0));
      u.Add(new Vector2(1, 0));
      u.Add(new Vector2(0, vscale));

      v.Add(v1 + n1);
      t.Add(v.Count - 1);
      v.Add(v2 + n1);
      t.Add(v.Count - 1);
      v.Add(v2 - n1);
      t.Add(v.Count - 1);
      u.Add(new Vector2(1, 0));
      u.Add(new Vector2(1, vscale));
      u.Add(new Vector2(0, vscale));

    }

    public void CreateMesh()
    {
      m.SetVertices(v);
      m.SetTriangles(t, 0, true);
      m.SetUVs(0, u);
      m.RecalculateNormals();
      /* foreach( Vector3 ver in v)
       {
         Debug.Log(ver);
       }*/
    }

    public Vector3 GetOffset()
    {
      if (kind_detail == KIND_SERVICE) return new Vector3(0, serviceroadoffset, 0);
      else return Vector3.zero;
    }

    private float GetWidthForKind(string kind)
    {
      switch (kind)
      {
        case KIND_HIGHWAY:
          return 10;
        case KIND_MAJOR_ROAD:
          return 7;
        case KIND_MINOR_ROAD:
          return 5;
        case KIND_SERVICE:
          return 3;
        case KIND_PATH:
          return 2;
        case KIND_RAIL:
          return 3;
      }
      return 1;
    }

  }
}