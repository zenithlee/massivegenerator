using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassiveFeature : MonoBehaviour {

  [Multiline(10)]
  public string properties;
  [Multiline(10)]
  public string segments;

  public string featurename;
  public string kind;
  public string type;

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
      if (dproperties[s] != null ) {
        string val = dproperties[s].ToString();
        properties += s + "=" + val + "\n";
      }
    }
    
    if ( dproperties["name"] != null )
    {
      featurename = dproperties["name"].ToString();
    }
    if (dproperties["kind"] != null)
    {
      kind = dproperties["kind"].ToString();
    }
  }

  public void SetSegments(List<List<Vector3>> lsegments)
  {
    foreach( List<Vector3> list in lsegments)
    {
      segments += "list\n";
      foreach (Vector3 s in list)
      {
        segments += s.ToString() + "\n";
      }
    }
    
  }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
