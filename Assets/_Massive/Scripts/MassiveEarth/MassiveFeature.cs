using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  public class MassiveFeature : MonoBehaviour
  {

    [Multiline(10)]
    public string properties;
    [Multiline(10)]
    public string segments;

    public string featurename;
    public string kind;
    public string kind_detail;
    public string type;
    public string id;
    public float height = 0;
    public bool is_bridge = false;

    public void SetID(string nid)
    {
      id = nid;
    }

    public void SetKind(string nKind)
    {
      kind = nKind;
    }

    public void SetType(string nType)
    {
      type = nType;
    }

    public void SetProperties(IDictionary dproperties)
    {
      foreach (string s in dproperties.Keys)
      {
        if (dproperties[s] != null)
        {
          string val = dproperties[s].ToString();
          properties += s + "=" + val + "\n";
        }
      }
      featurename = OSMTools.GetProperty(dproperties, "name");
      kind = OSMTools.GetProperty(dproperties, "kind");
      kind_detail = OSMTools.GetProperty(dproperties, "kind_detail");
      height = OSMTools.GetFloatProperty(dproperties, "height");      
      is_bridge = OSMTools.GetBoolProperty(dproperties, "is_bridge");
    }

    public void SetSegments(List<List<Vector3>> lsegments)
    {
      foreach (List<Vector3> list in lsegments)
      {
        segments += "list\n";
        foreach (Vector3 s in list)
        {
          segments += s.ToString() + "\n";
        }
      }

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