using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{
  public class EditorGlobals
  {

    // all height values have this added to them, this allows underwater and caves
    public static double HeightOffset = 10.0;
    // anything below this will be flattened or terraced
    public static double SeaLevel = 20.0;
    public static double MaxTerrainHeight = 8900.0;

  }

}