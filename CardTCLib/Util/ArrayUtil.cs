using System.Collections;

namespace CardTCLib.Util;

public static class ArrayUtil
{
    public static bool IsNullOrEmpty<T>(this T? array)
        where T : IList
    {
        if (array == null || array.Count == 0) return true;
        return false;
    }
}