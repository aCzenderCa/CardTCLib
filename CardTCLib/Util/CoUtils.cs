using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace CardTCLib.Util;

public static class CoUtils
{
    public static List<IEnumerator>? CacheEnumerators;

    public static Coroutine? CollectCacheAndStart()
    {
        var collectCache = CollectCache();
        if (collectCache == null) return null;
        return StartCoWithBlockAction(collectCache);
    }

    public static IEnumerator? CollectCache()
    {
        if (CacheEnumerators == null) return null;
        var cacheEnumerators = CacheEnumerators;
        CacheEnumerators = null;
        return _CollectCache(cacheEnumerators);
    }

    private static IEnumerator _CollectCache(List<IEnumerator> cacheEnumerators)
    {
        if (CacheEnumerators == null) yield break;
        foreach (var cacheEnumerator in cacheEnumerators)
        {
            while (cacheEnumerator.MoveNext())
            {
                yield return cacheEnumerator.Current;
            }
        }
    }

    public static Coroutine? StartCoWithBlockAction(IEnumerator? enumerator)
    {
        if (enumerator == null) return null;
        if (CacheEnumerators != null)
        {
            CacheEnumerators.Add(enumerator);
        }
        else
        {
            return GameManager.Instance.StartCoroutine(CoWithBlockAction(enumerator));
        }

        return null;
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

    public static IEnumerator OnEnumerator(this IEnumerator enumerator, Func<object?>? onstart = null,
        Func<object?>? onfinish = null)
    {
        var cur = onstart?.Invoke();
        if (cur != null) yield return cur;
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }

        cur = onfinish?.Invoke();
        if (cur != null) yield return cur;
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