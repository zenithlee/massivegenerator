using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GSD.Roads;

namespace _Massive
{


  public class MassiveRoadNetwork : MonoBehaviour
  {
    List<Road> Roads = new List<Road>();
    Material MajorRoadMaterial;
    Material MinorRoadMaterial;
    Material RailMaterial;
    Material SurfaceMaterial;
    Material PathMaterial;
    int IntersectionThresholdMeters = 20;

    public DRect TileBoundsMeters;
    public MeshCollider mc;
    bool RoadHighQuality = true;

    public void CreateTest()
    {
      /*
      Road r1 = new Road("minor", this.gameObject, Road.KIND_MINOR_ROAD);
      Roads.Add(r1);
      Vector3 v1 = new Vector3(100, 0, 0);
      Vector3 v2 = new Vector3(100, 0, 95);
      Vector3 v3 = new Vector3(0, 0, 140);
      r1.nodes.Add(new Node(v1, r1.name));
      r1.nodes.Add(new Node(v2, r1.name));
      r1.nodes.Add(new Node(v3, r1.name));
      */

      Road rs = new Road("straight", this.gameObject, Road.KIND_MINOR_ROAD);
      Roads.Add(rs);
      Vector3 v1 = new Vector3(0, 0, 0);
      Vector3 v2 = new Vector3(150, 0, 95);
      Vector3 v3 = new Vector3(250, 0, 140);
      rs.nodes.Add(new Node(v1, rs.name));
      rs.nodes.Add(new Node(v2, rs.name));
      rs.nodes.Add(new Node(v3, rs.name));

     /* Road rss = new Road("bstraight", this.gameObject, Road.KIND_MINOR_ROAD, Road.KIND_SERVICE);
      Roads.Add(rss);
      Vector3 v1s = new Vector3(180, 0, 0);
      Vector3 v2s = new Vector3(180, 0, 95);
      Vector3 v3s = new Vector3(190, 0, 140);
      rss.nodes.Add(new Node(v1s, rss.name));
      rss.nodes.Add(new Node(v2s, rss.name));
      rss.nodes.Add(new Node(v3s, rss.name));
      */

      //r1.CreateSmoothConnection(v1, v2, v3, v2, Vector3.zero, Vector3.zero);

      /*
      Bezier b = new Bezier(new Vector3(0, 0, 0), 
        new Vector3( 50, 0, 14 ), 
        new Vector3(50, 0, 14), 
        new Vector3(100, 0, 100));

      for ( float f = 0; f< 1; f+= 0.1f)
      {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = b.GetPointAtTime(f);
        go.transform.parent = this.transform;
      }
      */



      /*
      Road r1maj = new Road("major", this.gameObject, Road.KIND_MAJOR_ROAD);
      Roads.Add(r1maj);
      r1maj.nodes.Add(new Node(new Vector3(300, 0, 0), r1maj.name));
      r1maj.nodes.Add(new Node(new Vector3(300, 0, 100), r1maj.name));
      r1maj.nodes.Add(new Node(new Vector3(600, 0, 240), r1maj.name));

      Road r2 = new Road("path", this.gameObject, Road.KIND_PATH);
      Roads.Add(r2);
      r2.nodes.Add(new Node(new Vector3(0, 0, 250), r2.name));
      r2.nodes.Add(new Node(new Vector3(0, 0, 350), r2.name));
      r2.nodes.Add(new Node(new Vector3(200, 0, 350), r2.name));

      Road r3 = new Road("rail", this.gameObject, Road.KIND_RAIL);
      Roads.Add(r3);
      r3.nodes.Add(new Node(new Vector3(0, 0, 350), r3.name));
      r3.nodes.Add(new Node(new Vector3(0, 0, 450), r3.name));
      r3.nodes.Add(new Node(new Vector3(400, 0, 550), r3.name));
      */

      TileBoundsMeters = this.transform.parent.GetComponent<MassiveTile>().TileBoundsMeters;
      mc = transform.parent.Find("terrain").GetComponent<MeshCollider>();
     // GridSplit(true);
      CalculateIntersections();
      CreateRoadGeometry();
      //VisualizeNodes();
    }

    public void SetMaterials(Material iRoadMat,
      Material iRailMat,
      Material iPathMat,
      Material iServiceMat)
    {
      MajorRoadMaterial = iRoadMat;
      MinorRoadMaterial = iRoadMat;
      RailMaterial = iRailMat;
      PathMaterial = iPathMat;
      SurfaceMaterial = iServiceMat;
    }



    private Material GetMaterialForKind(string kind, string kind_detail)
    {
      switch (kind)
      {
        case Road.KIND_HIGHWAY:
          return MajorRoadMaterial;
        case Road.KIND_MAJOR_ROAD:
          return MajorRoadMaterial;
        case Road.KIND_MINOR_ROAD:
          if (kind_detail == Road.KIND_SERVICE)
          {
            return SurfaceMaterial;
          }
          else
          {
            return MajorRoadMaterial;
          }
        case Road.KIND_SERVICE:
          return SurfaceMaterial;
        case Road.KIND_PATH:
          if (kind_detail == Road.KIND_PEDESTRIAN)
          {
            return SurfaceMaterial;
          }
          else
            if (kind_detail == Road.KIND_CYCLEWAY)
          {
            return SurfaceMaterial;
          }
          else
          {
            return PathMaterial;
          }
        case Road.KIND_RAIL:
          return RailMaterial;
        case Road.KIND_RUNWAY:
          return SurfaceMaterial;
        default:
          return SurfaceMaterial;
      }
    }


