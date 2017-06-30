using UnityEngine;

namespace _Massive
{
  [System.Serializable]
  public class DRect
  {
    [SerializeField]
    public DVector3 Min { get; private set; }
    [SerializeField]
    public DVector3 Max { get; private set; }
    public DVector3 Size { get; private set; }

    public override string ToString()
    {
      return Min.ToString() + "\n" + Max.ToString();
    }

    public DVector3 Center
    {
      get
      {
        return new DVector3(Min.x + Size.x / 2, 0, Min.z + Size.z / 2);
      }
      set
      {
        Min = new DVector3(value.x - Size.x / 2, 0, value.x - Size.z / 2);
      }
    }

    public static DRect operator -(DRect a, DVector3 b)
    {
      return new DRect(a.Min - b, a.Max - b);
    }

    public void Move(DVector3 offset)
    {
      Max = Max + offset;
      Min = Min + offset;      
    }

    public double Height
    {
      get { return Size.y; }
    }

    public double Width
    {
      get { return Size.x; }
    }

    public DRect(DVector3 min, DVector3 max)
    {
      Min = min;
      Max = max;
      Size = (Max - Min).AbsoluteValues();
    }

    public bool Contains(DVector3 point)
    {
      bool flag = Width < 0.0 && point.x <= Min.x && point.x > (Min.x + Size.x) || Width >= 0.0 && point.x >= Min.x && point.x < (Min.x + Size.x);
      return flag && (Height < 0.0 && point.z <= Min.z && point.z > (Min.z + Size.z) || Height >= 0.0 && point.z >= Min.z && point.z < (Min.z + Size.z));
    }
  }
}
