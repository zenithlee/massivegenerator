using System;
using UnityEngine;

[System.Serializable]
public class DVector3 : IEquatable<DVector3>
{
    [SerializeField]
    public double x, y, z;

    public DVector3()
    {
    }

    public DVector3(double ix, double iy, double iz)
    {
        x = ix;
        y = iy;
        z = iz;
    }

    public DVector3(float ix, float iy, float iz)
    {
        x = (double)ix;
        y = (double)iy;
        z = (double)iz;
    }

    public void AddVector3(Vector3 v)
    {
        x += v.x;
        y += v.y;
        z += v.z;
    }

  public DVector3 AbsoluteValues()
  {
    return new DVector3(Math.Abs(x), Math.Abs(y), Math.Abs(z));
  }

  /**
   * WARNING: Will lose precision
   */
  public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static DVector3 FromVector3(Vector3 v)
    {
        DVector3 dv = new DVector3();
        dv.x = (double)v.x;
        dv.y = (double)v.y;
        dv.z = (double)v.z;
        return dv;
    }

    // Does no error checking, input string is assumed to be "x.xx,y.yy,z.zz"
    public static DVector3 FromString(string s)
    {
        DVector3 d = new DVector3();
        if (string.IsNullOrEmpty(s))
        {
            return d;
        }
        string[] parts = s.Split(',');
        d.x = double.Parse(parts[0]);
        d.y = double.Parse(parts[1]);
        d.z = double.Parse(parts[2]);
        return d;
    }

    public override string ToString()
    {
        return x.ToString() + "," + y.ToString() + "," + z.ToString();
    }

    public DVector3 Copy()
    {
        DVector3 t = new DVector3();
        t.x = x;
        t.y = y;
        t.z = z;
        return t;
    }

    public static DVector3 operator *(DVector3 dv, double m)
    {
        return new DVector3(dv.x * m, dv.y * m, dv.z * m);
    }

    public static DVector3 operator -(DVector3 a, DVector3 b)
    {   
        return new DVector3( a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static DVector3 operator -(DVector3 a, Vector3 b)
    {
        return new DVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static DVector3 operator +(DVector3 a, Vector3 b)
    {        
        return new DVector3( a.x + b.x, a.y + b.y, a.z + b.z);
    }
  public static DVector3 operator +(DVector3 a, DVector3 b)
  {
    return new DVector3(a.x + b.x, a.y + b.y, a.z + b.z);
  }

  public static bool operator ==(DVector3 lhs, DVector3 rhs)
    {
        return (double)DVector3.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
    }

    public static bool operator !=(DVector3 lhs, DVector3 rhs)
    {
        return (double)DVector3.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
    }

    public static double SqrMagnitude(DVector3 a)
    {
        return a.x * a.x + a.y * a.y + a.z * a.z;
    }

   

    public override bool Equals(object a)
    {        
        return ReferenceEquals(this, a);
    }

    public bool Equals(DVector3 b)
    {
        return (b.x == x && b.y == y && b.z == z);
    }

    public override int GetHashCode()
    {
        // Overflow is fine, wrap
        unchecked
        {
            int hash = 17;            
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            hash = hash * 23 + z.GetHashCode();
            return hash;
        }
    }
}