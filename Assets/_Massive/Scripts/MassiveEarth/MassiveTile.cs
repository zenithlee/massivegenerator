using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  [ExecuteInEditMode]
  public class MassiveTile : MonoBehaviour
  {

    //height values, always 256x256
    [SerializeField]
    public double[,] dHeights;

    //TileX, TileZ, Zoom
    public DVector3 TileIndex;
    public int ZoomLevel = 0;
    public DVector3 PositionMeters;
    [SerializeField]
    public DRect TileBoundsMeters;
    bool HasFirst = false;

    public DVector3 Size;
    
    string RoadStringJSON;
    public Dictionary<string, object> Data = new Dictionary<string, object>();        
    public Material WaterMaterial;    

    enum eLayers { boundaries, buildings, earth, landuse, places, pois, roads, transit, water };

    [Multiline(10)]
    public string Info;    

    //x,y,z tile index for the zoom level, same as mapzen or bing maps id
    public void SetTileIndex(Vector3 pos, int Zoom)
    {
      ZoomLevel = Zoom;
      TileIndex = DVector3.FromVector3(pos);
      //DRect r = MapFuncs.TileBoundsInMeters(new DVector3(pos.x, 0, pos.z), Zoom);
      //DVector3 c = MapFuncs.TileIdToCenterLatitudeLongitude((int)pos.x, (int)pos.z, Zoom);
      DRect latlonbounds = MapTools.TileIdToBounds((int)pos.x, (int)pos.z, Zoom);
      DVector3 c = latlonbounds.Max;
      PositionMeters = MapTools.LatLonToMeters(c.x, c.z);

      TileBoundsMeters = new DRect(MapTools.LatLonToMeters(latlonbounds.Min.x, latlonbounds.Min.z),
        MapTools.LatLonToMeters(latlonbounds.Max.x, latlonbounds.Max.z));
      Info = TileBoundsMeters.ToString();
    }
    

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
  }


}