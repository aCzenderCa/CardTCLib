using System.IO;
using NLua;

namespace CardTCLib.LuaLib;

public static class LuaLib
{
    public static void OpenLib(Lua lua)
    {
        var sLuaBridge1 = new StreamReader(typeof(MainRuntime).Assembly.GetManifestResourceStream("lua_bridge1")!)
            .ReadToEnd();
        lua.DoString(sLuaBridge1, nameof(sLuaBridge1));
    }
}