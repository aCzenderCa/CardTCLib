using BepInEx;
using CardTCLib.Patch;
using HarmonyLib;

namespace CardTCLib;

[BepInPlugin("zender.CardTCLib.MainRuntime", "CardTCLib", "1.0.0")]
[BepInDependency("Dop.plugin.CSTI.ModLoader")]
public class MainRuntime : BaseUnityPlugin
{
    private static readonly Harmony HarmonyInstance = new("zender.CardTCLib.MainRuntime");

    private void Awake()
    {
        HarmonyInstance.PatchAll(typeof(EffectWithSlotPatch));
    }
}