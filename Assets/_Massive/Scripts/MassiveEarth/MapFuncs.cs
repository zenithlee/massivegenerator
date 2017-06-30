using UnityEngine;
using System.Collections;
using System;
using System.Text;

namespace _Massive { 

public class MapFuncs  { 

	private const double EarthRadius = 6378137;
	private const double MinLatitude = -85.05112878;
	private const double MaxLatitude = 85.05112878;
	private const double MinLongitude = -180;
	private const double MaxLongitude = 180;

  private const int TileSize = 256;  
  private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
      private const double OriginShift = 2 * Math.PI * EarthRadius / 2;
    //private const double OriginShift = 0;


    /*
    public static Vector2 TileMeshSize(int TileX, int TileZ, int ZoomLevel)
    {
      int pixelX, pixelZ;
      TileXYToPixelXY(TileX, TileZ, out pixelX, out pixelZ);
      double lat, lon;
      PixelXYToLatLong(pixelX, pixelZ, ZoomLevel, out lat, out lon);
      double GroundRes = GroundResolution(lat, ZoomLevel);
      return new Vector2((float)GroundRes * 256, (float)GroundRes * 256);
    }
    */

    private static double Clip(double n, double minValue, double maxValue)
	{
		return Math.Min(Math.Max(n, minValue), maxValue);
	}
	
	public static uint MapSize(int levelOfDetail)
	{
		return (uint) 256 << levelOfDetail;
	}
	
	public static double GroundResolution(double latitude, int levelOfDetail)
	{
		latitude = Clip(latitude, MinLatitude, MaxLatitude);
		return Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / MapSize(levelOfDetail);
	}
	
	public static double MapScale(double latitude, int levelOfDetail, int screenDpi)
	{
		return GroundResolution(latitude, levelOfDetail) * screenDpi / 0.0254;
	}

  /*
  public static void PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail, out double latitude, out double longitude)
	{
		double mapSize = MapSize(levelOfDetail);
		double x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
		double y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);
		
		latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
		longitude = 360 * x;
	}
  */
	
      /*
	public static void TileXYToPixelXY(int tileX, int tileY, out int pixelX, out int pixelY)
	{
		pixelX = tileX * 256;
		pixelY = tileY * 256;
	}
  */

  public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
  {
    StringBuilder quadKey = new StringBuilder();
    for (int i = levelOfDetail; i > 0; i--)
    {
      char digit = '0';
      int mask = 1 << (i - 1);
      if ((tileX & mask) != 0)
      {
        digit++;
      }
      if ((tileY & mask) != 0)
      {
        digit++;
        digit++;
      }
      quadKey.Append(digit);
    }
    return quadKey.ToString();
  }

  public static Vector2 WorldToTilePos(double lon, double lat, int zoom)
  {
    Vector2 p = new Vector2();
    p.x = (float)((lon + 180.0) / 360.0 * (1 << zoom));
    p.y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
      1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

    return p;
  }

