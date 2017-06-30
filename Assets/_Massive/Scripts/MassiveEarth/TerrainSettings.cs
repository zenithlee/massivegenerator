using UnityEngine;
using System.Collections;

namespace _Massive { 

public class TerrainSettings : MonoBehaviour {

  public int TileX = 1193;
  public int TileZ = 1202;
  public float Latitude = 0;
  public float Longitude = 0;
  public float CliffAngle = 45;
  public float SeaLevel = 10;
  public int HeightMapResolution = 255;
  public int MetersPerTile = 17048;
    public int DetailResolution = 1024;
    public int DetailResolotionPerPatch = 8;
    public int SmoothingPasses = 3;

    public string GetLocalPath()
    {
      return "Assets/_Massive/Temp/" + TileX + "_" + TileZ;
    }

  public void CopyTo(TerrainSettings t)
  {
    t.CliffAngle = CliffAngle;
    t.SeaLevel = SeaLevel;
    t.Latitude = Latitude;
    t.Longitude = Longitude;
    t.HeightMapResolution = HeightMapResolution;
    t.MetersPerTile = MetersPerTile;
      t.DetailResolution = DetailResolution;
      t.DetailResolotionPerPatch = DetailResolotionPerPatch;
      t.SmoothingPasses = SmoothingPasses;
  }	
}

}