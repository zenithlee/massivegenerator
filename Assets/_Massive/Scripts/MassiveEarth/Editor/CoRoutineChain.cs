using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Massive
{

  public class CoroutineChain
  {
    List<bool> _subTasks = new List<bool>();

    private readonly EditorWindow _owningComponent;

    public CoroutineChain(EditorWindow owningComponent)
    {
      _owningComponent = owningComponent;
    }

    public void StartSubtask(IEnumerator routine)
    {
      _subTasks.Add(false);
      EditorCoroutine.StartCoroutine(StartJoinableCoroutine(_subTasks.Count - 1, routine));
    }

    public EditorCoroutine WaitForAll()
    {
      return EditorCoroutine.StartCoroutine(WaitForAllSubtasks());
    }

    private IEnumerator WaitForAllSubtasks()
    {
      while (true)
      {
        bool completedCheck = true;
        for (int i = 0; i < _subTasks.Count; i++)
        {
          if (_subTasks == null)
          {
            completedCheck = false;
            break;
          }
        }

        if (completedCheck)
        {
          break;
        }
        else
        {
          yield return null;
        }
      }
    }

    private IEnumerator StartJoinableCoroutine(int index, IEnumerator coroutine)
    {
      yield return EditorCoroutine.StartCoroutine(coroutine);
      _subTasks[index] = true;
    }
  }
}