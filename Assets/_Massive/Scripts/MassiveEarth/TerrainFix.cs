using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFix  {
  
  public void Fix(GameObject go) {
    Mesh m = go.transform.Find("terrain").GetComponent<MeshFilter>().sharedMesh;
    List<Vector3> l = new List<Vector3>();
    m.GetVertices(l);
    int[] triangles = m.triangles;    
    MeshCollider mc = go.transform.Find("terrain").GetComponent<MeshCollider>();
    

    for ( int i=0; i<l.Count; i++)
    {
      Vector3 v = l[i];

      Ray ray = new Ray(new Vector3(v.x, 9000, v.z) + go.transform.position, -Vector3.up);      

      LayerMask mask = LayerMask.GetMask("Roads", "Water", "Buildings"); 
      RaycastHit[] hits = Physics.SphereCastAll(ray, 14, 11000, mask);
      if ( hits != null )
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

          foreach ( RaycastHit hit in hits)
        {        
          if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
          {
            if (hit.collider.gameObject.name == "ocean")
            {
              v.y = hit.point.y - 5;
            }
            else
            {
              v.y = hit.point.y - 2;
            }
            l[i] = v;
          }

          if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Roads"))
          {
            if (v.y > hit.point.y)
            {
              v.y = lowest - 0.75f;
            }
            l[i] = v;
          }

          if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Buildings"))
          {            
            v.y = lowest - 0.75f;           
            l[i] = v;
          }
        }
         
      }
    }

    m.SetVertices(l);    
  }
 
}
