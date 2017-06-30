using System.Collections;
using UnityEngine;

namespace Assets.Helpers
{
    public static class CoroutineStarter
    {
        private static readonly MonoBehaviour coroutineStarter;
        public static Coroutine StartCoroutine(IEnumerator function)
        {
            return coroutineStarter.StartCoroutine(function);
        }

        public static void StopCoroutine(IEnumerator function)
        {
            if (function != null)
            {
                coroutineStarter.StopCoroutine(function);
            }
        }

        public static void StopCoroutine(Coroutine function)
        {
            if (function != null)
            {
                coroutineStarter.StopCoroutine(function);
            }
        }

        static CoroutineStarter()
        {
            coroutineStarter = new GameObject("CoroutineStarter").AddComponent<MonoBehaviour>();
            Object.DontDestroyOnLoad(coroutineStarter.gameObject);
        }
    }
}