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
    [SerializeField]
    int ZoomLevel = 14;
    [SerializeField]
    int TileX = 1098;
    [SerializeField]
    int TileZ = 514;

    int RangeSize = 1;
    int RangeXS = 1098;
    int RangeZS = 514;
    int RangeXE = 1099;
    int RangeZE = 515;

    [SerializeField]
    DVector3 StartPostion = new DVector3(1097, 0, 514);

    const int cellsize = 256;
    bool Stop = false;
    int Done = 0;
    int NeighbourDone = 0;
    int HeightmapDone = 0;
    int TileDone = 0;
    private string ExportLocation = @"I:\root\dev\UnityProjects\_Unity5\AssetBundles\Terrains\";

    GameObject TerrainContainer;
    public GameObject TerrainProto;
    Texture2D HeightMap;
    public GUISkin mySkin;
    float progress = 0;

    [SerializeField]
    bool bCreateGrass = false;
    [SerializeField]
    bool bExport = false;

    [SerializeField]
    Material RoadMaterial;
    [SerializeField]
    Material SmallStreetMaterial;
    [SerializeField]
    Material MajorRoadMaterial;
    [SerializeField]
    Material PathMaterial;
    [SerializeField]
    Material ServiceRoadMaterial;
    [SerializeField]
    Material RailwayMaterial;
    [SerializeField]
    Material WaterMaterial;
    [SerializeField]
    Material BuildingMaterial;
    [SerializeField]
    Material RoofMaterial;

    //List<Vector3> Ranges = new List<Vector3>();

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

    void Setup()
    {
      Paths.TileX = TileX;
      Paths.TileZ = TileZ;
      Paths.ZoomLevel = ZoomLevel;
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
      bExport = EditorGUILayout.Toggle("Export Bundle", bExport);
      GUILayout.EndHorizontal();

      GUILayout.Label("Range", EditorStyles.boldLabel);

      GUILayout.BeginHorizontal();
      RangeSize = EditorGUILayout.IntField("Range", RangeSize);
      if (GUILayout.Button("Generate") == true)
      {
        EditorCoroutine.StartCoroutine(GenerateRange());
      }
      GUILayout.EndHorizontal();
      if (GUILayout.Button("Download Neighbouring Heightmaps") == true)
      {
        DownloadHeightmapNeighbours();
      }
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
      if (GUILayout.Button("Create Continuity") == true)
      {
        CreateContiuousTiles();
      }

      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Generate Mesh Terrain") == true)
      {
        GenerateMeshTerrain();
      }
      GUILayout.EndHorizontal();

      GUILayout.Label("Generate", EditorStyles.boldLabel);

      if ( GUILayout.Button("Get OSM Tile") == true )
      {
        DownloadOSM();
      }

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
      if (GUILayout.Button("Road Network") == true)
      {
        GenerateRoadNetwork();
      }
      if (GUILayout.Button("Road Network Test") == true)
      {
        GenerateRoadNetworkTest();
      }
      if (GUILayout.Button("Polygon Test") == true)
      {
        PolygonTest();
      }
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Get Geo Info") == true)
      {
        GetGeoInfo();
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

      MajorRoadMaterial = EditorGUILayout.ObjectField("Major Road Material", MajorRoadMaterial, typeof(Material)) as Material;
      RoadMaterial = EditorGUILayout.ObjectField("Road Material", RoadMaterial, typeof(Material)) as Material;
      SmallStreetMaterial = EditorGUILayout.ObjectField("Small Road Material", SmallStreetMaterial, typeof(Material)) as Material;
      PathMaterial = EditorGUILayout.ObjectField("Path Material", PathMaterial, typeof(Material)) as Material;
      RailwayMaterial = EditorGUILayout.ObjectField("Railway Material", RailwayMaterial, typeof(Material)) as Material;
      WaterMaterial = EditorGUILayout.ObjectField("Water Material", WaterMaterial, typeof(Material)) as Material;
      BuildingMaterial = EditorGUILayout.ObjectField("Building Material", BuildingMaterial, typeof(Material)) as Material;
      RoofMaterial = EditorGUILayout.ObjectField("Roof Material", RoofMaterial, typeof(Material)) as Material;
      ServiceRoadMaterial = EditorGUILayout.ObjectField("Service Road Material", ServiceRoadMaterial, typeof(Material)) as Material;

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

    void DownloadOSM()
    {
      Done++;
      EditorCoroutine.StartCoroutine(DownloadOSMCo());
    }

    IEnumerator DownloadOSMCo()
    {
      CreateFolders();

      string url = Paths.GetPath(ePATHSPEC.OSMURL);
      //DRect r = MapTools.TileLatLonBounds(new DVector3(TileX, 0, TileZ), ZoomLevel);
      DRect r = MapTools.TileIdToBounds(TileX, TileZ, ZoomLevel);
      url = url.Replace("{x2}", r.Min.z.ToString());
      url = url.Replace("{y2}", r.Min.x.ToString());
      url = url.Replace("{x1}", r.Max.z.ToString());
      url = url.Replace("{y1}", r.Max.x.ToString());

      WWW www = new WWW(url);
      progress = 0;
      Debug.Log(url);
      while (!www.isDone)
      {
        //progress = www.progress;
        progress++;
        this.Repaint();
        yield return new WaitForSeconds(0.01f);
      }
      progress = 0;
      File.WriteAllText(Paths.GetPath(ePATHSPEC.OSMPath), www.text);
      AssetDatabase.Refresh();
      Done--;
    }

    void DownloadBuildings()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadFeaturesCo("buildings", Paths.GetPath(ePATHSPEC.BUILDINGSPATH), GenerateBuildings));
    }

    void GenerateBuildings()
    {
      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();
      string data = File.ReadAllText(Paths.GetPath(ePATHSPEC.BUILDINGSPATH));
      //t.GenerateRoadsJSON(data);
      Transform o = TerrainContainer.transform.Find("buildings");
      if (o != null) GameObject.DestroyImmediate(o.gameObject);
      GameObject go = new GameObject("buildings");
      MassiveBuilding b = go.AddComponent<MassiveBuilding>();
      b.SetMaterial(BuildingMaterial, RoofMaterial);
      go.transform.parent = TerrainContainer.transform; 
      go.transform.localPosition = Vector3.zero;
      b.Generate(data, go, t.PositionMeters);
      //DestroyImmediate(b);

      AssetTools.WriteMeshAssets(go, Paths.GetPath(ePATHSPEC.BUILDINGSASSETPATH));
    }

    void DownloadRoads()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadFeaturesCo("roads", Paths.GetPath(ePATHSPEC.ROADSPATH), GenerateRoads));
    }

    void GenerateRoads()
    {
      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();
      string data = File.ReadAllText(Paths.GetPath(ePATHSPEC.ROADSPATH));
      //t.GenerateRoadsJSON(data);
      GameObject go = new GameObject("roads");
      MassiveRoad r = go.AddComponent<MassiveRoad>();
      r.SetMaterial(MajorRoadMaterial, RoadMaterial, PathMaterial, RailwayMaterial);
      go.transform.parent = TerrainContainer.transform;
      go.transform.localPosition = Vector3.zero;
      r.Generate(data, t.PositionMeters, Paths.GetPath(ePATHSPEC.ROADSASSETPATH));
      DestroyImmediate(r);

      AssetTools.WriteMeshAssets(go, Paths.GetPath(ePATHSPEC.ROADSASSETPATH));
    }

    void GenerateRoadNetwork()
    {
      CreateFolders();

      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();
      string data = File.ReadAllText(Paths.GetPath(ePATHSPEC.ROADSPATH));
      Transform rn = TerrainContainer.transform.Find("roadnetwork");
      if (rn != null)
      {
        GameObject.DestroyImmediate(rn.gameObject);
      }
      GameObject go = new GameObject("roadnetwork");
      go.transform.parent = TerrainContainer.transform;
      go.transform.localPosition = Vector3.zero;
      go.layer = LayerMask.NameToLayer("Roads");
      MassiveRoadNetwork mr = go.AddComponent<MassiveRoadNetwork>();
      
      mr.SetMaterials(RoadMaterial, RailwayMaterial, PathMaterial, ServiceRoadMaterial);
      mr.CreateRoads(data, t.PositionMeters);
      AssetTools.WriteMeshAssets(go, Paths.GetPath(ePATHSPEC.ROADSASSETPATH));
      //mr.CreateTest();
    }

    void GenerateRoadNetworkTest()
    {
      CreateFolders();

      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();
      string data = File.ReadAllText(Paths.GetPath(ePATHSPEC.ROADSPATH));
      Transform rn = TerrainContainer.transform.Find("roadnetwork");
      if (rn != null)
      {
        GameObject.DestroyImmediate(rn.gameObject);
      }
      GameObject go = new GameObject("roadnetwork");
      go.transform.parent = TerrainContainer.transform;
      go.transform.localPosition = Vector3.zero;
      go.layer = LayerMask.NameToLayer("Roads");
      MassiveRoadNetwork mr = go.AddComponent<MassiveRoadNetwork>();
      mr.SetMaterials(RoadMaterial, RailwayMaterial, PathMaterial, ServiceRoadMaterial);
      mr.CreateTest();
    }

    void PolygonTest()
    {
      GameObject go = GameObject.Find("polytest");
      GameObject.DestroyImmediate(go);
      
        go = new GameObject("polytest");
        go.AddComponent<PolyTest>().Test();

      
    }

    void DownloadWater()
    {
      Done++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadFeaturesCo("water", Paths.GetPath(ePATHSPEC.WATERPATH), GenerateWater));
    }

    void GenerateWater()
    {
      MassiveTile t = TerrainContainer.GetComponent<MassiveTile>();
      t.WaterMaterial = this.WaterMaterial;

      if (File.Exists(Paths.GetPath(ePATHSPEC.WATERPATH)))
      {
        string data = File.ReadAllText(Paths.GetPath(ePATHSPEC.WATERPATH));
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

        AssetTools.WriteMeshAssets(go, Paths.GetPath(ePATHSPEC.WATERASSETPATH));
      }
    }

    IEnumerator DownloadFeaturesCo(string feature, string outpath, Action next = null)
    {
      string MKey = KeyProvider.GetMapZenKey();
      string FullURL = Paths.GetPath(ePATHSPEC.MAPZENURL);
      var tileurl = TileX + "/" + TileZ;
      var baseUrl = Paths.GetPath(ePATHSPEC.MAPZENURL);
      var url = baseUrl + feature + "/" + ZoomLevel + "/";
      FullURL = url + tileurl + ".json?api_key=" + MKey;

      string data = "";

      progress = 0;
      if (File.Exists(outpath))
      {
        Debug.Log("Cache Hit: " + outpath);
        data = File.ReadAllText(outpath);
      }
      else
      {
        Debug.Log(FullURL);
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

    //TODO get wiki info http://api.geonames.org/findNearbyWikipedia?lat=47&lng=9&username=demo 
    void GetGeoInfo()
    {
      Done++;
      EditorCoroutine.StartCoroutine(GetGeoInfoCo());
    }

    IEnumerator GetGeoInfoCo()
    {
      CreateFolders();
      string FullURL = Paths.GetPath(ePATHSPEC.GEOINFOURL);      
      DRect worldpos = MapTools.TileIdToBounds(TileX, TileZ, ZoomLevel);
      FullURL = FullURL.Replace("{e}", worldpos.Min.z.ToString());
      FullURL = FullURL.Replace("{n}", worldpos.Min.x.ToString());

      FullURL = FullURL.Replace("{w}", worldpos.Max.z.ToString());
      FullURL = FullURL.Replace("{s}", worldpos.Max.x.ToString());
      string outpath = Paths.GetPath(ePATHSPEC.GEOINFOPATH);

      string data = "";
      if (File.Exists(outpath))
      {
        Debug.Log("Cache Hit: " + outpath);
        data = File.ReadAllText(outpath);
      }
      else
      {
        Debug.Log(FullURL);
        WWW www = new WWW(FullURL);

        while (!www.isDone)
        {
          progress = www.progress;
          this.Repaint();
          yield return new WaitForSeconds(0.01f);
        }
        progress = 1;
        this.Repaint();

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
    }

    void CreateContiuousTiles()
    {
      CreateFolders();
      if (File.Exists(Paths.GetPath(ePATHSPEC.HEIGHTMAPCONTPATH)))
      {
        Debug.Log("Cache hit: Continuity map found for : " + Paths.GetPath(ePATHSPEC.HEIGHTMAPCONTPATH));
        return;
      }

      TerrainFix fix = new TerrainFix();
      fix.CreateHeightmapContinuity(TileX, TileZ);
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

      //int Chop = 0;

      string FullURL = Paths.GetPath(ePATHSPEC.SATTELITEMAPURL);

      FullURL = FullURL.Replace("{mapboxkey}", KeyProvider.GetMapBoxKey());
      FullURL = FullURL.Replace("{x}", TileX.ToString());
      FullURL = FullURL.Replace("{z}", TileZ.ToString());
      FullURL = FullURL.Replace("{zoom}", ZoomLevel.ToString());

      Texture2D Temp;

      if (File.Exists(Paths.GetPath(ePATHSPEC.TEXTUREPATH)))
      {
        Debug.Log("Cache hit:" + Paths.GetPath(ePATHSPEC.TEXTUREPATH));
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
        File.WriteAllBytes(Paths.GetPath(ePATHSPEC.TERRAINPATH) + "/texture.png", Temp.EncodeToPNG());
        DestroyImmediate(Temp);
        AssetDatabase.Refresh();
        Debug.Log("Downloaded Base Map");
      }

      Done--;
    }

    void DownloadHeightmapNeighbours()
    {
      NeighbourDone = 1;
      CreateFolders();
      Done = 1;
      HeightmapDone = 1;
      EditorCoroutine.StartCoroutine(DownloadHeightMapNeighboursCo());
    }

    IEnumerator DownloadHeightMapNeighboursCo()
    {
      HeightmapDone = 1;

      int itx = TileX;
      int itz = TileZ;

      Setup();
      HeightmapDone = 1;
      Debug.Log("Getting Root Heightmap " + TileX + "," + TileZ);
      EditorCoroutine.StartCoroutine(DownloadHeightMapCo());
      while (HeightmapDone > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }

      TileX = itx + 1;
      Setup();
      HeightmapDone = 1;
      Debug.Log("Getting Heightmap Neighbour " + TileX + "," + TileZ);
      EditorCoroutine.StartCoroutine(DownloadHeightMapCo());
      while (HeightmapDone > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }
      Debug.Log("Done " + TileX + "," + TileZ);

      TileX = itx + 1;
      TileZ = itz + 1;
      Setup();
      Debug.Log("Getting Heighmap Neighbour " + TileX + "," + TileZ);
      HeightmapDone = 1;
      EditorCoroutine.StartCoroutine(DownloadHeightMapCo());
      while (HeightmapDone > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }
      Debug.Log("Done " + TileX + "," + TileZ);

      TileX = itx;
      TileZ = itz + 1;
      Setup();
      Debug.Log("Getting Heightmap Neighbour " + TileX + "," + TileZ);
      HeightmapDone = 1;
      EditorCoroutine.StartCoroutine(DownloadHeightMapCo());
      while (HeightmapDone > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }
      Debug.Log("Done heightmap " + TileX + "," + TileZ);

      TileX = itx;
      TileZ = itz;
      Setup();
      NeighbourDone = 0;
    }

    void DownloadHeightMap()
    {
      HeightmapDone++;
      CreateFolders();
      EditorCoroutine.StartCoroutine(DownloadHeightMapCo());
    }

    IEnumerator DownloadHeightMapCo()
    {
      Debug.Log("Downloading Height Map");

      if (File.Exists(Paths.GetPath(ePATHSPEC.HEIGHTMAPATH)))
      {
        Debug.Log("Cache hit:" + Paths.GetPath(ePATHSPEC.HEIGHTMAPATH));
        //Temp = AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.TEXTUREPATH));
        yield return new WaitForSeconds(0.1f);
      }
      else
      {
        string FullURL = Paths.GetPath(ePATHSPEC.HEIGHTURL);

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

        string path = Paths.GetPath(ePATHSPEC.HEIGHTMAPATH);
        File.WriteAllBytes(path, Temp.EncodeToPNG());

        //AssetDatabase.Refresh();
        SwapImageForAsset(Temp, path);
        Debug.Log("Downloaded Height Map");
      }

      HeightmapDone--;
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

      if (File.Exists(Paths.GetPath(ePATHSPEC.NORMALMAPPATH)))
      {
        Debug.Log("Cache hit:" + Paths.GetPath(ePATHSPEC.NORMALMAPPATH));
        //Temp = AssetDatabase.LoadAssetAtPath<Texture2D>(GetPath(ePATHSPEC.TEXTUREPATH));
      }
      else
      {
        string FullURL = Paths.GetPath(ePATHSPEC.NORMALMAPURL);

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

        string path = Paths.GetPath(ePATHSPEC.NORMALMAPPATH);
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
      Setup();
      if (!Directory.Exists(Paths.TERRAINBASE))
      {
        Directory.CreateDirectory(Paths.TERRAINBASE);
      }
      if (!Directory.Exists(Paths.GetPath(ePATHSPEC.TERRAINPATH)))
      {
        Directory.CreateDirectory(Paths.GetPath(ePATHSPEC.TERRAINPATH));
      }

      TileX++;
      Setup();
      if (!Directory.Exists(Paths.GetPath(ePATHSPEC.TERRAINPATH)))
      {
        Directory.CreateDirectory(Paths.GetPath(ePATHSPEC.TERRAINPATH));
      }
      TileZ++;
      Setup();
      if (!Directory.Exists(Paths.GetPath(ePATHSPEC.TERRAINPATH)))
      {
        Directory.CreateDirectory(Paths.GetPath(ePATHSPEC.TERRAINPATH));
      }

      TileX--;
      Setup();
      if (!Directory.Exists(Paths.GetPath(ePATHSPEC.TERRAINPATH)))
      {
        Directory.CreateDirectory(Paths.GetPath(ePATHSPEC.TERRAINPATH));
      }
      TileZ--;
      Setup();
    }

    void GetContainers()
    {
      TerrainContainer = GameObject.Find("Terrain");
      TerrainProto = GameObject.Find("TerrainProto");
    }

    IEnumerator GenerateRange()
    {
      int itx = TileX;
      int itz = TileZ;
      //bExport = false;
      for (int xx = itx - RangeSize; xx <= itx + RangeSize; xx++)
      {
        for (int zz = itz - RangeSize; zz <= itz + RangeSize; zz++)
        {
          TileX = xx;
          TileZ = zz;
          TileDone = 0;
          TileDone++;
          Debug.Log("Doing " + TileX + "," + TileZ);
          EditorCoroutine.StartCoroutine(GenerateAll());
          while (TileDone > 0)
          {
            // Debug.Log(TileDone);
            this.Repaint();
            yield return new WaitForSeconds(1f);
          }
          Debug.Log("Done " + TileX + "," + TileZ);
        }
      }

      if (bExport)
      {
        Debug.Log("Exporting AssetBundles");
        ExportAssetBundle();
      }
    }

    //Generate a mesh terrain, prefab, and AssetBundle
    IEnumerator GenerateAll()
    {
      Done = 0;
      CreateFolders();
      GetContainers();

      DownloadBasemap();

      Done = 0;
      DownloadHeightMap();
      while (HeightmapDone > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }

      /*
      Debug.Log("Downloading Neighbours");
      DownloadHeightmapNeighbours();
      while ((NeighbourDone > 1 ) && (HeightmapDone > 1))
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }
      */

      Debug.Log("///Downloaded Neighbours");
      CreateContiuousTiles();

      Done = 0;
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
      GetGeoInfo();
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

      Done = 0;
      GeneratePrefab();
      while (Done > 0)
      {
        this.Repaint();
        yield return new WaitForSeconds(0.1f);
      }

      if (bExport)
      {
        // ExportAssetBundle();
      }

      TileDone--;
      Repaint();
    }

    void GeneratePrefab()
    {
      Debug.Log("Generating Prefab");

      string ShortName = TileX + "_" + TileZ;

      UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(Paths.GetPath(ePATHSPEC.PREFABPATH));
      PrefabUtility.ReplacePrefab(TerrainContainer, prefab, ReplacePrefabOptions.ConnectToPrefab);

      string path = AssetDatabase.GetAssetPath(prefab);
      AssetImporter assetImporter = AssetImporter.GetAtPath(path);
      assetImporter.assetBundleName = ZoomLevel + "/" + TileZ + "/" + ShortName + ".unity3d";
      Debug.Log("Generated Prefab");

      CopyThumbnails();
      Done--;
    }

    void CopyThumbnails()
    {
      //string path = "Assets/_Massive/AssetBundles";     
      string path = ExportLocation + "\\" + ZoomLevel + "\\" + TileZ + "\\";
      Directory.CreateDirectory(path);

      string srcimgpath = Paths.GetPath(ePATHSPEC.TERRAINPATH) + "/texture.png";
      string targetimgpath = path + TileX + "_" + TileZ + ".png";

      srcimgpath = srcimgpath.Replace("/", "\\");
      if (File.Exists(targetimgpath))
      {
        File.Delete(targetimgpath);
      }
      File.Copy(srcimgpath, targetimgpath);
      Debug.Log("Exporting Thumbnail " + targetimgpath);
    }

    void ExportAssetBundle()
    {
      Done++;

      string path = ExportLocation;
      Directory.CreateDirectory(path);

      //UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetPath(ePATHSPEC.PREFABPATH));
      //string assetpath = AssetDatabase.GetAssetPath(prefab);
      //AssetImporter assetImporter = AssetImporter.GetAtPath(assetpath);
      //assetImporter.assetBundleName = TileX + "_" + TileZ + ".unity3d";

      BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

      //assetImporter.assetBundleName = "";
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

    //TODO: Skip OBJ step and generate directly to .asset
    void GenerateMeshTerrain()
    {
      Done++;
      Debug.Log("Generating Mesh Terrain for " + TileX + "," + TileZ);
      AssetDatabase.Refresh();
      GetContainers();
      CreateFolders();

      Material m = new Material(Shader.Find("Standard"));
      m.EnableKeyword("_BUMPMAP");
      //_DETAIL_MULX2
      m.color = Color.white;
      m.SetFloat("_SpecularHighlights", 0f);
      m.SetFloat("_GlossyReflections", 0f);

      Texture2D t2 = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.GetPath(ePATHSPEC.TEXTUREPATH));
      m.mainTexture = t2;
      Texture2D n1 = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.GetPath(ePATHSPEC.NORMALMAPPATH));
      m.SetTexture("_BumpMap", n1);
      m.SetFloat("_Glossiness", 0);

      AssetDatabase.CreateAsset(m, Paths.GetPath(ePATHSPEC.MATERIALPATH));
      AssetDatabase.Refresh();

      //TerrainSettings tset = TerrainProto.GetComponent<TerrainSettings>();

      string hpath = Paths.GetPath(ePATHSPEC.HEIGHTMAPCONTPATH);
      Debug.Log("Instantiating HeightmapCont:" + TileX + "," + TileZ);
      HeightMap = Instantiate(AssetDatabase.LoadAssetAtPath<Texture2D>(hpath));

      string outpath = Paths.GetPath(ePATHSPEC.MESHOBJPATH);
      DRect TileSize = MapTools.TileBoundsInMeters(new DVector3(TileX, 0, TileZ), ZoomLevel);
      //ExportOBJ.ExportFromHeight(HeightMap, outpath, TileSize);      
      //ExportOBJ.ExportMapZenHeight(HeightMap, outpath, TileSize.Size);      
      DVector3 TileLatLon = MapTools.TileToWorldPos(new DVector3(TileX, 0, TileZ), ZoomLevel);

      TerrainContainer = new GameObject();
      MassiveTile tile = TerrainContainer.AddComponent<MassiveTile>();
      ExportOBJ.ExportMapZenHeight(tile, HeightMap, outpath, TileSize.Size, TileLatLon);

      AssetDatabase.Refresh();
      AssetDatabase.ImportAsset(Paths.GetPath(ePATHSPEC.MESHOBJPATH));
      GameObject asset = (GameObject)AssetDatabase.LoadMainAssetAtPath(Paths.GetPath(ePATHSPEC.MESHOBJPATH));
      GameObject obj = Instantiate(asset);

      //Swap OBJ for .Asset to preserve modifications
      Transform tr = obj.transform.Find("default");
      MeshFilter mf = tr.GetComponent<MeshFilter>();
      Mesh nm = Instantiate(mf.sharedMesh);
      AssetDatabase.CreateAsset(nm, Paths.GetPath(ePATHSPEC.MESHASSETPATH));
      AssetDatabase.ImportAsset(Paths.GetPath(ePATHSPEC.MESHASSETPATH));
      AssetDatabase.Refresh();
      Mesh masset = (Mesh)AssetDatabase.LoadMainAssetAtPath(Paths.GetPath(ePATHSPEC.MESHASSETPATH));

      GameObject ma = new GameObject("terrain");
      MeshFilter tmf = ma.AddComponent<MeshFilter>();
      tmf.mesh = masset;
      ma.AddComponent<MeshRenderer>();
      ma.transform.parent = TerrainContainer.transform;

      GameObject.DestroyImmediate(obj);
      AssetDatabase.Refresh();

      //set position
      Vector3 tm = MapTools.TileToMeters(TileX, TileZ, ZoomLevel).ToVector3();
      Vector3 sp = MapTools.TileToMeters((int)StartPostion.x, (int)StartPostion.z, ZoomLevel).ToVector3();
      DVector3 dif = new DVector3(TileX, 0, TileZ) - StartPostion;
      TerrainContainer.transform.position = new Vector3(tm.x - sp.x, 0, sp.z - tm.z);

      TerrainContainer.name = TileX + "_" + "0" + "_" + TileZ + "_" + ZoomLevel;
      GameObject TerGo = TerrainContainer.transform.Find("terrain").gameObject;
      MeshRenderer mr = TerGo.GetComponent<MeshRenderer>();
      TerGo.AddComponent<MeshCollider>();
      TerGo.layer = LayerMask.NameToLayer("Terrain");

      tile.ZoomLevel = ZoomLevel;
      tile.SetTileIndex(new Vector3(TileX, 0, TileZ), ZoomLevel);

      Material[] temp = new Material[1];
      //temp[0] = Instantiate((Material)AssetDatabase.LoadAssetAtPath<Material>(path + ".mat"));            
      temp[0] = (Material)AssetDatabase.LoadAssetAtPath<Material>(Paths.GetPath(ePATHSPEC.MATERIALPATH));
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
  }
}