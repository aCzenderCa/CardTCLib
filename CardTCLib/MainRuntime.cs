using System.IO;
using BepInEx;
using CardTCLib.Const;
using CardTCLib.Patch;
using HarmonyLib;
using NLua;

namespace CardTCLib;

[BepInPlugin("zender.CardTCLib.MainRuntime", "CardTCLib", "1.0.3")]
[BepInDependency("Dop.plugin.CSTI.ModLoader")]
public class MainRuntime : BaseUnityPlugin
{
    private static readonly Harmony HarmonyInstance = new("zender.CardTCLib.MainRuntime");
    private static readonly Lua LuaEnv = new();

    static MainRuntime()
    {
        HarmonyInstance.PatchAll(typeof(EffectWithSlotPatch));
        HarmonyInstance.PatchAll(typeof(TCEffectCardPatch));
        HarmonyInstance.PatchAll(typeof(UtilsPatch));

        HarmonyInstance.PatchAll(typeof(NpcActionPatch));

        SetupLuaEnv();
        // 在这注册modloader的事件
    }

    private static void SetupLuaEnv()
    {
        LuaEnv.LoadCLRPackage();
    }

    // modloader 加套事件系统
    private static void OnModLoaderSetup(string directory)
    {
        var setupScriptPath = Path.Combine(directory, TCSpecialModPaths.SetupLuaScripts);
        if (!Directory.Exists(setupScriptPath)) return;

        foreach (var script in Directory.EnumerateFiles(setupScriptPath, "*.lua", SearchOption.AllDirectories))
        {
            LuaEnv.DoFile(script);
        }
    }
}