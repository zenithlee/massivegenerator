using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{
  public class OSMTools
  {

    public static string GetProperty(IDictionary properties, string id)
    {
      string name = "";
      if (properties[id] != null)
      {
        name = properties[id].ToString();
      };
      return name;
    }
    //points in lat lon, form a closed loop
    public static List<List<Vector3>> CreatePolygon(IDictionary geometry, Vector3 Offset)
    {
      List<List<Vector3>> segments = new List<List<Vector3>>();
      List<Vector3> list = new List<Vector3>();
      foreach (IList piece in (IList)geometry["coordinates"])
      {
        foreach (IList p in piece)
        {
          double dx = (double)p[0];
          double dz = (double)p[1];
          Vector3 v = MapFuncs.LatLonToMeters((float)dz, (float)dx).ToVector3();
          v -= Offset;
          list.Add(v);
        }
        segments.Add(list);
      }      
      return segments;
    }

    public static List<List<Vector3>> CreateMultiPolygon(IList segments, DVector3 Offset)
    {
      List<List<Vector3>> lists= new List<List<Vector3>>();
      
        foreach (IList polygon in segments)
        {
          List<Vector3> list = new List<Vector3>();
          foreach (IList p in polygon)
          {
            double dx = (double)p[0];
            double dz = (double)p[1];
            Vector3 v = MapFuncs.LatLonToMeters((float)dz, (float)dx).ToVector3();
            v -= Offset.ToVector3();
            list.Add(v);
          }
        lists.Add(list);
        }      

      return lists;
    }

  }
}