    public void CreateRoads(string sData, DVector3 PositionMeters)
    {
      TileBoundsMeters = this.transform.parent.GetComponent<MassiveTile>().TileBoundsMeters;
      mc = transform.parent.Find("terrain").GetComponent<MeshCollider>();

      Dictionary<string, object> Data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(sData);
      List<List<Vector3>> segments = new List<List<Vector3>>();
      IList features = (IList)Data["features"];

      Roads.Clear();
      foreach (IDictionary feature in features)
      {
        IDictionary geometry = (IDictionary)feature["geometry"];
        IDictionary properties = (IDictionary)feature["properties"];

        string geotype = (string)geometry["type"];
        string name = OSMTools.GetProperty(properties, "name");
        string kind = OSMTools.GetProperty(properties, "kind");
        string kind_detail = OSMTools.GetProperty(properties, "kind_detail");
        //Debug.Log("Processing:" + name);

        if (geotype == "LineString")
        {
          Road rs = new Road(name, this.gameObject, kind, kind_detail);
          MassiveFeature mf = rs.go.AddComponent<MassiveFeature>();
          mf.SetProperties(properties);
          rs.is_bridge = mf.is_bridge;

          for (int i = 0; i < ((IList)geometry["coordinates"]).Count; i++)
          {
            IList piece = (IList)((IList)geometry["coordinates"])[i];
            double dx = (double)piece[0];
            double dz = (double)piece[1];
            Vector3 v = MapTools.LatLonToMeters((float)dz, (float)dx).ToVector3();
            v -= PositionMeters.ToVector3();
            Node n = new Node(v, name);
            rs.nodes.Add(n);
            if (i == 0)
            {
              //rs.go.transform.position = v;
              rs.go.layer = LayerMask.NameToLayer("Roads");
            }
          }
          Roads.Add(rs);
        }


        if (geotype == "MultiLineString")
        {
          GameObject go = new GameObject();
          go.transform.parent = this.transform;
          MassiveFeature mf = go.AddComponent<MassiveFeature>();
          mf.SetProperties(properties);

          for (int i = 0; i < ((IList)geometry["coordinates"]).Count; i++)
          {
            Road rs = new Road(name, go, kind, kind_detail);
            rs.is_bridge = mf.is_bridge;
            go.name = name;
            IList segment = (IList)((IList)geometry["coordinates"])[i];
            for (int j = 0; j < segment.Count; j++)
            {
              IList piece = (IList)segment[j];
              double dx = (double)piece[0];
              double dz = (double)piece[1];
              Vector3 v = MapTools.LatLonToMeters((float)dz, (float)dx).ToVector3();
              v -= PositionMeters.ToVector3();
              Node n = new Node(v, name);
              rs.nodes.Add(n);
              if ((i == 0) && (j == 0))
              {
                //rs.go.transform.position = v;
                rs.go.layer = LayerMask.NameToLayer("Roads");
              }
            }
            Roads.Add(rs);
          }

        }

      }
      //calculate intersections      
      CalculateIntersections();
      CreateRoadGeometry();
      //VisualizeNodes();
    }

    /**
     * <summary>
     * Splits a road by the heightmap grid, allowing perfect fitting
     * </summary> 
     */
    public void GridSplit(bool debug)
    {
      foreach (Road r in Roads)
      {
        List<Node> NewNodes = new List<Node>();
        for (int i = 0; i < r.nodes.Count - 1; i++)
        {
          Node n1 = r.nodes[i];
          Node n2 = r.nodes[i + 1];
          Vector3 vi;
          // out i1, v1s+n1, dir1, v2s + n2, dir2
          NewNodes.Add(n1);
          for (float j = 0; j < 256; j++)
          {
            Vector3 l1 = new Vector3(j * 10, 0, 0);
            Vector3 l2 = new Vector3(j * 10, 0, 5000);
            bool did = MathTools.LineLineIntersection(out vi, n1.pos, n2.pos - n1.pos, l1, l2 - l1);

            if (vi.x > n1.pos.x && vi.x < n2.pos.x)
            {
              r.ds(vi, "IIIII:" + r.name);
              Node n = new Node(vi, n1.NodeName);
              NewNodes.Add(n);
            }
          }
          NewNodes.Add(n2);
        }
        r.nodes.Clear();
        r.nodes = NewNodes;
      }
    }

    public Vector3 Adjust(Road r, Vector3 inp)
    {
      return inp;
      //inp += new Vector3(0, MeshTools.GetMeshHeight(TileBoundsMeters, r.go.transform.position, mc, inp), 0);
      inp += r.GetOffset();
      if (r.is_bridge) inp += new Vector3(0, 6, 0);
      inp += new Vector3(0, 0.2f);
      return inp;
    }


