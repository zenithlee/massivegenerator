using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _Massive
{
  public class MassiveTerrain : EditorWindow
  {

    public enum ePATHSPEC
    {
      TEMPFILES, TERRAINBASE, TERRAINPATH, TERRAINFULLNAME, IMAGEEXPORT,
      BASEMAPURL, HEIGHTURL, SKINPATH, SATTELITEMAPURL, MAPZENURL, NORMALMAPURL,
      TEXTUREPATH, NORMALMAPPATH, HEIGHTMAPATH, MATERIALPATH, MESHPATH, PREFABPATH, ROADSPATH, WATERPATH, BUILDINGSPATH,
      ROADSASSETPATH, WATERASSETPATH, BUILDINGSASSETPATH
    };
    public const string TERRAINBASE = "Assets/_Massive/Terrains";

    [SerializeField]
    int ZoomLevel = 11;
    [SerializeField]
    int TileX = 1098;
    [SerializeField]
    int TileZ = 514;

    int RangeSize = 1;
    int RangeXS = 1098;
    int RangeZS = 514;
    int RangeXE = 1099;
    int RangeZE = 515;

    DVector3 StartPostion = new DVector3(1097, 0, 514);

    const int cellsize = 256;
    bool Stop = false;
    int Done = 0;
    int TileDone = 0;
    private string ExportLocation = @"I:\root\dev\UnityProjects\_Unity5\AssetBundles\Terrains\";

    GameObject TerrainContainer;
    public GameObject TerrainProto;
    Texture2D HeightMap;
    public GUISkin mySkin;
    float progress = 0;

    bool bCreateGrass = false;

    [SerializeField]
    Material RoadMaterial;
    [SerializeField]
    Material SmallStreetMaterial;
    [SerializeField]
    Material WaterMaterial;
    [SerializeField]
    Material BuildingMaterial;

    List<Vector3> Ranges = new List<Vector3>();

    private void OnEnable()
    {
      HeightMap = new Texture2D(cellsize, cellsize, TextureFormat.ARGB32, false);
      StartPostion = new DVector3(TileX, 0, TileZ);
    }

    [MenuItem("Massive/Terrain Generator")]
    public static void ShowWindow()
    {
      EditorWindow.GetWindow(typeof(MassiveTerrain));
    }

    #region GUI
    void OnGUI()
    {
      //GUI.skin = EditorGUIUtility.Load(GetPath(ePATHSPEC.SKINPATH)) as GUISkin;
      GUI.skin = mySkin;
      // OnSceneGUI();
      //Handles.Draw(rect, camera, CameraDrawMode.Textured)
      // OnDrawGizmos();
      ZoomLevel = EditorGUILayout.IntField("Zoom Level", ZoomLevel);

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("-"))
      {
        TileX--;
      }

      TileX = EditorGUILayout.IntField("TileX", TileX);

      if (GUILayout.Button("+"))
      {
        TileX++;
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("-"))
      {
        TileZ--;
      }
      TileZ = EditorGUILayout.IntField("TileZ", TileZ);
      if (GUILayout.Button("+"))
      {
        TileZ++;
      }

      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Set As Start Point") == true)
      {
        SetStartPoint();
      }
      GUILayout.EndHorizontal();

      GUILayout.Label("Creation", EditorStyles.boldLabel);


      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Auto Generate 1 Tile") == true)
      {
        EditorCoroutine.StartCoroutine(GenerateAll());
      }
      GUILayout.EndHorizontal();

      GUILayout.Label("Range", EditorStyles.boldLabel);

      GUILayout.BeginHorizontal();
      RangeSize = EditorGUILayout.IntField("Range", RangeSize);
      if (GUILayout.Button("Generate") == true)
      {
        EditorCoroutine.StartCoroutine(GenerateRange());
      }
      GUILayout.EndHorizontal();
      GUILayout.Label((TileX - RangeSize) + "," + (TileZ - RangeSize) + "-" + (TileZ + RangeSize) + "," + (TileZ + RangeSize), EditorStyles.boldLabel);

      GUILayout.BeginHorizontal();
      bCreateGrass = EditorGUILayout.Toggle("Create Grass", bCreateGrass);
      GUILayout.EndHorizontal();


      GUILayout.Label("Components", EditorStyles.boldLabel);
      GUILayout.Label("Download", EditorStyles.boldLabel);
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Basemap") == true)
      {
        DownloadBasemap();
      }
      // GUILayout.Box(HeightMap, GUILayout.Width(64), GUILayout.Height(64));
      //EditorGUI.DrawPreviewTexture(new Rect(25, 60, 100, 100), HeightMap);

      if (GUILayout.Button("Heightmap") == true)
      {
        DownloadHeightMap();
      }
      if (GUILayout.Button("NormalMap") == true)
      {
        DownloadNormalMap();
      }
      GUILayout.EndHorizontal();



      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Generate Mesh Terrain") == true)
      {
        GenerateMeshTerrain();
      }
      GUILayout.EndHorizontal();

      GUILayout.Label("Generate", EditorStyles.boldLabel);

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Roads") == true)
      {
        DownloadRoads();
      }
      if (GUILayout.Button("Water") == true)
      {
        DownloadWater();
      }
      if (GUILayout.Button("Buildings") == true)
      {
        DownloadBuildings();        
      }
      GUILayout.EndHorizontal();
      

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Fix Terrain Mesh") == true)
      {
        FixMesh();
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Generate Prefab") == true)
      {
        GeneratePrefab();
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Export Assetbundle") == true)
      {
        ExportAssetBundle();
      }
      GUILayout.EndHorizontal();

      GUILayout.Label("Bundle Export Location");
      ExportLocation = GUILayout.TextField(ExportLocation);


      RoadMaterial = EditorGUILayout.ObjectField("Road Material", RoadMaterial, typeof(Material)) as Material;
      SmallStreetMaterial = EditorGUILayout.ObjectField("Small Road Material", SmallStreetMaterial, typeof(Material)) as Material;
      WaterMaterial = EditorGUILayout.ObjectField("Water Material", WaterMaterial, typeof(Material)) as Material;
      BuildingMaterial = EditorGUILayout.ObjectField("BuildingMaterial", BuildingMaterial, typeof(Material)) as Material;

      if (GUILayout.Button("STOP"))
      {
        Stop = true;
      }

      GUILayout.BeginHorizontal();
      GUILayout.Label("Progress", EditorStyles.boldLabel);
      EditorGUILayout.FloatField(progress);
      Done = EditorGUILayout.IntField(Done);
      GUILayout.EndHorizontal();
    }

    #endregion

    void SetStartPoint()
    {
      StartPostion = new DVector3(TileX, 0, TileZ);
    }

    public string GetPath(ePATHSPEC ps)
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
        case ePATHSPEC.TEXTUREPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/texture.png";
        case ePATHSPEC.NORMALMAPPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/normals.png";
        case ePATHSPEC.HEIGHTMAPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/height.png";
        case ePATHSPEC.MESHPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/mesh.obj";
        case ePATHSPEC.MATERIALPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/material.mat";
        case ePATHSPEC.PREFABPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/prefab.prefab";
        case ePATHSPEC.ROADSPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/roads.json";
        case ePATHSPEC.WATERPATH:
          return GetPath(ePATHSPEC.TERRAINPATH) + "/water.json";
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

    void DownloadBuildings()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadFeaturesCo("buildings", GetPath(ePATHSPEC.BUILDINGSPATH), GenerateBuildings));
    }

    void GenerateBuildings()
    {
      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();      
      string data = File.ReadAllText(GetPath(ePATHSPEC.BUILDINGSPATH));
      //t.GenerateRoadsJSON(data);
      GameObject go = new GameObject("buildings");
      MassiveBuilding b = go.AddComponent<MassiveBuilding>();
      b.SetMaterial(BuildingMaterial);
      go.transform.parent = TerrainContainer.transform;
      go.transform.localPosition = Vector3.zero;
      b.Generate(data, go, t.PositionMeters, GetPath(ePATHSPEC.BUILDINGSASSETPATH));
      DestroyImmediate(b);

      AssetTools.WriteMeshAssets(go, GetPath(ePATHSPEC.ROADSASSETPATH));
    }

    void DownloadRoads()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadFeaturesCo("roads", GetPath(ePATHSPEC.ROADSPATH), GenerateRoads));
    }

    void GenerateRoads()
    {      
      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();      
      string data = File.ReadAllText(GetPath(ePATHSPEC.ROADSPATH));
      //t.GenerateRoadsJSON(data);
      GameObject go = new GameObject("roads");
      MassiveRoad r = go.AddComponent<MassiveRoad>();
      r.SetMaterial(RoadMaterial);
      go.transform.parent = TerrainContainer.transform;
      go.transform.localPosition = Vector3.zero;
      r.Generate(data, t.PositionMeters, GetPath(ePATHSPEC.ROADSASSETPATH));
      DestroyImmediate(r);

      AssetTools.WriteMeshAssets(go, GetPath(ePATHSPEC.ROADSASSETPATH));
      
    }

    void DownloadWater()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadFeaturesCo("water", GetPath(ePATHSPEC.WATERPATH), GenerateWater));
    }

    void GenerateWater()
    {
      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();
      t.WaterMaterial = this.WaterMaterial;
      
      if ( File.Exists( GetPath(ePATHSPEC.WATERPATH))) { 
        string data = File.ReadAllText(GetPath(ePATHSPEC.WATERPATH));
        GameObject go = new GameObject();
        go.transform.parent = t.transform;        
        go.name = "water";
        MassiveWater w = go.AddComponent<MassiveWater>();
        w.OceanMaterial = WaterMaterial;
        //Ocean o = go.AddComponent<Ocean>();
        //o.m_oceanMat = WaterMaterial;
        //o.MainCamera = Camera.main;
        w.GenerateWaterJSON(data, go, t.PositionMeters);
        go.transform.localPosition = Vector3.zero;
        DestroyImmediate(w);

        AssetTools.WriteMeshAssets(go, GetPath(ePATHSPEC.WATERASSETPATH));
      }      
    }

    IEnumerator DownloadFeaturesCo(string feature, string outpath, Action next = null)
    {
      string MKey = KeyProvider.GetMapZenKey();
      string FullURL = GetPath(ePATHSPEC.MAPZENURL);
      var tileurl = TileX + "/" + TileZ;
      var baseUrl = GetPath(ePATHSPEC.MAPZENURL);
      var url = baseUrl + feature + "/" + ZoomLevel + "/";
      FullURL = url + tileurl + ".json?api_key=" + MKey;
      Debug.Log(FullURL);
      string data = "";

      progress = 0;
      if (File.Exists(outpath))
      {
        data = File.ReadAllText(outpath);
      }
      else
      {
        WWW www = new WWW(FullURL);
        while (!www.isDone)
        {
          progress = www.progress;
          this.Repaint();
          yield return new WaitForSeconds(0.01f);
        }
        progress = 1;
        this.Repaint();
        //yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
          data = www.text;
          File.WriteAllText(outpath, data);
          AssetDatabase.Refresh();
        }
        else
        {
          Debug.LogError(www.error);
        }
      }
      Done--;
      if (next != null)
      {
        next();
      }

    }

    void FixMesh()
    {
      Done++;
      TerrainFix fix = new TerrainFix();
      fix.Fix(TerrainContainer);
      Done--;
    }

    void DownloadBasemap()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadBasemapCo());
    }


    IEnumerator DownloadBasemapCo()
    {

      Debug.Log("Downloading Base Map");
      int pixx = 0;
      int pixy = 0;

      int Chop = 0; 

      string FullURL = GetPath(ePATHSPEC.SATTELITEMAPURL);
      
      FullURL = FullURL.Replace("{mapboxkey}", KeyProvider.GetMapBoxKey());
      FullURL = FullURL.Replace("{x}", TileX.ToString());
      FullURL = FullURL.Replace("{z}", TileZ.ToString());
      FullURL = FullURL.Replace("{zoom}", ZoomLevel.ToString());

      
      Texture2D Temp;

      if ( File.Exists(GetPath(ePATHSPEC.TEXTUREPATH) ))
      {
        Debug.Log("Cache hit:" + GetPath(ePATHSPEC.TEXTUREPATH));
        //Temp = AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.TEXTUREPATH));
      }
      else
      {
        Debug.Log(FullURL);
        Temp = new Texture2D((int)(cellsize), (int)(cellsize));
        WWW wwwTex = new WWW(FullURL);
        yield return wwwTex;

        if (!string.IsNullOrEmpty(wwwTex.error))
        {
          //URL.text = ">>>>IMAGE>>" + wwwTex.error;
          Debug.Log("Error downloading from " + FullURL);
          Done--;
          yield break;
        }
        wwwTex.LoadImageIntoTexture(Temp);
        File.WriteAllBytes(GetPath(ePATHSPEC.TERRAINPATH) + "/texture.png", Temp.EncodeToPNG());
        DestroyImmediate(Temp);
        AssetDatabase.Refresh();
        Debug.Log("Downloaded Base Map");
      }    
      
      Done--;
    }

    void DownloadHeightMap()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadHeightMapCo());
    }

    IEnumerator DownloadHeightMapCo()
    {
      Debug.Log("Downloading Height Map");

      if (File.Exists(GetPath(ePATHSPEC.HEIGHTMAPATH)))
      {
        Debug.Log("Cache hit:" + GetPath(ePATHSPEC.HEIGHTMAPATH));
        //Temp = AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.TEXTUREPATH));
      }
      else
      {
        string FullURL = GetPath(ePATHSPEC.HEIGHTURL);

        FullURL = FullURL.Replace("{mapboxkey}", KeyProvider.GetMapBoxKey());
        FullURL = FullURL.Replace("{x}", TileX.ToString());
        FullURL = FullURL.Replace("{z}", TileZ.ToString());
        FullURL = FullURL.Replace("{zoom}", ZoomLevel.ToString());
        WWW wwwTex = new WWW(FullURL);
        while (!wwwTex.isDone)
        {
          progress = wwwTex.progress;
          Repaint();
          yield return new WaitForSeconds(0.1f);
        }


        if (!string.IsNullOrEmpty(wwwTex.error))
        {
          Debug.Log("Error downloading from " + FullURL);
          yield break;
        }

        Texture2D Temp = new Texture2D((int)(cellsize), (int)(cellsize));
        wwwTex.LoadImageIntoTexture(Temp);
        //Color[] pixels = new Color[Temp.width * Temp.height];
        //pixels = Temp.GetPixels(0, 0, Temp.width, Temp.height);

        string path = GetPath(ePATHSPEC.HEIGHTMAPATH);
        File.WriteAllBytes(path, Temp.EncodeToPNG());

        AssetDatabase.Refresh();
        SwapImageForAsset(Temp, path);
        Debug.Log("Downloaded Height Map");
      }
      
      Done--;
    }

    void DownloadNormalMap()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadNormalMapCo());
    }

    IEnumerator DownloadNormalMapCo()
    {

      Debug.Log("Downloading Normap Map");

      if (File.Exists(GetPath(ePATHSPEC.NORMALMAPPATH)))
      {
        Debug.Log("Cache hit:" + GetPath(ePATHSPEC.NORMALMAPPATH));
        //Temp = AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.TEXTUREPATH));
      }
      else
      {
        string FullURL = GetPath(ePATHSPEC.NORMALMAPURL);

        FullURL = FullURL.Replace("{mapzenkey}", KeyProvider.GetMapZenKey());
        FullURL = FullURL.Replace("{x}", TileX.ToString());
        FullURL = FullURL.Replace("{z}", TileZ.ToString());
        FullURL = FullURL.Replace("{zoom}", ZoomLevel.ToString());

        WWW wwwTex = new WWW(FullURL);
        yield return wwwTex;

        if (!string.IsNullOrEmpty(wwwTex.error))
        {
          Debug.Log("Error downloading from " + FullURL);
          Done--;
          yield break;
        }

        Texture2D Temp = new Texture2D((int)(cellsize), (int)(cellsize));
        wwwTex.LoadImageIntoTexture(Temp);

        string path = GetPath(ePATHSPEC.NORMALMAPPATH);
        File.WriteAllBytes(path, Temp.EncodeToPNG());
        DestroyImmediate(Temp);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.textureType = TextureImporterType.NormalMap;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        //      importer.isReadable = true;
        AssetDatabase.WriteImportSettingsIfDirty(path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Downloaded Normap Map");
      }
           
      Done--;
    }

    void CreateFolders()
    {
      if (!Directory.Exists(TERRAINBASE))
      {
        Directory.CreateDirectory(TERRAINBASE);
      }
      if (!Directory.Exists(GetPath(ePATHSPEC.TERRAINPATH)))
      {
        Directory.CreateDirectory(GetPath(ePATHSPEC.TERRAINPATH));
      }
    }

    void GetContainers()
    {
      TerrainContainer = GameObject.Find("Terrain");
      TerrainProto = GameObject.Find("TerrainProto");
    }

    IEnumerator GenerateRange()
    {

      int itx = TileX;

      for ( int xx= TileX-RangeSize; xx < TileX + RangeSize; xx++)
      {
        TileDone = 0;
        TileDone++;
        EditorCoroutine.StartCoroutine(GenerateAll());
        while (TileDone > 0)
        {
          // Debug.Log(TileDone);
          this.Repaint();
          yield return new WaitForSeconds(5f);
        }
        TileX++;
      }

      TileX = itx;


    }


    //Generate a mesh terrain, prefab, and AssetBundle
    IEnumerator GenerateAll()
    {
     
      Done = 0;
      CreateFolders();
      GetContainers();

      DownloadBasemap();
      DownloadHeightMap();
      DownloadNormalMap();

      while (Done > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }

      Done = 0;
      GenerateMeshTerrain();
      while (Done > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }

      Done = 0;

      DownloadWater();
      DownloadRoads();
      DownloadBuildings();

      while (Done > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }

      Done = 0;

      FixMesh();
      while (Done > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }      

      GeneratePrefab();
     
      ExportAssetBundle();
     
      TileDone--;
    }

    void GeneratePrefab()
    {
      Debug.Log("Generating Prefab");
      string ShortName = TileX + "_" + TileZ;


      UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(GetPath(ePATHSPEC.PREFABPATH));
      PrefabUtility.ReplacePrefab(TerrainContainer, prefab, ReplacePrefabOptions.ConnectToPrefab);

      //string path = AssetDatabase.GetAssetPath(prefab);
      //AssetImporter assetImporter = AssetImporter.GetAtPath(path);
      //assetImporter.assetBundleName = ShortName + ".unity3d";
      Debug.Log("Generated Prefab");
    }

    void ExportAssetBundle()
    {
      Done++;
      Debug.Log("Exporting AssetBundle " + TileX + "_" + TileZ);
      //string path = "Assets/_Massive/AssetBundles";     
      string path = ExportLocation + "\\" + ZoomLevel + "\\" + TileZ + "\\";
      Directory.CreateDirectory(path);


      string srcimgpath = GetPath(ePATHSPEC.TERRAINPATH) + "/texture.png";
      string targetimgpath = path + TileX + "_" + TileZ + ".png";

      srcimgpath = srcimgpath.Replace("/", "\\");
      if (File.Exists(targetimgpath))
      {
        File.Delete(targetimgpath);
      }
      File.Copy(srcimgpath, targetimgpath);

      UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetPath(ePATHSPEC.PREFABPATH));
      string assetpath = AssetDatabase.GetAssetPath(prefab);
      AssetImporter assetImporter = AssetImporter.GetAtPath(assetpath);
      assetImporter.assetBundleName = TileX + "_" + TileZ + ".unity3d";

      BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

      assetImporter.assetBundleName = "";


      //UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetPath(ePATHSPEC.PREFABPATH));
      //BuildPipeline.BuildAssetBundle(prefab, null, path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

      //AssetBundleBuild[] build = new AssetBundleBuild[1];
      //build[0] = new AssetBundleBuild();
      //build[0].assetBundleName = TileX+ "_" + TileZ + ".unity3d";
      //build[0].assetNames = new string[1] { GetPath(ePATHSPEC.PREFABPATH) };
      //BuildPipeline.BuildAssetBundles("Assets/", build, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

      Debug.Log("Exported AssetBundle " + TileX + "_" + TileZ);
      Done--;
    }

    void GenerateMeshTerrain()
    {
      Done++;
      Debug.Log("Generating Mesh Terrain");
      AssetDatabase.Refresh();
      GetContainers();
      CreateFolders();

      Material m = new Material(Shader.Find("Standard"));
      m.EnableKeyword("_BUMPMAP");
      //_DETAIL_MULX2
      m.color = Color.white;
      m.SetFloat("_SpecularHighlights", 0f);
      m.SetFloat("_SpecularHighlights", 0f);

      Texture2D t2 = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.TEXTUREPATH));
      m.mainTexture = t2;
      Texture2D n1 = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.NORMALMAPPATH));
      m.SetTexture("_BumpMap", n1);
      m.SetFloat("_Glossiness", 0);

      AssetDatabase.CreateAsset(m, GetPath(ePATHSPEC.MATERIALPATH));
      AssetDatabase.Refresh();

      Debug.Log("Generate Mesh Terrain");

      //TerrainSettings tset = TerrainProto.GetComponent<TerrainSettings>();

      string hpath = GetPath(ePATHSPEC.HEIGHTMAPATH);
      HeightMap = Instantiate(AssetDatabase.LoadAssetAtPath<Texture2D>(hpath));

      string outpath = GetPath(ePATHSPEC.MESHPATH);
      DRect TileSize = MapFuncs.TileBoundsInMeters(new DVector3(TileX, 0, TileZ), ZoomLevel);
      //ExportOBJ.ExportFromHeight(HeightMap, outpath, TileSize);      
      //ExportOBJ.ExportMapZenHeight(HeightMap, outpath, TileSize.Size);      
      DVector3 TileLatLon = MapFuncs.TileToWorldPos(new DVector3(TileX, 0, TileZ), ZoomLevel);

      TerrainContainer = new GameObject();
      MassiveTile tile = TerrainContainer.AddComponent<MassiveTile>();
      ExportOBJ.ExportMapZenHeight(tile, HeightMap, outpath, TileSize.Size, TileLatLon);

      AssetDatabase.Refresh();
      AssetDatabase.ImportAsset(GetPath(ePATHSPEC.MESHPATH));
      GameObject asset = (GameObject)AssetDatabase.LoadMainAssetAtPath(GetPath(ePATHSPEC.MESHPATH));
      GameObject obj = Instantiate(asset);
      obj.transform.Find("default").transform.parent = TerrainContainer.transform;
      GameObject.DestroyImmediate(obj);

      //set position
      Vector3 tm = MapFuncs.TileToMeters(TileX, TileZ, ZoomLevel).ToVector3();
      Vector3 sp = MapFuncs.TileToMeters((int)StartPostion.x, (int)StartPostion.z, ZoomLevel).ToVector3();
      DVector3 dif = new DVector3(TileX, 0, TileZ) - StartPostion;
      TerrainContainer.transform.position = tm - sp;

      TerrainContainer.name = TileX + "_" + "0" + "_" + TileZ + "_" + ZoomLevel;
      TerrainContainer.transform.Find("default").name = "terrain";

      GameObject TerGo = TerrainContainer.transform.Find("terrain").gameObject;
      MeshRenderer mr = TerGo.GetComponent<MeshRenderer>();
      TerGo.AddComponent<MeshCollider>();
      TerGo.layer = LayerMask.NameToLayer("Terrain");


      tile.ZoomLevel = ZoomLevel;
      tile.SetTileIndex(new Vector3(TileX, 0, TileZ), ZoomLevel);

      Material[] temp = new Material[1];
      //temp[0] = Instantiate((Material)AssetDatabase.LoadAssetAtPath<Material>(path + ".mat"));            
      temp[0] = (Material)AssetDatabase.LoadAssetAtPath<Material>(GetPath(ePATHSPEC.MATERIALPATH));
      temp[0].name = "mat_" + TileX + "_" + TileZ;
      //TextureTools.Bilinear(t2, 512, 512);
      temp[0].mainTexture = t2;
      //temp[1] = Instantiate((Material)AssetDatabase.LoadAssetAtPath<Material>(path + ".mat"));
      //temp[1].name = "hello";
      mr.sharedMaterials = temp;
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      Debug.Log("Generated Mesh Terrain");
      Done--;
    }

    Texture2D SwapImageForAsset(Texture2D tex, string path)
    {
      tex.wrapMode = TextureWrapMode.Repeat;

      AssetDatabase.Refresh();
      AssetDatabase.ImportAsset(path);
      TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
      importer.textureType = TextureImporterType.Default;
      importer.wrapMode = TextureWrapMode.Repeat;
      importer.textureCompression = TextureImporterCompression.Uncompressed;
      importer.isReadable = true;
      AssetDatabase.WriteImportSettingsIfDirty(path);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      Texture2D tex2 = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
      return tex2;
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