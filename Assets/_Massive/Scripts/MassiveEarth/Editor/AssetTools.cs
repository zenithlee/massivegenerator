using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace _Massive
{
  public class AssetTools : MonoBehaviour
  {

    public static void WriteMeshAssets(GameObject go, string path)
    {
      if (File.Exists(path))
      {
        //Directory.CreateDirectory(path);
        File.Delete(path);
      }

      MeshFilter[] mfs = go.transform.GetComponentsInChildren<MeshFilter>();

      foreach( MeshFilter mf in mfs )
      {
        if (File.Exists(path))
        {
          AssetDatabase.AddObjectToAsset(mf.sharedMesh, path);
        }
        else
        {
          AssetDatabase.CreateAsset(mf.sharedMesh, path);
          AssetDatabase.Refresh();
        }
      }
    }

  }
}