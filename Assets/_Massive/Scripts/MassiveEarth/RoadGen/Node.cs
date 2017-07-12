using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  public class Node
  {
    public string NodeName;
    public Vector3 pos;
    public int IntersectionCount = 0;
    public enum eNodeTypes { STRAIGHT, TJUNCTION, ELBOWJUNCTION, INTERSECTION };
    public eNodeTypes type = eNodeTypes.STRAIGHT;
    List<Node> IntersectingNodes = new List<Node>();

    public Node(Vector3 ipos, string iname = "")
    {
      pos = ipos;
      NodeName = iname;
    }

    public void AddIntersection(Node n2, eNodeTypes nType) 
    {
      if ( !IntersectingNodes.Contains(n2)) { 
        IntersectionCount++;
        type = nType;
        IntersectingNodes.Add(n2);
      }
    }

    public bool CheckIntersection(Node n2)
    {
      return IntersectingNodes.Contains(n2);
    }

    public void Visualize(GameObject parent, Vector3 Offset)
    {
      GameObject pin;
      if ( type == eNodeTypes.STRAIGHT) { 
      pin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pin.transform.localScale = new Vector3(5, 50, 5);
      }
      else
      {
        pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pin.transform.localScale = new Vector3(20, 20, 20);
      }
      GameObject.DestroyImmediate(pin.GetComponent<Collider>());
      
      pin.transform.position = pos + Offset;
      pin.transform.parent = parent.transform;

      NodeInfo ni = pin.AddComponent<NodeInfo>();
      ni.Type = type;
      ni.IntersectionCount = IntersectionCount;
      foreach( Node n in IntersectingNodes)
      {
        ni.Info += ">" + n.NodeName;
      }
      
    }
  }
}
