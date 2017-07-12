using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Massive
{

  public class TerrainFix
  {

    int WaterLayer = 0;
    int RoadLayer = 0;
    int BuildingLayer = 0;

#if UNITY_EDITOR
    public void CreateHeightmapContinuity(int TileX, int TileZ)
    {      
      int orx = Paths.TileX;
      int orz = Paths.TileZ;

      string path = Paths.GetPath(ePATHSPEC.HEIGHTMAPATH);
      Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

      Paths.TileX++;
      path = Paths.GetPath(ePATHSPEC.HEIGHTMAPATH);
      Texture2D tX = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      if (tX == null )
      {
        Debug.LogError("Don't have heightmap tile :" + Paths.TileX + "," + Paths.TileZ);
        return;
      }
      Paths.TileX--;

      Paths.TileZ++;
      path = Paths.GetPath(ePATHSPEC.HEIGHTMAPATH);
      Texture2D tZ = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      if (tZ == null)
      {
        Debug.LogError("Don't have tile :" + Paths.TileX + "," + Paths.TileZ);
        return;
      }
      Paths.TileZ--;

      Paths.TileX++;
      Paths.TileZ++;
      path = Paths.GetPath(ePATHSPEC.HEIGHTMAPATH);
      Texture2D tXZ = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      if (tXZ == null)
      {
        Debug.LogError("Don't have tile :" + Paths.TileX + "," + Paths.TileZ);
        return;
      }
      Paths.TileX--;
      Paths.TileZ--;

      //Texture2D coordinate space is 0,0 =  Left,Bottom
      Texture2D tc = new Texture2D(t.width+1, t.height+1, t.format, false);
      //place the centre block in the top left of the target
      tc.SetPixels(0, 1, t.width,t.height, t.GetPixels());

      //get left edge of Tx
      Color[] colors = tX.GetPixels(0, 0, 1, tX.height);
      //set the right edge
      tc.SetPixels(tc.width-1, 1, 1, t.height, colors);
      
      //get top edge of tz
      colors = tZ.GetPixels(0, t.height -1, tX.width, 1);
      //set the bottom edge
      tc.SetPixels(0, 0, tX.width, 1, colors);


      //get top left pixel of txz
      colors = tXZ.GetPixels(0, tXZ.height - 1, 1, 1);
      //set the bottom right pixel
      tc.SetPixels(tc.width-1, 0, 1, 1, colors);

      //tc.SetPixel(0, 0, Color.red);

      path = Paths.GetPath(ePATHSPEC.HEIGHTMAPCONTPATH);
      //AssetDatabase.CreateAsset(tc, path);
      File.WriteAllBytes(path, tc.EncodeToPNG());
      AssetDatabase.Refresh();
      TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
      importer.textureType = TextureImporterType.Default;
      importer.wrapMode = TextureWrapMode.Repeat;
      importer.textureCompression = TextureImporterCompression.Uncompressed;
      importer.filterMode = FilterMode.Point;
      importer.isReadable = true;
      importer.mipmapEnabled = false;
      AssetDatabase.WriteImportSettingsIfDirty(path);
      AssetDatabase.SaveAssets();

      AssetDatabase.Refresh();
      Debug.Log("Created Continuity for Tile :" + Paths.TileX + "," + Paths.TileZ);

    }
#endif

    public void Fix(GameObject go)
    {

      WaterLayer = LayerMask.NameToLayer("Water");
      RoadLayer = LayerMask.NameToLayer("Roads");
      BuildingLayer = LayerMask.NameToLayer("Buildings");

      Mesh m = go.transform.Find("terrain").GetComponent<MeshFilter>().sharedMesh;
      List<Vector3> l = new List<Vector3>();
      m.GetVertices(l);
      int[] triangles = m.triangles;
      MeshCollider mc = go.transform.Find("terrain").GetComponent<MeshCollider>();

      for (int i = 0; i < l.Count; i++)
      {
        Vector3 v = l[i];

        Ray ray = new Ray(new Vector3(v.x, 9000, v.z) + go.transform.position, -Vector3.up);

        LayerMask mask = LayerMask.GetMask("Roads", "Water", "Buildings");
        RaycastHit[] hits = Physics.SphereCastAll(ray, 34, 11000, mask);
        if (hits != null)
        {
          float lowest = 99999;
          //Debug.Log(hit.collider.name);

          foreach (RaycastHit hit in hits)
          {
            if (hit.point.y < lowest)
            {
              lowest = hit.point.y;
            }
          }

          foreach (RaycastHit hit in hits)
          {
            if (hit.collider.gameObject.layer == WaterLayer)
            {
              if (hit.collider.gameObject.name == "ocean")
              {
                v.y = hit.point.y - 15;
              }
              else
              {
                v.y = hit.point.y - 2;
              }
              l[i] = v;
            }

            if (hit.collider.gameObject.layer == RoadLayer)
            {
              //v.y = hit.point.y - 0.6f;
              //l[i] = v;
            }

            if (hit.collider.gameObject.layer == BuildingLayer)
            {
              v.y = hit.point.y - 0.5f;
              l[i] = v;
            }
          }

        }
      }

      m.SetVertices(l);
    }

  }
}