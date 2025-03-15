using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using CardTCLib.Const;
using CardTCLib.LuaBridge;
using CardTCLib.Patch;
using HarmonyLib;
using NLua;
using UnityEngine;

namespace CardTCLib;

[BepInPlugin("zender.CardTCLib.MainRuntime", "CardTCLib", "1.1.7")]
[BepInDependency("Dop.plugin.CSTI.ModLoader")]
public class MainRuntime : BaseUnityPlugin
{
    private static readonly Harmony HarmonyInstance = new("zender.CardTCLib.MainRuntime");
    public static readonly Lua LuaEnv = new();
    public static readonly CoroutineHelper CoroutineHelper = new(LuaEnv);
    public static readonly Events Events = new();
    public static readonly GameBridge Game = new();

    static MainRuntime()
    {
        HarmonyInstance.PatchAll(typeof(EffectWithSlotPatch));
        HarmonyInstance.PatchAll(typeof(TCEffectCardPatch));
        HarmonyInstance.PatchAll(typeof(UtilsPatch));

        HarmonyInstance.PatchAll(typeof(NpcActionPatch));
        HarmonyInstance.PatchAll(typeof(NpcCommonPatch));

        HarmonyInstance.PatchAll(typeof(CardPatch_0));
        HarmonyInstance.PatchAll(typeof(GamePatch_0));

        SetupLuaEnv();
        // 在这注册modloader的事件

        ModLoader.ModLoader.OnLoadMod += OnModLoaderSetup;
    }

    private static void SetupLuaEnv()
    {
        LuaEnv.State.Encoding = Encoding.UTF8;
        LuaEnv.LoadCLRPackage();

        LuaEnv["Events"] = Events;
        LuaEnv["CoroutineHelper"] = CoroutineHelper;
        LuaEnv["Game"] = Game;

        LuaLib.LuaLib.OpenLib(LuaEnv);
    }

    // modloader 加套事件系统
    private static void OnModLoaderSetup(string directory)
    {
        var setupScriptPath = Path.Combine(directory, TCSpecialModPaths.SetupLuaScripts);
        if (!Directory.Exists(setupScriptPath)) return;

        foreach (var script in Directory.EnumerateFiles(setupScriptPath, "*.lua", SearchOption.AllDirectories)
                     .OrderBy(Path.GetFileName))
        {
            LuaEnv.DoString(File.ReadAllText(script, Encoding.UTF8), Path.GetFileName(script));
        }
    }
}