    public void CreateRoadGeometry()
    {
      foreach (Road r in Roads)
      {
        for (int i = 0; i < r.nodes.Count - 1; i++)
        {
          Node o1 = r.nodes[i];
          Node o2 = r.nodes[i + 1];

          Vector3 v1 = o1.pos - r.go.transform.position + this.transform.position;
          //v1 = Adjust(r, v1);
          //v1 += new Vector3( 0, MeshTools.GetMeshHeight(TileBoundsMeters, r.go.transform.position, mc, v1), 0);
          Vector3 v2 = o2.pos - r.go.transform.position + this.transform.position;
          //v2 = Adjust(r, v2);
          //v2 += new Vector3(0, MeshTools.GetMeshHeight(TileBoundsMeters, r.go.transform.position, mc, v2), 0);

          //TODO: if the straight angle between 2 consecutive nodes is low, skip generating connections and join directly
          //float angle = MeshTools.AngleBetween2D(v2-v1, v3-v2);
          //Debug.Log(angle);


          //calculate connection offsets (pull back from end point to fit intersection)
          Vector3 mv1s = MeshTools.LerpByDistance(v1, v2, r.width);
          //GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
          //go1.transform.position = mv1;

          Vector3 mv1e = MeshTools.LerpByDistance(v2, v1, r.width);
          //GameObject go2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
          //go2.transform.position = mv2;          

          //float dist = (v2.pos - v1.pos).magnitude;
          Vector3 n1 = MeshTools.GetNormal(v1, v1, v2) * r.width;
          //
          if (RoadHighQuality == true)
          {
            r.CreateHQStraight(mv1s, mv1e, n1, mc);
          }
          else
          {
            r.CreateStraight(mv1s, mv1e, n1);
          }

          //create connecting geometry between 2 connected line nodes, e.g. an elbow or t
          if (i < r.nodes.Count - 2)
          {
            Node o3 = r.nodes[i + 2];
            Vector3 v3 = o3.pos - r.go.transform.position + this.transform.position;
            //v3 += new Vector3(0, MeshTools.GetMeshHeight(TileBoundsMeters, r.go.transform.position, mc, v3), 0);
            //v3 = Adjust(r, v3);
            Vector3 mv2s = MeshTools.LerpByDistance(v2, v3, r.width);
            //GameObject go3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //go3.transform.position = mv3;

            //v2 = MeshTools.LerpByDistance(v3, v2, r.width);
            Vector3 n2 = MeshTools.GetNormal(v2, v2, v3) * r.width;
            // r.CreateConnection(mv2, mv3, n1, n2);
            r.CreateSmoothConnection(v1, v2, v2, v3,
              mv1e, mv2s, n1, n2);
          }
        }




        r.CreateMesh();

        //MeshTools.FitMeshToTerrain(TileBoundsMeters, r.go.transform.position - this.transform.position, mc, r.m);
        //MeshTools.UpdatePivot(r.go, r.m); 
        r.go.AddComponent<MeshFilter>().mesh = r.m;
        MeshRenderer re = r.go.AddComponent<MeshRenderer>();
        re.material = GetMaterialForKind(r.kind, r.kind_detail);

        r.m.RecalculateBounds();

        re.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        re.receiveShadows = true;

        r.go.AddComponent<MeshCollider>();

      }

      this.transform.position = new Vector3(0, 0.1f, 0);
    }


    public void CalculateIntersections()
    {
      for (int i = 0; i < Roads.Count; i++)
      {
        for (int j = 0; j < Roads.Count; j++)
        {
          if (j == i) continue;
          Road r1 = Roads[i];
          Road r2 = Roads[j];
          Node n1 = r1.nodes[0]; // the first node in the list will own the intersection
          for (int k = 0; k < r2.nodes.Count; k++)
          {
            //if we already have an intersection for this node pair then continue
            if (n1.CheckIntersection(r2.nodes[k])) continue;

            //check for elbow joins on end nodes
            if ((k == 0) || (k >= r2.nodes.Count - 1))
            {

              if (Vector3.Distance(n1.pos, r2.nodes[k].pos) < IntersectionThresholdMeters)
              {
                //Debug.Log("Elbow found " + r1.name + "-" + r2.name);
                //r1.nodes[0].type = Node.eNodeTypes.ELBOWJUNCTION;
                n1.AddIntersection(r2.nodes[k], Node.eNodeTypes.ELBOWJUNCTION);
              }
            }
            else //everything else
            {
              if (Vector3.Distance(n1.pos, r2.nodes[k].pos) < IntersectionThresholdMeters)
              {
                //Debug.Log("T found " + r1.name + "-" + r2.name);
                n1.AddIntersection(r2.nodes[k], Node.eNodeTypes.TJUNCTION);
              }
            }
          }

        }
      }
    }

    public void VisualizeNodes()
    {
      foreach (Road r in Roads)
      {
        foreach (Node n in r.nodes)
        {
          n.Visualize(r.go, this.transform.position);
        }
      }

    }
  }

}