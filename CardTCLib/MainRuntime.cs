﻿using System.IO;
using System.Text;
using BepInEx;
using CardTCLib.Const;
using CardTCLib.LuaBridge;
using CardTCLib.Patch;
using HarmonyLib;
using ModLoader;
using NLua;

namespace CardTCLib;

[BepInPlugin("zender.CardTCLib.MainRuntime", "CardTCLib", "1.0.8")]
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

        SetupLuaEnv();
        // 在这注册modloader的事件

        ModLoader.ModLoader.OnLoadMod += OnModLoaderSetup;
        ModLoader.ModLoader.OnLoadModComplete += () =>
        {
            foreach (var (_, uniqueIDScriptable) in ModLoader.ModLoader.AllGUIDDict)
            {
                var uniqueIdObjectBridge = new UniqueIdObjectBridge(uniqueIDScriptable);
                var nameChinese = uniqueIdObjectBridge.NameChinese;
                if (!string.IsNullOrEmpty(nameChinese))
                {
                    uniqueIDScriptable.name += $"_{nameChinese}";
                }
            }
        };
    }

    private static void SetupLuaEnv()
    {
        LuaEnv.State.Encoding = Encoding.UTF8;
        LuaEnv.LoadCLRPackage();

        LuaEnv["Events"] = Events;
        LuaEnv["CoroutineHelper"] = CoroutineHelper;
        LuaEnv["Game"] = Game;
    }

    // modloader 加套事件系统
    private static void OnModLoaderSetup(string directory)
    {
        var setupScriptPath = Path.Combine(directory, TCSpecialModPaths.SetupLuaScripts);
        if (!Directory.Exists(setupScriptPath)) return;

        foreach (var script in Directory.EnumerateFiles(setupScriptPath, "*.lua", SearchOption.AllDirectories))
        {
            LuaEnv.DoString(File.ReadAllText(script, Encoding.UTF8));
        }
    }
}