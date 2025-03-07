﻿using BepInEx;
using CardTCLib.Patch;
using HarmonyLib;

namespace CardTCLib;

[BepInPlugin("zender.CardTCLib.MainRuntime", "CardTCLib", "1.0.2")]
[BepInDependency("Dop.plugin.CSTI.ModLoader")]
public class MainRuntime : BaseUnityPlugin
{
    private static readonly Harmony HarmonyInstance = new("zender.CardTCLib.MainRuntime");

    static MainRuntime()
    {
        HarmonyInstance.PatchAll(typeof(EffectWithSlotPatch));
        HarmonyInstance.PatchAll(typeof(TCEffectCardPatch));
        HarmonyInstance.PatchAll(typeof(UtilsPatch));
        
        HarmonyInstance.PatchAll(typeof(NpcActionPatch));
    }
}