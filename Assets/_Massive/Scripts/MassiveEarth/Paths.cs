using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  public enum ePATHSPEC
  {
    TEMPFILES, TERRAINBASE, TERRAINPATH, TERRAINFULLNAME, IMAGEEXPORT,
    BASEMAPURL, HEIGHTURL, SKINPATH, SATTELITEMAPURL, MAPZENURL, NORMALMAPURL, OSMURL,
    TEXTUREPATH, NORMALMAPPATH, HEIGHTMAPATH, HEIGHTMAPCONTPATH, MATERIALPATH, MESHOBJPATH, MESHASSETPATH, PREFABPATH, ROADSPATH, WATERPATH, BUILDINGSPATH,
    ROADSASSETPATH, WATERASSETPATH, BUILDINGSASSETPATH, OSMPath,
    GEOINFOURL, GEOINFOPATH, GEOWIKIURL, GEOWIKIPATH
  };


  public class Paths
  {

    public static int ZoomLevel = 14;
    public const string TERRAINBASE = "Assets/_Massive/Terrains";
    public static int TileX;
    public static int TileZ;

    public static string GetPath(ePATHSPEC ps)
    {
      switch (ps)
      {
        case ePATHSPEC.TERRAINBASE:
          return TERRAINBASE + "/" + ZoomLevel;
        case ePATHSPEC.TERRAINPATH:
          return TERRAINBASE + "/" + ZoomLevel + "/" + TileZ + "/" + TileX;
        case ePATHSPEC.TERRAINFULLNAME:
          return TERRAINBASE + "/" + ZoomLevel + "/" + TileZ + "/" + TileX + "/" + TileX + "_" + TileZ;
        case ePATHSPEC.BASEMAPURL:
          return "http://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial";
        case ePATHSPEC.SATTELITEMAPURL:
          //a300231032023001
          //return "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/{zoom}/{x}/{z}.png?api_key={mapzenkey}";
          return "https://api.mapbox.com:443/v4/mapbox.satellite/{zoom}/{x}/{z}@2x.png?access_token={mapboxkey}";
        //  return "https://t1.ssl.ak.tiles.virtualearth.net/tiles/a{quadkey}.jpeg?g=5772&n=z&c4w=1";
        case ePATHSPEC.HEIGHTURL:
          return "https://api.mapbox.com/v4/mapbox.terrain-rgb/{zoom}/{x}/{z}.pngraw?access_token={mapboxkey}";
        //return "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/{zoom}/{x}/{z}.png?api_key={mapzenkey}";
        //return "http://dev.virtualearth.net/REST/v1/Elevation/Bounds?bounds=";
        case ePATHSPEC.NORMALMAPURL:
          return "https://tile.mapzen.com/mapzen/terrain/v1/normal/{zoom}/{x}/{z}.png?api_key={mapzenkey}";
        case ePATHSPEC.MAPZENURL:
          return "https://tile.mapzen.com/mapzen/vector/v1/";
        case ePATHSPEC.GEOINFOURL:
          return "http://api.geonames.org/citiesJSON?north={n}&south={s}&east={e}&west={w}&lang=de&username=geosentient";
        case ePATHSPEC.GEOWIKIURL:
          return "http://api.geonames.org/wikipediaBoundingBox?{n}=44.1&{s}=-9.9&{e}=-22.4&{w}=55.2&username=geosentient";
        case ePATHSPEC.OSMURL:
          return "http://api.openstreetmap.org/api/0.6/map?bbox={x1},{y1},{x2},{y2}";
        case ePATHSPEC.TEXTUREPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/texture.png";
        case ePATHSPEC.NORMALMAPPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/normals.png";
        case ePATHSPEC.HEIGHTMAPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/height.png";
        case ePATHSPEC.HEIGHTMAPCONTPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) +"/heightcont.png";
        case ePATHSPEC.MESHOBJPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/mesh.obj";
        case ePATHSPEC.MESHASSETPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/mesha.asset";
        case ePATHSPEC.MATERIALPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/material.mat";
        case ePATHSPEC.PREFABPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/prefab.prefab";
        case ePATHSPEC.ROADSPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/roads.json";
        case ePATHSPEC.WATERPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/water.json";
        case ePATHSPEC.GEOINFOPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/info.json";
        case ePATHSPEC.GEOWIKIPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/wiki.json";
        case ePATHSPEC.OSMPath:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/osm.osm";

        case ePATHSPEC.BUILDINGSPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/buildings.json";
        case ePATHSPEC.ROADSASSETPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/roads.asset";
        case ePATHSPEC.WATERASSETPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/water.asset";
        case ePATHSPEC.BUILDINGSASSETPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/buildings.asset";
        default:
          return "Assets/_Massive";
      }
    }

  }
}