  public static DVector3 TileToWorldPos(DVector3 t, int zoom)
  {
    DVector3 p = new DVector3();
    double n = Math.PI - ((2.0 * Math.PI * t.z) / Math.Pow(2.0, zoom));

    p.x = (float)((t.x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
    p.z = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

    return p;
  }

  //Returns bounds of the given tile in EPSG:900913 coordinates
  public static DRect TileBoundsInMeters(DVector3 tileindex, int zoom)
  {
      DVector3 min = PixelsToMeters(new DVector3(tileindex.x * TileSize, 0, tileindex.z * TileSize), zoom);
      DVector3 max = PixelsToMeters(new DVector3((tileindex.x + 1) * TileSize, 0, (tileindex.z + 1) * TileSize), zoom);    
    return new DRect(new DVector3(min.x, 0, min.z), new DVector3(max.x, 0,max.z));
  }

  //Returns bounds of the given tile in latutude/longitude using WGS84 datum
  public static DRect TileLatLonBounds(DVector3 t, int zoom)
  {
    DRect bound = TileBoundsInMeters(t, zoom);
    DVector3 min = MetersToLatLon(new DVector3(bound.Min.x, 0, bound.Min.z));
    DVector3 max = MetersToLatLon(new DVector3(bound.Min.x + bound.Size.x, 0, bound.Min.z + bound.Size.z));
    return new DRect(new DVector3(min.x, 0, min.z), new DVector3(max.x, 0, max.z));
  }


    /// <summary>
    /// Gets the WGS84 longitude of the northwest corner from a tile's X position and zoom level.
    /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
    /// </summary>
    /// <param name="x"> Tile X position. </param>
    /// <param name="zoom"> Zoom level. </param>
    /// <returns> NW Longitude. </returns>
    public static double TileXToNWLongitude(int x, int zoom)
    {
      var n = Math.Pow(2.0, zoom);
      var lon_deg = x / n * 360.0 - 180.0;
      return lon_deg;
    }

    /// <summary>
    /// Gets the WGS84 latitude of the northwest corner from a tile's Y position and zoom level.
    /// See: http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames.
    /// </summary>
    /// <param name="y"> Tile Y position. </param>
    /// <param name="zoom"> Zoom level. </param>
    /// <returns> NW Latitude. </returns>
    public static double TileYToNWLatitude(int y, int zoom)
    {
      var n = Math.Pow(2.0, zoom);
      var lat_rad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
      var lat_deg = lat_rad * 180.0 / Math.PI;
      return lat_deg;
    }

    /// <summary>
    /// Gets the <see cref="T:Mapbox.Utils.Vector2dBounds"/> of a tile.
    /// </summary>
    /// <param name="x"> Tile X position. </param>
    /// <param name="y"> Tile Y position. </param>
    /// <param name="zoom"> Zoom level. </param>
    /// <returns> The <see cref="T:Mapbox.Utils.Vector2dBounds"/> of the tile. </returns>
    public static DRect TileIdToBounds(int x, int z, int zoom)
    {
      DVector3 sw = new DVector3(TileYToNWLatitude(z, zoom), 0, TileXToNWLongitude(x + 1, zoom));
      DVector3 ne = new DVector3(TileYToNWLatitude(z + 1, zoom), 0, TileXToNWLongitude(x, zoom));
      return new DRect(sw, ne);
    }

    /// <summary>
    /// Gets the WGS84 lat/lon of the center of a tile.
    /// </summary>
    /// <param name="x"> Tile X position. </param>
    /// <param name="y"> Tile Y position. </param>
    /// <param name="zoom"> Zoom level. </param>
    /// <returns>A <see cref="T:UnityEngine.Vector2d"/> of lat/lon coordinates.</returns>
    public static DVector3 TileIdToCenterLatitudeLongitude(int x, int y, int zoom)
    {
      var bb = TileIdToBounds(x, y, zoom);
      var center = bb.Center;
      return new DVector3(center.x, 0, center.z);
    }


    //Converts XY point from (Spherical) Web Mercator EPSG:3785 (unofficially EPSG:900913) to lat/lon in WGS84 Datum
    public static DVector3 MetersToLatLon(DVector3 m)
  {
    DVector3 ll = new DVector3();
    ll.x = (m.x / OriginShift) * 180;
    ll.z = (m.z / OriginShift) * 180;
    ll.z = 180 / Math.PI * (2 * Math.Atan(Math.Exp(ll.z * Math.PI / 180)) - Math.PI / 2);
    return new DVector3(ll.z, 0, ll.x);
  }

  //Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
  public static DVector3 LatLonToMeters(double lat, double lon)
  {
    DVector3 p = new DVector3();
    p.x = (lon * OriginShift / 180);
    p.y = (Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180));
    p.y = (p.y * OriginShift / 180);
    return new DVector3(p.x, 0, p.y);
  }

  public static DVector3 TileToMeters(int TileX, int TileZ, int ZoomLevel)
  {
    return PixelsToMeters(new DVector3(TileX * TileSize, 0, TileZ * TileSize), ZoomLevel);    
  }

  //Converts EPSG:900913 to pyramid pixel coordinates in given zoom level
  public static DVector3 MetersToPixels(DVector3 m, int zoom)
  {
    var res = Resolution(zoom);
    var pix = new DVector3();
    pix.x = ((m.x + OriginShift) / res);
    pix.y = ((-m.y + OriginShift) / res);
    return pix;
  }

  //Converts pixel coordinates in given zoom level of pyramid to EPSG:900913
  public static DVector3 PixelsToMeters(DVector3 p, int zoom)
  {
    var res = Resolution(zoom);
    var met = new DVector3();
    met.x = (p.x * res - OriginShift);
    met.z = (p.z * res - OriginShift); /// removed -
    return met;
  }

  //Resolution (meters/pixel) for given zoom level (measured at Equator)
  public static double Resolution(int zoom)
  {
    return InitialResolution / (Math.Pow(2, zoom));
  }

    public static DVector3 ProportionInBounds(DVector3 pos, DRect rect)
    {
      DVector3 r = new DVector3();

      //TODO: check if DRECT has reversed min/max
      r.x = (pos.x - rect.Max.x) / rect.Size.x;
      r.y = (pos.y - rect.Max.y) / rect.Size.y;
      r.z = (pos.z - rect.Max.z) / rect.Size.z;

      return r;

    }

}

}