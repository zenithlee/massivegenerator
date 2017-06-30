using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;
using System.Collections.Generic;

/*
 mapzen 
mapzen-7LsTf3t
 mapbox 
pk.eyJ1Ijoic2VudGllbnR6ZW5pdGgiLCJhIjoiY2o0OGloaHduMGl4ajM0cXpwdWxxbHJjZSJ9.0MuHQYnhEeEEzXSyOzHcww
-33.9258
18.42322
 */

public class KeyProvider {

  List<string> ServiceMaps = new List<string>();
  

  string ServiceMap;


  public KeyProvider()
  {
    string s = File.ReadAllText("keys/keys.json");
    JSONNode node = JSON.Parse(s);
    foreach( JSONNode n in node.Children )
    {
     // Debug.Log(n["key"]);
      ServiceMaps.Add(n["key"]);
    }

  }

  public void TestKey()
  {
    for ( int i=0; i< 10; i++)
    {
      Debug.Log(GetKey());      
    }
  }

  public static string GetMapZenKey()
  {
    return "mapzen-7LsTf3t";
  }

  public static string GetMapBoxKey()
  {
    return "pk.eyJ1Ijoic2VudGllbnR6ZW5pdGgiLCJhIjoiY2o0OGloaHduMGl4ajM0cXpwdWxxbHJjZSJ9.0MuHQYnhEeEEzXSyOzHcww";
  }

  public string GetKey()
  {
    int keyindex = (int)Random.Range(0, ServiceMaps.Count);
    ServiceMap = (string)ServiceMaps[keyindex];
    //return "";
    return ServiceMap;
  }  
}
