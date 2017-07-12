using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{
  public class OSMTools
  {

    public static float GetFloatProperty(IDictionary properties, string id)
    {
      float f = 0;
      if (properties[id] != null)
      {
        string sf = properties[id].ToString();
        f = float.Parse(sf);
      };
      return f;
    }

    public static bool GetBoolProperty(IDictionary properties, string id)
    {
      bool f = false;
      if (properties[id] != null)
      {
        string sf = properties[id].ToString();
        f = bool.Parse(sf);
      };
      return f;
    }

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
    public static List<List<Vector3>> CreatePolygon(IList geometry, Vector3 Offset)
    {
      List<List<Vector3>> lists = new List<List<Vector3>>();
      List<Vector3> list = new List<Vector3>();
      foreach (IList p in geometry)
      {
        double dx = (double)p[0];
        double dz = (double)p[1];
        Vector3 v = MapTools.LatLonToMeters((float)dz, (float)dx).ToVector3();
        v -= Offset;
        list.Add(v);
      }
      lists.Add(list);
      return lists;
    }

    public static List<List<Vector3>> CreateMultiPolygon(IList segments, DVector3 Offset)
    {
      List<List<Vector3>> lists = new List<List<Vector3>>();

      foreach (IList polygon in segments)
      {
        List<Vector3> list = new List<Vector3>();
        foreach (IList p in polygon)
        {
          double dx = (double)p[0];
          double dz = (double)p[1];
          Vector3 v = MapTools.LatLonToMeters((float)dz, (float)dx).ToVector3();
          v -= Offset.ToVector3();
          list.Add(v);
        }
        lists.Add(list);
      }

      return lists;
    }

   
    /**
     * Interpolates a list of points, inserting amt points between each vertex equally spaced
     * TODO: Try this formula to space by distance (this avoids dense clumping on small lists)
     * dx = x1 - x0
dy = y1 - y0
l = Sqrt(dx^2 + dy^2)
x = x0 + dx/l * t
y = y0 + dy/l * t
     */
    public static List<Vector3> InsertPoints(List<Vector3> p, int amt)
    {
      int MaxInsertions = 200;
      List<Vector3> vout = new List<Vector3>();

      Vector3 first = p[0];

      for (int i = 0; i < p.Count; i++)
      {
        vout.Add(p[i]);
        if (i < p.Count - 1)
        {
          Vector3 n = p[i + 1];

          Vector3 dif = n - p[i];

          float stepsize = (1.0f / (float)amt);

          Vector3 dir = dif.normalized;

          float dist = Vector3.Distance(p[i], n);
          for (float f = 0; f < MaxInsertions; f++)
          {
            Vector3 inb = MeshTools.LerpByDistance(p[i], n, f * amt);
            if (Vector3.Distance(first, inb) > Vector3.Distance(first, n))
            {
              break;
            }
            vout.Add(inb);
          }

          /*
          for ( float f = stepsize; f< 1; f += stepsize) {
            Vector3 inb = Vector3.Lerp(p[i], n, f);
            vout.Add(inb);
          }
          */

        }
      }

      return vout;
    }

  }
}