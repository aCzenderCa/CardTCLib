using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CardTCLib.Util;

public static class ArrayUtil
{
    public static bool IsNullOrEmpty<T>(this T? array)
        where T : IList
    {
        if (array == null || array.Count == 0) return true;
        return false;
    }

    public static IEnumerable<T> RemoveAtSpecial<T>(this IEnumerable<T> array, int index)
    {
        var i = 0;
        foreach (var t in array)
        {
            if (i != index)
            {
                yield return t;
            }

            i++;
        }
    }

    public static T[] RemoveAtOnArr<T>(this T[] array, int index)
    {
        return array.RemoveAtSpecial(index).ToArray();
    }
}