using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace CardTCLib.Util;

public static class CoUtils
{
    public static void StartCoWithBlockAction(IEnumerator? enumerator)
    {
        if (enumerator == null) return;
        GameManager.Instance.StartCoroutine(CoWithBlockAction(enumerator));
    }

    private static IEnumerator CoWithBlockAction(IEnumerator enumerator)
    {
        var s = Time.frameCount + new Random().Next().ToString();
        while (GameManager.Instance.QueuedCardActions.Count > 0 &&
               GameManager.Instance.QueuedCardActions.FirstOrDefault().Message != s)
        {
            yield return null;
        }

        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    public static IEnumerator OnEnumerator(this IEnumerator enumerator, Action? onstart = null, Action? onfinish = null)
    {
        onstart?.Invoke();
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        onfinish?.Invoke();
    }

    public static IEnumerator Then(this IEnumerator? enumerator1, IEnumerator? enumerator2)
    {
        if (enumerator1 != null)
            while (enumerator1.MoveNext())
            {
                yield return enumerator1.Current;
            }

        if (enumerator2 != null)
            while (enumerator2.MoveNext())
            {
                yield return enumerator2.Current;
            }
    }
}