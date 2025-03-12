using System.Collections.Generic;
using NLua;

namespace CardTCLib.LuaBridge;

public static class LuaTableUtils
{
    public static T? GetObj<T>(this LuaTable? table, string key)
    {
        var val = table?[key];
        if (val is T result) return result;
        return default;
    }

    public static bool GetBool(this LuaTable? table, string key)
    {
        var val = table?[key];
        if (val is bool result) return result;
        return val != null;
    }

    public static double GetNum(this LuaTable? table, string key)
    {
        var val = table?[key];
        if (val is long l)
            return l;
        if (val is double d)
            return d;

        return 0;
    }

    public static IEnumerable<(int idx, T val)> Ipairs<T>(this LuaTable table)
    {
        var i = 1;
        var val = table[i];
        while (val != null)
        {
            if (val is T t)
            {
                yield return (i - 1, t);
            }

            i += 1;
            val = table[i];
        }
    }

    public static IEnumerable<(object key, object val)> Pairs(this LuaTable table)
    {
        foreach (var key in table.Keys)
        {
            yield return (key, table[key]);
        }
    }
}