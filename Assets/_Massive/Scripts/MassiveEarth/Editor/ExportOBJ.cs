// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// C # manual conversion work by Yun Kyu Choi

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace _Massive
{

  enum SaveFormat { Triangles, Quads }
  enum SaveResolution { Full = 0, Half, Quarter, Eighth, Sixteenth }


  class ExportOBJ : EditorWindow
  {
    SaveFormat saveFormat = SaveFormat.Triangles;
    SaveResolution saveResolution = SaveResolution.Half;

    static TerrainData terrain;
    static Vector3 terrainPos;

    static int tCount;
    static int counter;
    static int totalCount;
    static int progressUpdateInterval = 10000;    
    static float[,] _heightData;

    [MenuItem("Terrain/Export To Obj...")]
    static void Init()
    {
      terrain = null;
      Terrain terrainObject = Selection.activeObject as Terrain;
      if (!terrainObject)
      {
        terrainObject = Terrain.activeTerrain;
      }
      if (terrainObject)
      {
        terrain = terrainObject.terrainData;
        terrainPos = terrainObject.transform.position;
      }

      EditorWindow.GetWindow<ExportOBJ>().Show();
    }

    public static void Export(Terrain t, string Path)
    {
      ExportTerrain(t.terrainData, Path, SaveResolution.Half, SaveFormat.Triangles);
    }

    public static void ExportFromHeight(Texture2D t, string Path, Vector3 TileSize)
    {
      ExportTerrainFromHeightMap(t, Path, SaveResolution.Half, SaveFormat.Triangles, TileSize);
    }

    public static void ExportMapZenHeight(MassiveTile tile, Texture2D t, string Path, DVector3 TileSize, DVector3 TileLatLon)
    {
      ExportFromMapZenHeight(tile, t, Path, SaveResolution.Half, SaveFormat.Triangles, TileSize);
      //ExportZenHeight(Container, t, Path, TileSize, TileLatLon);
    }

    public static float QueryHeightData(float x, float y)
    {
      //return dheights[(int)Mathf.Clamp(x * 256, 0, 255), (int)Mathf.Clamp(y * 256, 0, 255)];      
        if (_heightData != null)
        {
          var intX = (int)Mathf.Clamp(x * 256, 0, 255);
          var intY = (int)Mathf.Clamp(y * 256, 0, 255);
          return _heightData[intX, intY];
        }

        return 0;
    }

    public static void ExportZenHeight(GameObject Container, Texture2D HeightMap, string filename, DVector3 TileSize, DVector3 TileLatLon)
    {
      // You can change that line to provide another MeshFilter
      MeshFilter filter = Container.AddComponent<MeshFilter>();
      Container.AddComponent<MeshRenderer>();      
      filter.sharedMesh = new Mesh();
      Mesh mesh = filter.sharedMesh;
      //mesh.Clear();

      float length = 1f;
      float width = 1f;

      int hx = HeightMap.width;
      int hz = HeightMap.height;
      _heightData = new float[hx, hz];

      
      for (int xx = 0; xx < hx; xx++)
      {
        for (int zz = 0; zz < hz; zz++)
        {
          Color c = HeightMap.GetPixel(xx, zz);
          double height = TextureTools.GetAbsoluteHeightFromColor(c);
          _heightData[xx, zz] = (float)height;  //height values are reversed in unity terrain
        }
      } 

      int resX = 21; // 2 minimum
      int resZ = 21;

      float _relativeScale = 1 / Mathf.Cos(Mathf.Deg2Rad * (float)TileLatLon.x);
      #region Vertices		
      Vector3[] vertices = new Vector3[resX * resZ];
      for (float z = 0; z < resZ; z++)
      {
        // [ -length / 2, length / 2 ]
        float zPos = ((float)z / (resZ - 1) - .5f) * length;
        for (float x = 0; x < resX; x++)
        {
          // [ -width / 2, width / 2 ]
          float xPos = ((float)x / (resX - 1) - .5f) * width;
          //float h = (float)dheights[z*2, x*2];
          //if ( x >= resX -2 ) h = (float)dheights[z * 2, HeightMap.height-1];
          float h = QueryHeightData(x / (resX - 1), 1 - z / (resZ - 1)) * _relativeScale;
          vertices[(int)(x + z * resX)] = new Vector3(xPos, (float)h, zPos);
        }
      }
      #endregion

      #region Normales
      Vector3[] normales = new Vector3[vertices.Length];
      for (int n = 0; n < normales.Length; n++)
        normales[n] = Vector3.up;
      #endregion

      #region UVs		
      Vector2[] uvs = new Vector2[vertices.Length];
      for (int v = 0; v < resZ; v++)
      {
        for (int u = 0; u < resX; u++)
        {
          uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
        }
      }
      #endregion

      #region Triangles
      int nbFaces = (resX - 1) * (resZ - 1);
      int[] triangles = new int[nbFaces * 6];
      int t = 0;
      for (int face = 0; face < nbFaces; face++)
      {
        // Retrieve lower left corner from face ind
        int i = face % (resX - 1) + (face / (resZ - 1) * resX);

        triangles[t++] = i + resX;
        triangles[t++] = i + 1;
        triangles[t++] = i;

        triangles[t++] = i + resX;
        triangles[t++] = i + resX + 1;
        triangles[t++] = i + 1;
      }
      #endregion

      mesh.vertices = vertices;
      mesh.normals = normales;
      mesh.uv = uvs;
      mesh.triangles = triangles;

      mesh.RecalculateBounds();
      Container.transform.localScale = TileSize.ToVector3() + new Vector3(0, 1, 0);
    }

    //NOTE: expects a continuity Heightmap, not a regular heightmap. The cont. map contains edge pixels from adjacent maps.
    static void ExportFromMapZenHeight(MassiveTile tile, Texture2D HeightMap, string fileName, SaveResolution saveResolution, SaveFormat saveFormat, DVector3 TileSize)
    {

      EditorUtility.ClearProgressBar();
      //TerrainData td = new TerrainData();
      //td.baseMapResolution = 256;
      //td.heightmapResolution = 256;

      int w = HeightMap.width  ;
      int h = HeightMap.height ;
      

      DVector3 size = new DVector3(TileSize.x, 1, TileSize.z);
      tile.dHeights = new double[w, h];

      for (int xx = 0; xx < h; xx++)
      {
        for (int zz = 0; zz < w; zz++)
        {

          ////repeat the edge pixels. Stitcher will join adjacent tiles when more tiles are loaded
         // int xxx = xx < h-1 ? xx : xx-1;
          //int zzz = zz < w-1 ? zz : zz - 1;
          Color c = HeightMap.GetPixel(xx, zz);
          //double height = BitmapUtils.MassivePixelToDouble(c) - EditorGlobals.HeightOffset;
          double height = TextureTools.GetAbsoluteHeightFromColor(c);
          //if (height < EditorGlobals.HeightOffset) height = -20;
          //range 0 - 1
          double fh = (float)(height);
          //float fh = (float)(height);
          tile.dHeights[xx, zz] = fh;  //height values are reversed in unity terrain
        }
      }
      //w = (int)TileSize.x;
      //h = (int)TileSize.z;
      Vector3 meshScale = TileSize.ToVector3();
      int tRes = (int)Mathf.Pow(2, (int)saveResolution);
      meshScale = new Vector3(meshScale.x / (w - 1) * tRes, (float)size.y, meshScale.z / (h - 1) * tRes);
      Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));      

      w = (w - 1) / tRes + 1;
      h = (h - 1) / tRes + 1;
      Vector3[] tVertices = new Vector3[w * h];
      Vector2[] tUV = new Vector2[w * h];

      int[] tPolys;

      if (saveFormat == SaveFormat.Triangles)
      {
        tPolys = new int[(w - 1) * (h - 1) * 6];
      }
      else
      {
        tPolys = new int[(w - 1) * (h - 1) * 4];
      }

      // Build vertices and UVs
      for (int y = 0; y < h; y++)
      {
        for (int x = 0; x < w; x++)
        {
          tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, (float)tile.dHeights[y * tRes, x * tRes], x)) + terrainPos;
          tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
        }
      }

      int index = 0;
      if (saveFormat == SaveFormat.Triangles)
      {
        // Build triangle indices: 3 indices into vertex array for each triangle
        for (int y = 0; y < h - 1; y++)
        {
          for (int x = 0; x < w - 1; x++)
          {
            // For each grid cell output two triangles
            tPolys[index++] = (y * w) + x;
            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = (y * w) + x + 1;

            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = ((y + 1) * w) + x + 1;
            tPolys[index++] = (y * w) + x + 1;
          }
        }
      }
      else
      {
        // Build quad indices: 4 indices into vertex array for each quad
        for (int y = 0; y < h - 1; y++)
        {
          for (int x = 0; x < w - 1; x++)
          {
            // For each grid cell output one quad
            tPolys[index++] = (y * w) + x;
            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = ((y + 1) * w) + x + 1;
            tPolys[index++] = (y * w) + x + 1;
          }
        }
      }

      // Export to .obj
      StreamWriter sw = new StreamWriter(fileName);
      try
      {

        sw.WriteLine("# Unity terrain OBJ File");

        // Write vertices
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        counter = tCount = 0;
        totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
        for (int i = 0; i < tVertices.Length; i++)
        {
          UpdateProgress();
          StringBuilder sb = new StringBuilder("v ", 20);
          // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
          // Which is important when you're exporting huge terrains.
          sb.Append(tVertices[i].x.ToString()).Append(" ").
             Append(tVertices[i].y.ToString()).Append(" ").
             Append(tVertices[i].z.ToString());
          sw.WriteLine(sb);
        }
        // Write UVs
        for (int i = 0; i < tUV.Length; i++)
        {
          UpdateProgress();
          StringBuilder sb = new StringBuilder("vt ", 22);
          sb.Append(tUV[i].y.ToString()).Append(" ").
             Append(tUV[i].x.ToString());
          sw.WriteLine(sb);
        }
        if (saveFormat == SaveFormat.Triangles)
        {
          // Write triangles
          for (int i = 0; i < tPolys.Length; i += 3)
          {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("f ", 43);
            sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
               Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
               Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
            sw.WriteLine(sb);
          }
        }
        else
        {
          // Write quads
          for (int i = 0; i < tPolys.Length; i += 4)
          {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("f ", 57);
            sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
               Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
               Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
               Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
            sw.WriteLine(sb);
          }
        }
      }
      catch (Exception err)
      {
        Debug.Log("Error saving file: " + err.Message);
      }
      sw.Close();

      terrain = null;      
      
    }


    /**
     * Given an RGB encoded (Massive Format) heightmap, convert it to terrain data and generate a mesh     
     * DOUBLEPRECISION
     */
    static void ExportTerrainFromHeightMap(Texture2D HeightMap, string fileName, SaveResolution saveResolution, SaveFormat saveFormat, Vector3 TileSize)
    {
      EditorUtility.ClearProgressBar();
      //TerrainData td = new TerrainData();
      //td.baseMapResolution = 256;
      //td.heightmapResolution = 256;

      int w = HeightMap.width+2;
      int h = HeightMap.height+2;

      DVector3 size = new DVector3(TileSize.x, (float)EditorGlobals.MaxTerrainHeight, TileSize.z  );
      double[,] dheights = new double[w, h];

      for (int xx = 0; xx < h; xx++)
      {
        for (int zz = 0; zz < w; zz++)
        {
          int xxx = Math.Min(xx, HeightMap.width-1);
          int zzz = Math.Min(zz, HeightMap.height-1);
          Color c = HeightMap.GetPixel(xxx, zzz);
          //double height = BitmapUtils.MassivePixelToDouble(c) - EditorGlobals.HeightOffset;
          double height = TextureTools.MassivePixelToDouble(c);
          if (height < EditorGlobals.HeightOffset) height = -20;
          //range 0 - 1
          float fh = (float)(height/EditorGlobals.MaxTerrainHeight);
          dheights[xx, zz] = fh;  //height values are reversed in unity terrain
        }
      }

      //td.SetHeights(0, 0, fheights);

      //ExportOBJ.ExportTerrain(td, fileName, saveResolution, saveFormat);

      
      Vector3 meshScale = size.ToVector3();
      int tRes = (int)Mathf.Pow(2, (int)saveResolution);
      meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
      Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
      //float[,] tData = terrain.GetHeights(0, 0, w, h);

      w = (w - 1) / tRes + 1;
      h = (h - 1) / tRes + 1;
      Vector3[] tVertices = new Vector3[w * h];
      Vector2[] tUV = new Vector2[w * h];

      int[] tPolys;

      if (saveFormat == SaveFormat.Triangles)
      {
        tPolys = new int[(w - 1) * (h - 1) * 6];
      }
      else
      {
        tPolys = new int[(w - 1) * (h - 1) * 4];
      }

      // Build vertices and UVs
      for (int y = 0; y < h; y++)
      {
        for (int x = 0; x < w; x++)
        {
          tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, (float)dheights[x * tRes, y * tRes], x)) + terrainPos;
          tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
        }
      }

      int index = 0;
      if (saveFormat == SaveFormat.Triangles)
      {
        // Build triangle indices: 3 indices into vertex array for each triangle
        for (int y = 0; y < h - 1; y++)
        {
          for (int x = 0; x < w - 1; x++)
          {
            // For each grid cell output two triangles
            tPolys[index++] = (y * w) + x;
            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = (y * w) + x + 1;

            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = ((y + 1) * w) + x + 1;
            tPolys[index++] = (y * w) + x + 1;
          }
        }
      }
      else
      {
        // Build quad indices: 4 indices into vertex array for each quad
        for (int y = 0; y < h - 1; y++)
        {
          for (int x = 0; x < w - 1; x++)
          {
            // For each grid cell output one quad
            tPolys[index++] = (y * w) + x;
            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = ((y + 1) * w) + x + 1;
            tPolys[index++] = (y * w) + x + 1;
          }
        }
      }

      // Export to .obj
      StreamWriter sw = new StreamWriter(fileName);
      try
      {

        sw.WriteLine("# Unity terrain OBJ File");

        // Write vertices
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        counter = tCount = 0;
        totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
        for (int i = 0; i < tVertices.Length; i++)
        {
          UpdateProgress();
          StringBuilder sb = new StringBuilder("v ", 20);
          // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
          // Which is important when you're exporting huge terrains.
          sb.Append(tVertices[i].x.ToString()).Append(" ").
             Append(tVertices[i].y.ToString()).Append(" ").
             Append(tVertices[i].z.ToString());
          sw.WriteLine(sb);
        }
        // Write UVs
        for (int i = 0; i < tUV.Length; i++)
        {
          UpdateProgress();
          StringBuilder sb = new StringBuilder("vt ", 22);
          sb.Append(tUV[i].x.ToString()).Append(" ").
             Append(tUV[i].y.ToString()); ///
          sw.WriteLine(sb);
        }
        if (saveFormat == SaveFormat.Triangles)
        {
          // Write triangles
          for (int i = 0; i < tPolys.Length; i += 3)
          {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("f ", 43);
            sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
               Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
               Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
            sw.WriteLine(sb);
          }
        }
        else
        {
          // Write quads
          for (int i = 0; i < tPolys.Length; i += 4)
          {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("f ", 57);
            sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
               Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
               Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
               Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
            sw.WriteLine(sb);
          }
        }
      }
      catch (Exception err)
      {
        Debug.Log("Error saving file: " + err.Message);
      }
      sw.Close();

      terrain = null;
      EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
      EditorWindow.GetWindow<ExportOBJ>().Close();
      EditorUtility.ClearProgressBar();
    }

    /*
     * Convert Terrain Data to Mesh
     * */
    static void ExportTerrain(TerrainData terrain, string fileName, SaveResolution saveResolution, SaveFormat saveFormat)
    {

      int w = terrain.heightmapWidth;
      int h = terrain.heightmapHeight;
      Vector3 meshScale = terrain.size;
      int tRes = (int)Mathf.Pow(2, (int)saveResolution);
      meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
      Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
      float[,] tData = terrain.GetHeights(0, 0, w, h);

      w = (w - 1) / tRes + 1;
      h = (h - 1) / tRes + 1;
      Vector3[] tVertices = new Vector3[w * h];
      Vector2[] tUV = new Vector2[w * h];

      int[] tPolys;

      if (saveFormat == SaveFormat.Triangles)
      {
        tPolys = new int[(w - 1) * (h - 1) * 6];
      }
      else
      {
        tPolys = new int[(w - 1) * (h - 1) * 4];
      }

      // Build vertices and UVs
      for (int y = 0; y < h; y++)
      {
        for (int x = 0; x < w; x++)
        {
          tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + terrainPos;
          tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
        }
      }

      int index = 0;
      if (saveFormat == SaveFormat.Triangles)
      {
        // Build triangle indices: 3 indices into vertex array for each triangle
        for (int y = 0; y < h - 1; y++)
        {
          for (int x = 0; x < w - 1; x++)
          {
            // For each grid cell output two triangles
            tPolys[index++] = (y * w) + x;
            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = (y * w) + x + 1;

            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = ((y + 1) * w) + x + 1;
            tPolys[index++] = (y * w) + x + 1;
          }
        }
      }
      else
      {
        // Build quad indices: 4 indices into vertex array for each quad
        for (int y = 0; y < h - 1; y++)
        {
          for (int x = 0; x < w - 1; x++)
          {
            // For each grid cell output one quad
            tPolys[index++] = (y * w) + x;
            tPolys[index++] = ((y + 1) * w) + x;
            tPolys[index++] = ((y + 1) * w) + x + 1;
            tPolys[index++] = (y * w) + x + 1;
          }
        }
      }

      // Export to .obj
      StreamWriter sw = new StreamWriter(fileName);
      try
      {

        sw.WriteLine("# Unity terrain OBJ File");

        // Write vertices
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        counter = tCount = 0;
        totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
        for (int i = 0; i < tVertices.Length; i++)
        {
          UpdateProgress();
          StringBuilder sb = new StringBuilder("v ", 20);
          // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
          // Which is important when you're exporting huge terrains.
          sb.Append(tVertices[i].x.ToString()).Append(" ").
             Append(tVertices[i].y.ToString()).Append(" ").
             Append(tVertices[i].z.ToString());
          sw.WriteLine(sb);
        }
        // Write UVs
        for (int i = 0; i < tUV.Length; i++)
        {
          UpdateProgress();
          StringBuilder sb = new StringBuilder("vt ", 22);
          sb.Append(tUV[i].y.ToString()).Append(" ").
             Append(tUV[i].x.ToString()); ///
          sw.WriteLine(sb);
        }
        if (saveFormat == SaveFormat.Triangles)
        {
          // Write triangles
          for (int i = 0; i < tPolys.Length; i += 3)
          {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("f ", 43);
            sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
               Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
               Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
            sw.WriteLine(sb);
          }
        }
        else
        {
          // Write quads
          for (int i = 0; i < tPolys.Length; i += 4)
          {
            UpdateProgress();
            StringBuilder sb = new StringBuilder("f ", 57);
            sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
               Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
               Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
               Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
            sw.WriteLine(sb);
          }
        }
      }
      catch (Exception err)
      {
        Debug.Log("Error saving file: " + err.Message);
      }
      sw.Close();

      terrain = null;
      EditorUtility.DisplayProgressBar("Saving file to disc.", "This might take a while...", 1f);
      EditorWindow.GetWindow<ExportOBJ>().Close();
      EditorUtility.ClearProgressBar();
    }

    static void UpdateProgress()
    {
      if (counter++ == progressUpdateInterval)
      {
        counter = 0;
       // EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
      }
    }
  }
}
