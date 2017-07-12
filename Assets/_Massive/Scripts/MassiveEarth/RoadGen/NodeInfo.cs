using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Massive
{

  public class NodeInfo : MonoBehaviour
  {

    public Node.eNodeTypes Type = Node.eNodeTypes.STRAIGHT;
    public int IntersectionCount = 0;
    [Multiline(10)]
    public string Info;

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